use std::time::Duration;
use spacetimedb::{reducer, ReducerContext, ScheduleAt, Table, TimeDuration};
use crate::*;

#[reducer(init)]
pub fn init(ctx: &ReducerContext) {
    log::info!("Initializing...");

    ctx.db.map().insert(Map {id: 0, name: "Floor".to_string(), collider: floor_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Ramp".to_string(), collider: ramp_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Ramp2".to_string(), collider: ramp_2_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Platform".to_string(), collider: platform_collider() });
}

#[reducer(client_connected)]
pub fn connect(ctx: &ReducerContext) {
    if IsUnitTestModeEnabled(ctx) {
        return;
    }

    log::info!("{} just connected.", ctx.sender);

    let logged_out_player_option = ctx.db.logged_out_players().identity().find(ctx.sender);

    if logged_out_player_option.is_some() {
        let logged_out_player = logged_out_player_option.unwrap();
        ctx.db.logged_in_players().insert(logged_out_player);
        ctx.db.logged_out_players().identity().delete(ctx.sender);
    } 

    else {
        ctx.db.logged_in_players().insert(Player {id: 0, identity: ctx.sender, name: "Test Player".to_string() });
    }

}

#[reducer(client_disconnected)]
pub fn disconnect(ctx: &ReducerContext) {
    if IsUnitTestModeEnabled(ctx) {
        return;
    }

    let player = ctx.db.logged_in_players().identity().find(ctx.sender).expect("Player not found");

    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    let respawn_timer_option = ctx.db.respawn_timers().identity().find(ctx.sender); 

    if let Some(mut magician) = magician_option {
        cleanup_on_disconnect_or_death(ctx, &mut magician);

        let game_option = ctx.db.game().id().find(magician.game_id);
        if game_option.is_some() {
            let mut game = game_option.unwrap();
            if game.current_players > 0 {
                game.current_players -= 1;
                ctx.db.game().id().update(game);
            }
        }

        ctx.db.magician().identity().delete(player.identity);
    }

    else if let Some(respawn_timer) = respawn_timer_option {
        let game_option = ctx.db.game().id().find(respawn_timer.game_id);
        if game_option.is_some() {
            let mut game = game_option.unwrap();
            if game.current_players > 0 {
                game.current_players -= 1;
                ctx.db.game().id().update(game);
            }
        }

        ctx.db.respawn_timers().scheduled_id().delete(respawn_timer.scheduled_id); 
    }
    
    ctx.db.logged_in_players().identity().delete(player.identity);
    ctx.db.logged_out_players().insert(player);
    
    log::info!("{} just disconnected.", ctx.sender);
}

#[reducer]
pub fn handle_game_end(ctx: &ReducerContext, timer: GameTimersTimer) 
{ 
    log::info!("Game Ended With Id {}", timer.game_id);
    let game_id = timer.game_id;
    cleanup_on_game_end(ctx, game_id);
    ctx.db.game_timers().scheduled_id().delete(timer.scheduled_id);
    ctx.db.game().id().delete(game_id);
}

#[reducer]
pub fn try_join_game(ctx: &ReducerContext) 
{
    let player_option = ctx.db.logged_in_players().identity().find(ctx.sender);
    if let Some(player) = player_option {
        let mut game: Game = match ctx.db.game().in_progress().filter(false).next() {
            Some(existing_game) => existing_game,
            None => {
                let created_game = ctx.db.game().insert(Game { id: 0, max_players: 12, current_players: 1, in_progress: false });
                let tick_millis: u64 = 1000 / 60;
                let tick_rate: f32 = 1.0 / 60.0;

                ctx.db.move_all_magicians().insert(MoveAllMagiciansTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
                ctx.db.handle_magician_timers_timer().insert(HandleMagicianTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
                ctx.db.handle_magician_stateless_timers_timer().insert(HandleMagicianStatelessTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
                ctx.db.gravity_magician().insert(GravityTimerMagician { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, gravity: 20.0, game_id: created_game.id });
                ctx.db.player_effects_table_timer().insert(PlayerEffectsTableTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
                
                create_test_player(ctx, created_game.id);
                created_game
            }
        };

        game.current_players += 1;
        if game.current_players >= 2 && game.in_progress != true {
            game.in_progress = true;
            let end_time = ctx.timestamp.checked_add(TimeDuration::from_micros(600_000_000)).expect("Match End Time Timestamp Overflow"); // 10 Minutes
            let game_end_timer = GameTimersTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Time(end_time), game_id: game.id};
            ctx.db.game_timers().insert(game_end_timer);
        }

        let game_id = game.id;
        ctx.db.game().id().update(game);

        let magician_config = MagicianConfig {player, game_id: game_id, position: DbVector3 { x: 0.0, y: 0.0, z: 0.0 }};
        let magician = create_magician(magician_config);
        let inserted_magician = ctx.db.magician().insert(magician);

        let invincible_effect = create_invicible_effect(5.0);
        let effects = vec![invincible_effect];
        add_effects_to_table(ctx, effects, inserted_magician.id, inserted_magician.id, game_id);   
    } 
}


#[reducer]
pub fn try_leave_game(ctx: &ReducerContext) 
{
    let player_option = ctx.db.logged_in_players().identity().find(ctx.sender);
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    let respawn_timer_option = ctx.db.respawn_timers().identity().find(ctx.sender); 

    if let Some(player) = player_option {
        if let Some(mut magician) = magician_option {
            cleanup_on_disconnect_or_death(ctx, &mut magician);

            let game_option = ctx.db.game().id().find(magician.game_id);
            if game_option.is_some() {
                let mut game = game_option.unwrap();
                if game.current_players > 0 {
                    game.current_players -= 1;
                    ctx.db.game().id().update(game);
                }
            }

            ctx.db.magician().identity().delete(player.identity);
        }

        else if let Some(respawn_timer) = respawn_timer_option {
            let game_option = ctx.db.game().id().find(respawn_timer.game_id);
            if game_option.is_some() {
                let mut game = game_option.unwrap();
                if game.current_players > 0 {
                    game.current_players -= 1;
                    ctx.db.game().id().update(game);
                }
            }

            ctx.db.respawn_timers().scheduled_id().delete(respawn_timer.scheduled_id); 
        }
    }
}

#[reducer]
pub fn handle_respawn(ctx: &ReducerContext, timer: RespawnTimersTimer) 
{ 
    let player_option = ctx.db.logged_in_players().identity().find(timer.player.identity);
    if player_option.is_none() {
        ctx.db.respawn_timers().scheduled_id().delete(timer.scheduled_id);
        return;
    }

    let player = player_option.expect("Player Existence Already Confirmed!");
    let game_option = ctx.db.game().id().find(timer.game_id);
    if game_option.is_some() {
        let magician_config = MagicianConfig {player, game_id: timer.game_id, position: DbVector3 { x: 0.0, y: 0.0, z: 0.0 }};
        let magician = create_magician(magician_config);
        let inserted_magician = ctx.db.magician().insert(magician);

        let invincible_effect = create_invicible_effect(5.0);
        let effects = vec![invincible_effect];
        add_effects_to_table(ctx, effects, inserted_magician.id, inserted_magician.id, timer.game_id);       
    }
        
    ctx.db.respawn_timers().scheduled_id().delete(timer.scheduled_id);   
}
use spacetimedb::{reducer, ReducerContext, ScheduleAt, Table, TimeDuration};
use crate::*;

#[reducer(init)] // Init gets called at DB publish
pub fn init(ctx: &ReducerContext) // Adds map pieces to database with colliders (map pieces are static)
{
    log::info!("Initializing...");
    ctx.db.map().insert(Map {id: 0, name: "Floor".to_string(), collider: floor_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Ramp".to_string(), collider: ramp_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Ramp2".to_string(), collider: ramp_2_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Platform".to_string(), collider: platform_collider() });
}

#[reducer(client_connected)]
pub fn connect(ctx: &ReducerContext) // Adds player to logged_in_players and out of logged_out_players - Creates player if they don't exist in logged_out_players
{
    if IsUnitTestModeEnabled(ctx) { return; } // Cuts reducer short if we are unit testing to ensure persistent data while tests run

    let logged_out_player_option = ctx.db.logged_out_players().identity().find(ctx.sender);

    if let Some(logged_out_player) = logged_out_player_option {
        ctx.db.logged_in_players().insert(logged_out_player);
        ctx.db.logged_out_players().identity().delete(ctx.sender);
    } 

    else {
        ctx.db.logged_in_players().insert(Player {id: 0, identity: ctx.sender, name: "Test Player".to_string() });
    }

    log::info!("{} just connected.", ctx.sender);
}

#[reducer(client_disconnected)]
pub fn disconnect(ctx: &ReducerContext) // Cleans up data related to player - Cases: player is in match, dead & respawning, or in lobby
{
    if IsUnitTestModeEnabled(ctx) { return; } // Cuts reducer short if we are unit testing to ensure persistent data while tests run

    let player = ctx.db.logged_in_players().identity().find(ctx.sender).expect("Player not found");

    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    let respawn_timer_option = ctx.db.respawn_timers().identity().find(ctx.sender); 

    if let Some(mut magician) = magician_option { // Case: in match
        cleanup_on_disconnect_or_death(ctx, &mut magician);
        decrement_player_count_of_game(ctx, magician.game_id);
        ctx.db.magician().identity().delete(player.identity);
    }

    else if let Some(respawn_timer) = respawn_timer_option { // Case: dead & respawning
        decrement_player_count_of_game(ctx, respawn_timer.game_id);
        ctx.db.respawn_timers().scheduled_id().delete(respawn_timer.scheduled_id); 
    }
    
    ctx.db.logged_in_players().identity().delete(player.identity); // Case: In lobby but still executed for all cases
    ctx.db.logged_out_players().insert(player);
    
    log::info!("{} just disconnected.", ctx.sender);
}

#[reducer]
pub fn try_join_game(ctx: &ReducerContext) // Adds player to first unstarted game - Creates new game if all games are in progress
{
    let player_option = ctx.db.logged_in_players().identity().find(ctx.sender);
    if let Some(player) = player_option {
        let mut game: Game = match ctx.db.game().in_progress().filter(false).next() { // Finds unstarted game or creates new if none
            Some(mut existing_game) => { 
                existing_game.current_players += 1;
                existing_game 
            },
            None =>  { 
                let mut created_game = create_game(ctx);
                created_game.current_players += 1;
                created_game
            }
        };

        if game.current_players == game.max_players && game.in_progress != true { // Starts game if full - No new players can join, players can leave and rejoin (rejoin WIP)
            game.in_progress = true;
            let end_time = ctx.timestamp.checked_add(TimeDuration::from_micros(600_000_000)).expect("Match End Time Timestamp Overflow"); // 10 Minutes
            let game_end_timer = GameTimersTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Time(end_time), game_id: game.id};
            ctx.db.game_timers().insert(game_end_timer);
        }

        let effects = vec![create_invincible_effect(5.0)];
        let magician_config = MagicianConfig {player, game_id: game.id, position: DbVector3 { x: 0.0, y: 0.0, z: 0.0 }};
        let magician = create_magician(magician_config);

        let inserted_magician = ctx.db.magician().insert(magician);
        add_effects_to_table(ctx, effects, inserted_magician.id, inserted_magician.id, game.id);   

        ctx.db.game().id().update(game);
    } 
}

#[reducer]
pub fn try_leave_game(ctx: &ReducerContext) // Cleans up data related to player but only match related data - Similar functionality to disconnect
{
    let player_option = ctx.db.logged_in_players().identity().find(ctx.sender);
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    let respawn_timer_option = ctx.db.respawn_timers().identity().find(ctx.sender); 

    if let Some(player) = player_option {
        if let Some(mut magician) = magician_option {
            cleanup_on_disconnect_or_death(ctx, &mut magician);
            decrement_player_count_of_game(ctx, magician.game_id);
            ctx.db.magician().identity().delete(player.identity);
        }

        else if let Some(respawn_timer) = respawn_timer_option {
            decrement_player_count_of_game(ctx, respawn_timer.game_id);
            ctx.db.respawn_timers().scheduled_id().delete(respawn_timer.scheduled_id); 
        }
    }
}

#[reducer]
pub fn handle_respawn(ctx: &ReducerContext, timer: RespawnTimersTimer) // Respawns player into game
{ 
    let player_option = ctx.db.logged_in_players().identity().find(timer.player.identity); // Player not gauranteed to exist - Not sure if this is true, but case handled regardless
    if let Some(player) = player_option {
        let game_option = ctx.db.game().id().find(timer.game_id);
        if game_option.is_some() {
            let effects = vec![create_invincible_effect(5.0)];
            let magician_config = MagicianConfig {player, game_id: timer.game_id, position: DbVector3 { x: 0.0, y: 0.0, z: 0.0 }};
            let magician = create_magician(magician_config);

            let inserted_magician = ctx.db.magician().insert(magician);
            add_effects_to_table(ctx, effects, inserted_magician.id, inserted_magician.id, timer.game_id);       
        }
    }
        
    ctx.db.respawn_timers().scheduled_id().delete(timer.scheduled_id); 
}

#[reducer]
pub fn handle_game_end(ctx: &ReducerContext, timer: GameTimersTimer) // Cleans up data related to game - Data: magicians, effects, respawns, and scheduled reducers
{
    cleanup_on_game_end(ctx, timer.game_id);
    ctx.db.game_timers().scheduled_id().delete(timer.scheduled_id);
    ctx.db.game().id().delete(timer.game_id);
}
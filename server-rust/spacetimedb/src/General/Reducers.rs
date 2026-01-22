use std::time::Duration;
use spacetimedb::{reducer, ReducerContext, ScheduleAt, Table, Identity, Timestamp, TimeDuration};
use crate::*;

#[reducer(init)]
pub fn init(ctx: &ReducerContext) {
    log::info!("Initializing...");

    ctx.db.map().insert(Map {id: 0, name: "Floor".to_string(), collider: floor_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Ramp".to_string(), collider: ramp_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Ramp2".to_string(), collider: ramp_2_collider() });
    ctx.db.map().insert(Map {id: 0, name: "Platform".to_string(), collider: platform_collider() });

    let game = ctx.db.game().insert(Game {id: 0, max_players: 12, current_players: 1, in_progress: false });

    let tick_millis: u64 = 1000 / 60;
    let tick_rate: f32 = 1.0 / 60.0;

    ctx.db.move_all_magicians().insert(MoveAllMagiciansTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
    ctx.db.handle_magician_timers_timer().insert(HandleMagicianTimersTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
    ctx.db.handle_magician_stateless_timers_timer().insert(HandleMagicianStatelessTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
    ctx.db.gravity_magician().insert(GravityTimerMagician {scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, gravity: 20.0, game_id: game.id });
    ctx.db.player_effects_table_timer().insert(PlayerEffectsTableTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });

    ctx.db.magician().insert(Magician {
        identity: Identity::default(),
        id: 10000,
        name: "Test Magician".to_string(),
        game_id: game.id,
        position: DbVector3 { x: 0.0, y: 0.0, z: 5.0 },
        rotation: DbRotation2 { yaw: 180.0, pitch: 0.0 },
        velocity: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
        corrected_velocity: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
        collider: MagicianIdleCollider(),
        collision_entries: vec![CollisionEntry { entry_type: CollisionEntryType::Map, id: 1 }],
        is_colliding: false,
        kinematic_information: KinematicInformation { jump: false, falling: false, crouched: false, grounded: false, sprinting: false },
        combat_information: CombatInformation { health: 200.0, max_health: 200.0, speed_multiplier: 1.0, game_score: 0},
        state: MagicianState::Default,
        stateless_timers: vec![ StatelessTimer { name: "Tarot".to_string(), state: StatelessTimerState::Inactive, cooldown_time: 20.0, application_time: 0.0, current_time: 0.0} ],
        permissions: vec![
            PermissionEntry { key: "CanWalk".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanRun".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanJump".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanCrouch".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanAttack".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanReload".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanDust".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanCloak".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanHypnosis".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "CanTarot".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Stunned".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Dusted".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Taroted".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Cloaked".to_string(), subscribers: Vec::new() },
            PermissionEntry { key: "Hypnosised".to_string(), subscribers: Vec::new() },
        ], 
        timers: vec![ Timer { name: "Attack".to_string(), state: TimerState::Inactive, cooldown_time: 0.7, use_finished_time: 0.7, current_time: 0.0 },
            Timer { name: "Reload".to_string(), state: TimerState::Inactive, cooldown_time: 2.2, use_finished_time: 2.2, current_time: 0.0 },
            Timer { name: "Dust".to_string(), state: TimerState::Inactive, cooldown_time: 10.0, use_finished_time: 2.4, current_time: 0.0 },
            Timer { name: "Cloak".to_string(), state: TimerState::Inactive, cooldown_time: 20.0, use_finished_time: 1.5, current_time: 0.0 },
            Timer { name: "Hypnosis".to_string(), state: TimerState::Inactive, cooldown_time: 20.0, use_finished_time: 2.0, current_time: 0.0 },],
        bullets: Vec::new(),
        bullet_capacity: 8,
        effects: Vec::new()
    });
}

#[reducer(client_connected)]
pub fn connect(ctx: &ReducerContext) {
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

    let player = ctx.db.logged_in_players().identity().find(ctx.sender).expect("Player not found after insert/restore");
    let mut game: Game = match ctx.db.game().in_progress().filter(false).next() {
        Some(existing_game) => existing_game,
        None => {
            let created_game = ctx.db.game().insert(Game { id: 0, max_players: 12, current_players: 0, in_progress: false });

            let tick_millis: u64 = 1000 / 60;
            let tick_rate: f32 = 1.0 / 60.0;

            ctx.db.move_all_magicians().insert(MoveAllMagiciansTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
            ctx.db.handle_magician_timers_timer().insert(HandleMagicianTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
            ctx.db.handle_magician_stateless_timers_timer().insert(HandleMagicianStatelessTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });
            ctx.db.gravity_magician().insert(GravityTimerMagician { scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, gravity: 20.0, game_id: created_game.id });
            ctx.db.player_effects_table_timer().insert(PlayerEffectsTableTimer {scheduled_id: 0, scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: created_game.id });

            created_game
        }
    };


    game.current_players += 1;
    let game_id = game.id;
    ctx.db.game().id().update(game);

    let magician_config = MagicianConfig {player, game_id: game_id, position: DbVector3 { x: 0.0, y: 0.0, z: 0.0 }};
    let magician = create_magician(magician_config);
    ctx.db.magician().insert(magician);
}


#[reducer(client_disconnected)]
pub fn disconnect(ctx: &ReducerContext) {
    let player = ctx.db.logged_in_players().identity().find(ctx.sender).expect("Player not found");
    let magician_option = ctx.db.magician().identity().find(ctx.sender);

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
    
    ctx.db.logged_in_players().identity().delete(player.identity);
    ctx.db.logged_out_players().insert(player);
    
    log::info!("{} just disconnected.", ctx.sender);
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
        ctx.db.magician().insert(magician);
    }
        
    ctx.db.respawn_timers().scheduled_id().delete(timer.scheduled_id);
}

pub fn handle_magician_death(ctx: &ReducerContext, magician: &mut Magician) 
{
    let player_option = ctx.db.logged_in_players().identity().find(magician.identity);
    cleanup_on_disconnect_or_death(ctx, magician);
    
    if let Some(player) = player_option {
        let respawn_time = ctx.timestamp.checked_add(TimeDuration::from_micros(5_000_000)).expect("Respawn timestamp overflow");
        let respawn_timer = RespawnTimersTimer { scheduled_id: 0, scheduled_at: ScheduleAt::Time(respawn_time), game_id: magician.game_id, player };
        ctx.db.respawn_timers().insert(respawn_timer);
    }

    ctx.db.magician().id().delete(magician.id);
}
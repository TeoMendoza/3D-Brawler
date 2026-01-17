use std::time::Duration;
use spacetimedb::{reducer, ReducerContext, ScheduleAt, Table, Identity};
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
        state: MagicianState::Default,
        permissions: vec![PermissionEntry { key: "CanWalk".to_string(), subscribers: vec![] }, PermissionEntry { key: "CanRun".to_string(), subscribers: vec![] }, PermissionEntry { key: "CanJump".to_string(), subscribers: vec![] }, PermissionEntry { key: "CanCrouch".to_string(), subscribers: vec![] }, PermissionEntry { key: "CanAttack".to_string(), subscribers: vec![] }, PermissionEntry { key: "CanReload".to_string(), subscribers: vec![] }, PermissionEntry { key: "CanDust".to_string(), subscribers: Vec::new() },],
        timers: vec![Timer { name: "Attack".to_string(), state: TimerState::Inactive, cooldown_time: 0.7, use_finished_time: 0.7, current_time: 0.0 }, Timer { name: "Reload".to_string(), state: TimerState::Inactive, cooldown_time: 2.2, use_finished_time: 2.2, current_time: 0.0 }, Timer { name: "Dust".to_string(), state: TimerState::Inactive, cooldown_time: 10.0, use_finished_time: 1.0, current_time: 0.0 },],
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
    let magician = ctx.db.magician().identity().find(ctx.sender).expect("Magician not found");

    let game_option = ctx.db.game().id().find(magician.game_id);
    if game_option.is_some() {
        let mut game = game_option.unwrap();
        if game.current_players > 0 {
            game.current_players -= 1;
            ctx.db.game().id().update(game);
        }
    }

    ctx.db.magician().id().delete(magician.id);

    let collision_entry = CollisionEntry { entry_type: CollisionEntryType::Magician, id: magician.id };
    for mut other in ctx.db.magician().game_id().filter(magician.game_id) {
        if let Some(index) = other.collision_entries.iter().position(|entry| *entry == collision_entry) {
            other.collision_entries.swap_remove(index);
            ctx.db.magician().id().update(other);
        }
    }

    ctx.db.logged_in_players().identity().delete(player.identity);
    ctx.db.logged_out_players().insert(player);
    
    log::info!("{} just disconnected.", ctx.sender);
}
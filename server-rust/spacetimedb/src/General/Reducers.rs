use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;
use crate::General::Tables::*;
use crate::Magician::Tables::*;
use crate::Map::Tables::*;


#[spacetimedb::reducer(init)]
pub fn init(ctx: &ReducerContext) {
    log::info!("Initializing...");

    ctx.db.map().insert(Map { name: "Floor".to_string(), collider: FloorCollider });
    ctx.db.map().insert(Map { name: "Ramp".to_string(), collider: RampCollider });
    ctx.db.map().insert(Map { name: "Ramp2".to_string(), collider: Ramp2Collider });
    ctx.db.map().insert(Map { name: "Platform".to_string(), collider: PlatformCollider });

    let game = ctx.db.game().insert(Game {max_players: 12, current_players: 1, in_progress: false });

    let tick_millis: u64 = 1000 / 60;
    let tick_rate: f32 = 1.0 / 60.0;

    ctx.db.move_all_magicians_timer().insert(MoveAllMagiciansTimer {scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
    ctx.db.handle_magician_timers_timer().insert(HandleMagicianTimersTimer {scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
    ctx.db.gravity_magician_timer().insert(GravityTimerMagician {scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, gravity: 20, game_id: game.id });

    ctx.db.magician().insert(Magician {
        identity: Identity::default(),
        id: 10000,
        name: "Test Magician".to_string(),
        match_id: game.id,
        position: DbVector3 { x: 0.0, y: 0.0, z: 5.0 },
        rotation: DbRotation2 { yaw: 180, pitch: 0 },
        velocity: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
        corrected_velocity: DbVector3 { x: 0.0, y: 0.0, z: 0.0 },
        collider: MagicianIdleCollider,
        collision_entries: vec![CollisionEntry { entry_type: CollisionEntryType::Map, id: 1 }],
        is_colliding: false,
        kinematic_information: KinematicInformation { jump: false, falling: false, crouched: false, grounded: false, sprinting: false },
        state: MagicianState::Default,
        player_permission_config: vec![PermissionEntry { key: "CanWalk".to_string(), values: vec![] }, PermissionEntry { key: "CanRun".to_string(), values: vec![] }, PermissionEntry { key: "CanJump".to_string(), values: vec![] }, PermissionEntry { key: "CanCrouch".to_string(), values: vec![] }, PermissionEntry { key: "CanAttack".to_string(), values: vec![] }, PermissionEntry { key: "CanReload".to_string(), values: vec![] }],
        timers: vec![Timer { name: "Attack".to_string(), current_time: 0.7, reset_time: 0.7 }, Timer { name: "Reload".to_string(), current_time: 2.2, reset_time: 2.2 }],
        bullets: Vec::new(),
        bullet_capacity: 8,
    });
}

#[spacetimedb::reducer(client_connected)]
pub fn Connect(ctx: &ReducerContext) {
    log::info!("{} just connected.", ctx.sender);

    let logged_out_player_option = ctx.db.logged_out_players().identity().find(ctx.sender);

    if logged_out_player_option.is_some() {
        let logged_out_player = logged_out_player_option.unwrap();
        ctx.db.logged_in_players().insert(logged_out_player);
        ctx.db.logged_out_players().identity().delete(ctx.sender);
    } 

    else {
        ctx.db.logged_in_players().insert(Player { identity: ctx.sender, name: "Test Player".to_string() });
    }

    let player = ctx.db.logged_in_players().identity().find(ctx.sender).expect("Player not found after insert/restore");

    let mut game_list: Vec<Game> = ctx.db.game().in_progress().filter(false).collect();
    let mut game: Game;
    if game_list.len() > 0 {
        game = game_list[0];
    } 
    
    else {

        game = ctx.db.game().insert(Game {max_players: 12, current_players: 0, in_progress: false });
        let tick_millis: u64 = 1000 / 60;
        let tick_rate: f32 = 1.0 / 60.0;

        ctx.db.move_all_magicians_timer().insert(MoveAllMagiciansTimer {scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
        ctx.db.handle_magician_timers_timer().insert(HandleMagicianTimersTimer {scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, game_id: game.id });
        ctx.db.gravity_magician_timer().insert(GravityTimerMagician {scheduled_at: ScheduleAt::Interval(Duration::from_millis(tick_millis).into()), tick_rate, gravity: 20, game_id: game.id });
    }

    game.current_players += 1;
    ctx.db.game().id().update(game);

    let magician_config = MagicianConfig::new(player, game.id, DbVector3 { x: 0.0, y: 0.0, z: 0.0 });
    let magician = CreateMagician(magician_config);
    ctx.db.magician().insert(magician);
}


#[spacetimedb::reducer(client_disconnected)]
pub fn Disconnect(ctx: &ReducerContext) {
    let player = ctx.db.logged_in_players().identity().find(ctx.sender).expect("Player not found");
    let magician = ctx.db.magician().identity().find(ctx.sender).expect("Magician not found");

    let game_option = ctx.db.game().id().find(magician.match_id);
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
        if other.collision_entries.contains(&collision_entry) {
            other.collision_entries.retain(|entry| entry != &collision_entry);
            ctx.db.magician().id().update(other);
        }
    }

    ctx.db.logged_out_players().insert(player);
    ctx.db.logged_in_players().identity().delete(player.identity);

    log::info!("{} just disconnected.", ctx.sender);
}
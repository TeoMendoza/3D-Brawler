use spacetimedb::{table, Identity, ScheduleAt};
use crate::*;

#[table(name = magician, public)]
pub struct Magician {
    #[primary_key] pub identity: Identity,
    #[unique] #[auto_inc] pub id: u64,
    #[index(btree)] pub game_id: u32,
    pub name: String,
    pub position: DbVector3,
    pub rotation: DbRotation2,
    pub velocity: DbVector3,
    pub corrected_velocity: DbVector3,
    pub collider: ComplexCollider,
    pub collision_entries: Vec<CollisionEntry>,
    pub is_colliding: bool,
    pub state: MagicianState,
    pub kinematic_information: KinematicInformation,
    pub player_permission_config: Vec<PermissionEntry>,
    pub timers: Vec<Timer>,
    pub bullets: Vec<ThrowingCard>,
    pub bullet_capacity: i32,
}

#[table(name = move_all_magicians, scheduled(move_magicians))]
pub struct MoveAllMagiciansTimer {
    #[primary_key] #[auto_inc] pub scheduled_id: u64,
    pub scheduled_at: ScheduleAt,
    pub tick_rate: f32,
    pub game_id: u32,
}

#[table(name = gravity_magician, scheduled(apply_gravity_magician))]
pub struct GravityTimerMagician {
    #[primary_key] #[auto_inc] pub scheduled_id: u64,
    pub scheduled_at: ScheduleAt,
    pub tick_rate: f32,
    pub gravity: f32,
    pub game_id: u32,
}

#[table(name = handle_magician_timers_timer, scheduled(handle_magician_timers))]
pub struct HandleMagicianTimersTimer {
    #[primary_key] #[auto_inc] pub scheduled_id: u64,
    pub scheduled_at: ScheduleAt,
    pub tick_rate: f32,
    pub game_id: u32,
}

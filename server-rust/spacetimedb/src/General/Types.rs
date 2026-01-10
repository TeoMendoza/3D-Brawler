use spacetimedb::{SpacetimeType};

#[derive(SpacetimeType, Clone, Debug, Copy, Default)]
pub struct DbVector3 {
    pub x: f32,
    pub y: f32,
    pub z: f32
}

#[derive(SpacetimeType, Clone, Debug, Copy)]
pub struct DbRotation2 {
    pub yaw: f32, // Y axis, horizontal
    pub pitch: f32, // X axis, vertical
}

#[derive(SpacetimeType, Clone, Debug)]
pub struct PermissionEntry {
    pub key: String,
    pub subscribers: Vec<String>,
}

#[derive(SpacetimeType, Clone, Debug)]
pub struct MovementRequest {
    pub move_forward: bool,
    pub move_backward: bool,
    pub move_left: bool,
    pub move_right: bool,
    pub sprint: bool,
    pub jump: bool,
    pub crouch: bool,
    pub aim: DbRotation2,
}

#[derive(SpacetimeType, Clone, Debug)]
pub enum CharacterType {
    Magician,
}

#[derive(SpacetimeType, Clone, Debug)]
pub struct Timer {
    pub name: String,
    pub state: TimerState,
    pub cooldown_time: f32, // Seconds
    pub use_finished_time: f32, // Seconds
    pub current_time: f32, // Seconds
}

#[derive(SpacetimeType, Clone, Debug, Eq, PartialEq)]
pub enum TimerState {
    Inactive,
    InUse,
    InCooldown
}
use std::time::Duration;
use spacetimedb::{table, rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp, UniqueColumn};


#[derive(SpacetimeType, Clone, Debug, Copy)]
struct DbVector3 {
    pub x: f32,
    pub y: f32,
    pub z: f32
}

#[derive(SpacetimeType, Clone, Debug, Copy)]
struct DbRotation2 {
    pub yaw: f32, // Y axis, horizontal
    pub pitch: f32, // X axis, vertical
}

#[derive(SpacetimeType, Clone, Debug)]
struct PermissionEntry {
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
    pub reset_time: f32, // Seconds
    pub current_time: f32, // Seconds
}
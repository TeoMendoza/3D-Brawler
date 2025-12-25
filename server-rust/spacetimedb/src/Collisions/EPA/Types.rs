use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

#[spacetimedb::type]
pub struct CollisionContact {
    pub normal: DbVector3,
    pub penetration_depth: f32,
    pub collision_type: CollisionEntryType,
}

#[spacetimedb::type]
pub struct EpaFace {
    pub index_a: i32,
    pub index_b: i32,
    pub index_c: i32,
    pub normal: DbVector3,
    pub distance: f32,
    pub obsolete: bool,
}

#[spacetimedb::type]
pub struct EpaEdge {
    pub index_a: i32,
    pub index_b: i32,
    pub obsolete: bool,
}

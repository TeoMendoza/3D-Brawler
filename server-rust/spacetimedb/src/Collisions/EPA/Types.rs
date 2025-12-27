use spacetimedb::{SpacetimeType};
use crate::*;

#[derive(SpacetimeType)]
pub struct CollisionContact {
    pub normal: DbVector3,
    pub penetration_depth: f32,
    pub collision_type: CollisionEntryType,
}

#[derive(SpacetimeType)]
pub struct EpaFace {
    pub index_a: i32,
    pub index_b: i32,
    pub index_c: i32,
    pub normal: DbVector3,
    pub distance: f32,
    pub obsolete: bool,
}

#[derive(SpacetimeType, Clone)]
pub struct EpaEdge {
    pub index_a: i32,
    pub index_b: i32,
    pub obsolete: bool,
}

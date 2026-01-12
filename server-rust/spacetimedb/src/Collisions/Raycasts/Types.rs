use spacetimedb::{Identity, SpacetimeType};
use crate::*;

#[derive(SpacetimeType, Copy, Clone, Debug, PartialEq, Eq)]
pub enum RaycastHitType {
    None = 0,
    Magician = 1,
    MapPiece = 2,
}

#[derive(SpacetimeType, Copy, Clone, Debug)]
pub struct Raycast {
    pub hit: bool,
    pub hit_distance: f32,
    pub hit_point: DbVector3,
    pub hit_type: RaycastHitType,
    pub hit_identity: Identity,
    pub hit_entity_id: u64,
}

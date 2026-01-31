use spacetimedb::{table};
use crate::*;

#[table(name = map, public)]
pub struct Map {
    #[primary_key] #[auto_inc] pub id: u64,
    #[unique] pub name: String, // Used to link to prefab on client side
    pub collider: ComplexCollider,
}

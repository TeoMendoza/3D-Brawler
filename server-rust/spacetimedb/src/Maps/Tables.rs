use std::time::Duration;
use spacetimedb::{table, rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;

#[spacetimedb::table(name = map, public)]
pub struct Map {
    #[primary_key] #[auto_inc] pub id: u64,
    #[unique] pub name: String,
    pub collider: ComplexCollider,
}

use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

#[spacetimedb::type]
pub struct MagicianConfig {
    pub player: Player,
    pub match_id: u32,
    pub position: DbVector3,
}

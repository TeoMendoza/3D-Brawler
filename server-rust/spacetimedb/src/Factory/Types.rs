use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;

#[derive(SpacetimeType)]
pub struct MagicianConfig {
    pub player: Player,
    pub game_id: u32,
    pub position: DbVector3,
}

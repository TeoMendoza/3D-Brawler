use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

#[derive(Clone, Debug)]
pub struct GjkDistanceResult {
    pub intersects: bool,
    pub distance: f32,
    pub separation_direction: DbVector3,
    pub point_on_a: DbVector3,
    pub point_on_b: DbVector3,
    pub simplex: Vec<GjkVertex>,
    pub last_direction: DbVector3,
}

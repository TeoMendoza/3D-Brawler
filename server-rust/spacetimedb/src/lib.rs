use std::time::Duration;
use spacetimedb::{table, rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp, UniqueColumn};

pub mod Collisions;
pub mod Factory;
pub mod General;
pub mod Magician;
pub mod Map;
pub mod Scrap;


pub use General::Types::*;
pub use Magician::Types::*;
pub use Map::Types::*;
pub use Collisions::*;

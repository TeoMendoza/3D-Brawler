use std::time::Duration;
use spacetimedb::{table, rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp, UniqueColumn};

pub mod Collisions;
pub mod Factory;
pub mod General;
pub mod Magicians;
pub mod Maps;
//pub mod Scrap;


pub use General::*;
pub use Factory::*;
pub use Magicians::*;
pub use Maps::*;
pub use Collisions::*;

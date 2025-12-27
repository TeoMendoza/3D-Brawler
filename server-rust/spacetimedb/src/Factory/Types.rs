use spacetimedb::{SpacetimeType};
use crate::*;

#[derive(SpacetimeType)]
pub struct MagicianConfig {
    pub player: Player,
    pub game_id: u32,
    pub position: DbVector3,
}

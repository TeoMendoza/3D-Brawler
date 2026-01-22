use crate::*;

#[derive(Clone, Debug, Default)]
pub struct GjkDistanceResult {
    pub intersects: bool,
    pub distance: f32,
    pub separation_direction: DbVector3,
    pub point_on_a: DbVector3,
    pub point_on_b: DbVector3,
    pub simplex: Vec<GjkVertex>,
    pub last_direction: DbVector3,
}

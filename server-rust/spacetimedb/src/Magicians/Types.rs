use spacetimedb::{SpacetimeType};
use crate::*;

#[derive(SpacetimeType)]
pub struct KinematicInformation {
    pub jump: bool,
    pub falling: bool,
    pub crouched: bool,
    pub grounded: bool,
    pub sprinting: bool,
}

#[derive(SpacetimeType)]
pub struct ActionRequestMagician {
    pub state: MagicianState,
    pub attack_information: AttackInformation,
    pub reload_information: ReloadInformation,
    pub dust_information: DustInformation,
}

#[derive(SpacetimeType)]
pub struct AttackInformation {
    pub camera_position_offset: DbVector3,
    pub camera_yaw_offset: f32,
    pub camera_pitch_offset: f32,
    pub spawn_point_offset: DbVector3,
    pub max_distance: f32,
}

#[derive(SpacetimeType)]
pub struct ReloadInformation {}

#[derive(SpacetimeType)]
pub struct DustInformation {
    pub camera_position_offset: DbVector3,
    pub camera_yaw_offset: f32,
    pub camera_pitch_offset: f32,
    pub spawn_point_offset: DbVector3,
    pub max_distance: f32,
    pub cone_half_angle_degrees: f32
}

#[derive(SpacetimeType, PartialEq, Eq, Clone, Copy)]
pub enum MagicianState {
    Default,
    Attack,
    Reload,
    Dust, // Blind Ability
}

#[derive(SpacetimeType, Clone)]
pub struct ThrowingCard {
    pub effects: Vec<Effect>,
}

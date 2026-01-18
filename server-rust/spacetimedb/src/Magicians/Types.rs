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
    pub cloak_information: CloakInformation,
    pub hypnosis_information: HypnosisInformation
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
pub struct CloakInformation {}

#[derive(SpacetimeType)]
pub struct DustInformation {
    pub camera_position_offset: DbVector3,
    pub camera_yaw_offset: f32,
    pub camera_pitch_offset: f32,
    pub spawn_point_offset: DbVector3,
    pub max_distance: f32,
    pub cone_half_angle_degrees: f32
}

#[derive(SpacetimeType)]
pub struct HypnosisInformation {}

#[derive(SpacetimeType)]
pub struct HypnosisCameraInformation {
    pub camera_position_offset: DbVector3,
    pub camera_yaw_offset: f32,
    pub camera_pitch_offset: f32,
    pub spawn_point_offset: DbVector3,
    pub max_distance: f32,
}

#[derive(SpacetimeType, PartialEq, Eq, Clone, Copy)]
pub enum MagicianState {
    Default,
    Attack,
    Reload,
    Dust, // Blind Ability
    Cloak,
    Hypnosis
}

#[derive(SpacetimeType, Clone)]
pub struct ThrowingCard {
    pub effects: Vec<Effect>,
}

#[derive(SpacetimeType, Clone)]
pub struct CombatInformation {
    pub health: f32,
    pub max_health: f32,
    pub speed_multiplier: f32,
    pub game_score: u32,
    pub blind: bool,
    pub reversed: bool,
    pub stunned: bool,
    pub cloaked: bool,
    pub hypnosis: bool
}

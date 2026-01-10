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

#[derive(SpacetimeType, PartialEq, Eq, Clone, Copy)]
pub enum MagicianState {
    Default,
    Attack,
    Reload,
}

#[derive(SpacetimeType, Clone)]
pub struct ThrowingCard {
    pub effects: Vec<Effect>,
}

#[derive(SpacetimeType, Clone)]
pub struct Effect {
    pub effect_type: EffectType,
}

#[derive(SpacetimeType, PartialEq, Eq, Clone)]
pub enum EffectType {
    Damage,
}

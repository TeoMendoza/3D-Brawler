use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

#[spacetimedb::type]
pub struct KinematicInformation {
    pub jump: bool,
    pub falling: bool,
    pub crouched: bool,
    pub grounded: bool,
    pub sprinting: bool,
}

#[spacetimedb::type]
pub struct ActionRequestMagician {
    pub state: MagicianState,
    pub attack_information: AttackInformation,
    pub reload_information: ReloadInformation,
}

#[spacetimedb::type]
pub struct AttackInformation {
    pub camera_position_offset: DbVector3,
    pub camera_yaw_offset: f32,
    pub camera_pitch_offset: f32,
    pub spawn_point_offset: DbVector3,
    pub max_distance: f32,
}

#[spacetimedb::type]
pub struct ReloadInformation {}

#[spacetimedb::type]
pub enum MagicianState {
    Default,
    Attack,
    Reload,
}

#[spacetimedb::type]
pub struct ThrowingCard {
    pub effects: Vec<Effect>,
}

#[spacetimedb::type]
pub struct Effect {
    pub effect_type: EffectType,
}

#[spacetimedb::type]
pub enum EffectType {
    Damage,
}

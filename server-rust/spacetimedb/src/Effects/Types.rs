use spacetimedb::{SpacetimeType};

#[derive(SpacetimeType, Clone)]
pub struct Effect {
    pub effect_type: EffectType,
    pub application_information: ApplicationInformation,
    pub damage_information: Option<DamageInformation>,
}

#[derive(SpacetimeType, Clone)]
pub struct ApplicationInformation {
    pub application_type: ApplicationType,
    pub current_time: Option<f32>,
    pub end_time: Option<f32>, // When The Effect Should End & Be Removed From Table (Duration & Reapply)
    pub reapply_time: Option<f32>, // How Often Should Effect Be Reapplied (Reapply)
    pub current_reapply_time: Option<f32>
}

#[derive(SpacetimeType, Clone, Copy)]
pub struct DamageInformation {
    pub base_damage: f32,
    pub damage_multiplier: f32, // For Headshot, Legshot, Bodyshot
}

#[derive(SpacetimeType, PartialEq, Eq, Clone)]
pub enum EffectType {
    Damage,
}

#[derive(SpacetimeType, Clone, Copy)]
pub enum ApplicationType {
    Single,
    Duration,
    Reapply,
}
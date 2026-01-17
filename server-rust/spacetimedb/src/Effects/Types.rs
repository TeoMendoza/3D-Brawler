use spacetimedb::{SpacetimeType};

#[derive(SpacetimeType, Clone)]
pub struct Effect {
    pub effect_type: EffectType,
    pub application_information: ApplicationInformation,
    pub damage_information: Option<DamageEffectInformation>,
    pub cloak_information: Option<CloakEffectInformation>,
    pub dust_information: Option<DustEffectInformation>,
    pub speed_information: Option<SpeedEffectInformation>
}

#[derive(SpacetimeType, Clone)]
pub struct ApplicationInformation {
    pub application_type: ApplicationType,
    pub current_time: Option<f32>,
    pub end_time: Option<f32>, // When The Effect Should End & Be Removed From Table (Duration & Reapply)
    pub reapply_time: Option<f32>, // How Often Should Effect Be Reapplied (Reapply)
    pub current_reapply_time: Option<f32>,
}

#[derive(SpacetimeType, Clone)]
pub struct DamageEffectInformation {
    pub base_damage: f32,
    pub damage_multiplier: f32, // For Headshot, Legshot, Bodyshot
}

#[derive(SpacetimeType, Clone)]
pub struct DustEffectInformation { } // Not Sure What To Put Here, Probably Some Sort Of Data To Determine How To Fade The Effect, Also Maybe A Visiblity Parameter For How See Through

#[derive(SpacetimeType, Clone)]
pub struct CloakEffectInformation { } // Maybe Visiblity Param Later

#[derive(SpacetimeType, Clone)]
pub struct SpeedEffectInformation {
    pub speed_multiplier: f32 // Can Be Increase / Decrease
 }

#[derive(SpacetimeType, PartialEq, Eq, Clone, Copy)]
pub enum EffectType {
    Damage,
    Dust,
    Cloak,
    Speed
}

#[derive(SpacetimeType, Clone)]
pub enum ApplicationType {
    Single,
    Duration,
    Reapply
}
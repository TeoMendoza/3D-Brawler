use spacetimedb::{SpacetimeType};

#[derive(SpacetimeType, Clone, Copy)]
pub struct Effect { // Generic effect type, effect specific information must be set but non related can be none
    pub effect_type: EffectType,
    pub application_information: ApplicationInformation,
    pub damage_information: Option<DamageEffectInformation>,
    pub cloak_information: Option<CloakEffectInformation>,
    pub dust_information: Option<DustEffectInformation>,
    pub speed_information: Option<SpeedEffectInformation>,
    pub hypnosis_information: Option<HypnosisEffectInformation>,
    pub stunned_information: Option<StunnedEffectInformation>,
    pub tarot_information: Option<TarotEffectInformation>,
    pub invincible_information: Option<InvincibleEffectInformation>
}

#[derive(SpacetimeType, Clone, Copy)]
pub struct ApplicationInformation {
    pub application_type: ApplicationType,
    pub current_time: Option<f32>,
    pub end_time: Option<f32>, // When The Effect Should End & Be Removed From Table (Duration & Reapply)
    pub reapply_time: Option<f32>, // How Often Should Effect Be Reapplied (Reapply)
    pub current_reapply_time: Option<f32>,
    pub applied: Option<bool>
}

#[derive(SpacetimeType, Clone, Copy)]
pub struct DamageEffectInformation {
    pub base_damage: f32,
    pub damage_multiplier: f32, // For Headshot, Legshot, Bodyshot
}

#[derive(SpacetimeType, Clone, Copy)]
pub struct DustEffectInformation { } // Not Sure What To Put Here, Probably Some Sort Of Data To Determine How To Fade The Effect, Also Maybe A Visiblity Parameter For How See Through

#[derive(SpacetimeType, Clone, Copy)]
pub struct CloakEffectInformation { } // Maybe Visiblity Param Later

#[derive(SpacetimeType, Clone, Copy)]
pub struct SpeedEffectInformation {
    pub speed_multiplier: f32 // Can Be Increase / Decrease
}

#[derive(SpacetimeType, Clone, Copy)]
pub struct HypnosisEffectInformation { 
    pub last_target_id: Option<u64>, // Used To Determine Previous Target To Undo Effects When Target Changes
}

#[derive(SpacetimeType, Clone, Copy)]
pub struct StunnedEffectInformation { }

#[derive(SpacetimeType, Clone, Copy)]
pub struct TarotEffectInformation { }

#[derive(SpacetimeType, Clone, Copy)]
pub struct InvincibleEffectInformation { }

#[derive(SpacetimeType, PartialEq, Eq, Clone, Copy, Debug)]
pub enum EffectType {
    Damage, // Damages target
    Dust, // Blinds target
    Cloak, // Makes target invisible
    Speed, // Alters target movement speed
    Stunned, // Stuns target
    Hypnosis, // Makes target able to stun other players
    Tarot, // Makes target movement input reversed
    Invincible // Makes target invincible
}

#[derive(SpacetimeType, Clone, Copy)]
pub enum ApplicationType {
    Single,
    Duration,
    Reapply,
    Indefinite
}
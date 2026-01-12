use spacetimedb::{table};
use crate::*;

#[table(name = player_effects)]
pub struct PlayerEffect
{
    #[primary_key] #[unique] #[auto_inc] pub id: u64,
    #[index(btree)] pub target_id: u64,
    #[index(btree)] pub game_id: u32,

    // Exactly The Same As Effect Class, But Restore For Organizational Purposes
    pub effect_type: EffectType,
    pub application_information: ApplicationInformation,
    pub damage_information: Option<DamageInformation>
}
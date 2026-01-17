use spacetimedb::{ReducerContext};
use crate::*;

pub fn apply_damage_effect_magician(ctx: &ReducerContext, magician: &mut Magician, damage_effect: &Option<DamageEffectInformation>) 
{
    log::info!("Apply Damage Effect Called");
}

pub fn match_single_effect_to_method(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Damage => { apply_damage_effect_magician(ctx, magician, &effect.damage_information); },

        _ => {}
    }
}

pub fn match_duration_effect_to_method(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Dust => {},

        _ => {}
    }
}

pub fn match_reapply_effect_to_method(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        _ => {}
    }
}
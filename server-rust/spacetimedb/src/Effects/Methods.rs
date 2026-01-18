use spacetimedb::{ReducerContext};
use crate::*;

pub fn apply_damage_effect_magician(ctx: &ReducerContext, magician: &mut Magician, damage_effect: &Option<DamageEffectInformation>) 
{
    log::info!("Apply Damage Effect Called");
    let damage = damage_effect.as_ref().expect("Damage Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.health -= damage.base_damage;
}

pub fn apply_dust_effect_magician(ctx: &ReducerContext, magician: &mut Magician, dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Apply Dust Effect Called");
    let _dust = dust_effect.as_ref().expect("Dust Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.blind = true;
}

pub fn apply_cloak_effect_magician(ctx: &ReducerContext, magician: &mut Magician, cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Apply Cloak Effect Called");
    let _cloak = cloak_effect.as_ref().expect("Cloak Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.cloaked = true;
}

pub fn apply_speed_effect_magician(ctx: &ReducerContext, magician: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Apply Speed Effect Called");
    let speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.speed_multiplier = speed.speed_multiplier;
}   

pub fn apply_hypnosis_effect_magician(ctx: &ReducerContext, magician: &mut Magician, hypnosis_effect: &Option<HypnosisEffectInformation>) 
{
    log::info!("Apply Hypnosis Effect Called");
    let _hypnosis = hypnosis_effect.as_ref().expect("Hypnosis Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.hypnosis = true;
}

pub fn apply_stunned_effect_magician(ctx: &ReducerContext, magician: &mut Magician, stunned_effect: &Option<StunnedEffectInformation>) 
{
    log::info!("Apply Stunned Effect Called");
    let _stunned = stunned_effect.as_ref().expect("Stunned Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.stunned = true;
}


pub fn undo_dust_effect_magician(ctx: &ReducerContext, magician: &mut Magician, dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Undo Dust Effect Called");
    let _dust = dust_effect.as_ref().expect("Dust Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.blind = false;
}

pub fn undo_cloak_effect_magician(ctx: &ReducerContext, magician: &mut Magician, cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Undo Cloak Effect Called");
    let _cloak = cloak_effect.as_ref().expect("Cloak Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.cloaked = false;
}

pub fn undo_speed_effect_magician(ctx: &ReducerContext, magician: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Undo Speed Effect Called");
    let _speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.speed_multiplier = 1.0;
}

pub fn undo_hypnosis_effect_magician(ctx: &ReducerContext, magician: &mut Magician, hypnosis_effect: &Option<HypnosisEffectInformation>) 
{
    log::info!("Undo Hypnosis Effect Called");
    let _hypnosis = hypnosis_effect.as_ref().expect("Hypnosis Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.hypnosis = false;

    for effect in ctx.db.player_effects().sender_id().filter(magician.id) {
        // Find The Stun Effect And Call Undo Function For Target Player + Remove From DB. Also change it to use hypnosis effect last stunned player data. Itll be easier to know if a stun currently exists when hypnosis ultimate ends
    }
}

pub fn undo_stunned_effect_magician(ctx: &ReducerContext, magician: &mut Magician, stunned_effect: &Option<StunnedEffectInformation>) 
{
    log::info!("Undo Stunned Effect Called");
    let _stunned = stunned_effect.as_ref().expect("Stunned Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.stunned = false;
}

pub fn match_and_apply_single_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Damage => { apply_damage_effect_magician(ctx, magician, &effect.damage_information); },

        _ => {}
    }
}

pub fn match_and_apply_duration_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Dust => { apply_dust_effect_magician(ctx, magician, &effect.dust_information); },

        EffectType::Cloak => { apply_cloak_effect_magician(ctx, magician, &effect.cloak_information); },

        EffectType::Speed => { apply_speed_effect_magician(ctx, magician, &effect.speed_information); },

        EffectType::Hypnosis => { apply_hypnosis_effect_magician(ctx, magician, &effect.hypnosis_informaton ); }
        _ => {}
    }
}

pub fn match_and_apply_reapply_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        _ => {}
    }
}

pub fn match_and_apply_indefinite_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {

        EffectType::Stunned => { apply_stunned_effect_magician(ctx, magician, &effect.stunned_information);}

        _ => {}
    }
}


pub fn match_and_undo_duration_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Dust => { undo_dust_effect_magician(ctx, magician, &effect.dust_information); },

        EffectType::Cloak => { undo_cloak_effect_magician(ctx, magician, &effect.cloak_information); },

        EffectType::Speed => { undo_speed_effect_magician(ctx, magician, &effect.speed_information); },

        EffectType::Hypnosis => { undo_hypnosis_effect_magician(ctx, magician, &effect.hypnosis_informaton ); }

        _ => {}
    }
}

pub fn match_and_undo_reapply_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        _ => {}
    }
}

pub fn match_and_undo_indefinite_effect(ctx: &ReducerContext, magician: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {

        EffectType::Stunned => { undo_stunned_effect_magician(ctx, magician, &effect.stunned_information);}

        _ => {}
    }
}


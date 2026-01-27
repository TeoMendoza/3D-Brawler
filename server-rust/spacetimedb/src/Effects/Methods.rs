use spacetimedb::{ReducerContext};
use crate::*;


// -------------------
// Effect Applications
// -------------------

pub fn apply_damage_effect_magician(ctx: &ReducerContext, magician: &mut Magician, damage_effect: &Option<DamageEffectInformation>) 
{
    log::info!("Apply Damage Effect Called");
    let damage = damage_effect.as_ref().expect("Damage Effect Must Have Information!");
    let combat_info = &mut magician.combat_information;
    combat_info.health -= damage.base_damage;

    if combat_info.health <= 0.0 {
        handle_magician_death(ctx, magician);
    }

    else {
        try_interrupt_cloak_and_speed_effects_magician(ctx, magician);
    }
}

pub fn apply_dust_effect_magician(_ctx: &ReducerContext, target: &mut Magician, dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Apply Dust Effect Called");
    let _dust = dust_effect.as_ref().expect("Dust Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Dusted", "DustEffect");
}

pub fn apply_cloak_effect_magician(_ctx: &ReducerContext, target: &mut Magician, cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Apply Cloak Effect Called");
    let _cloak = cloak_effect.as_ref().expect("Cloak Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Cloaked", "CloakEffect");
}

pub fn apply_speed_effect_magician(_ctx: &ReducerContext, target: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Apply Speed Effect Called");
    let speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = speed.speed_multiplier;
}   

pub fn apply_hypnosis_effect_magician(_ctx: &ReducerContext, target: &mut Magician, hypnosis_effect: &Option<HypnosisEffectInformation>) 
{
    log::info!("Apply Hypnosis Effect Called");
    let _hypnosis = hypnosis_effect.as_ref().expect("Hypnosis Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Hypnosised", "HypnosisEffect");
}

pub fn apply_stunned_effect_magician(ctx: &ReducerContext, target: &mut Magician, stunned_effect: &Option<StunnedEffectInformation>) 
{
    log::info!("Apply Stunned Effect Called");
    let _stunned = stunned_effect.as_ref().expect("Stunned Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Stunned", "StunEffect");
    try_interrupt_cloak_and_speed_effects_magician(ctx, target);
    adjust_timer_for_stunnable_state(target, target.state);
    target.state = MagicianState::Stunned;
}

pub fn apply_tarot_effect_magician(_ctx: &ReducerContext, target: &mut Magician, tarot_effect: &Option<TarotEffectInformation>) 
{
    log::info!("Apply Tarot Effect Called");
    let _tarot = tarot_effect.as_ref().expect("Tarot Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Taroted", "TarotEffect");
}

pub fn apply_invincible_effect_magician(_ctx: &ReducerContext, target: &mut Magician, invincible_effect: &Option<InvincibleEffectInformation>) 
{
    log::info!("Apply Invincible Effect Called");
    let _dust = invincible_effect.as_ref().expect("Invincible Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Invincibled", "InvincibleEffect");
}


// ---------------
// Effect Removals
// ---------------

pub fn undo_dust_effect_magician(_ctx: &ReducerContext, target: &mut Magician, dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Undo Dust Effect Called");
    let _dust = dust_effect.as_ref().expect("Dust Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Dusted", "DustEffect");
}

pub fn undo_cloak_effect_magician(_ctx: &ReducerContext, target: &mut Magician, cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Undo Cloak Effect Called");
    let _cloak = cloak_effect.as_ref().expect("Cloak Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Cloaked", "CloakEffect");
}

pub fn undo_speed_effect_magician(_ctx: &ReducerContext, target: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Undo Speed Effect Called");
    let _speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = 1.0;
}

pub fn undo_hypnosis_effect_magician(ctx: &ReducerContext, target: &mut Magician, hypnosis_effect: &Option<HypnosisEffectInformation>) 
{
    log::info!("Undo Hypnosis Effect Called");
    let hypnosis = hypnosis_effect.as_ref().expect("Hypnosis Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Hypnosised", "HypnosisEffect");

    if let Some(last_target_id) = hypnosis.last_target_id {
        let stunned_magician_option = ctx.db.magician().id().find(last_target_id); 
        if let Some(mut stunned_magician) = stunned_magician_option {
            let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, target.id, EffectType::Stunned));
            let stunned_effect_option = match (stunned_iterator.next(), stunned_iterator.next()) {
                (None, _) => None,
                (Some(effect), None) => Some(effect),
                (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"),
            };

            if let Some(stunned_effect) = stunned_effect_option {
                undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id);
                ctx.db.magician().id().update(stunned_magician);
            }
        }    
    }
}

pub fn undo_tarot_effect_magician(_ctx: &ReducerContext, target: &mut Magician, tarot_effect: &Option<TarotEffectInformation>) 
{
    log::info!("Undo Tarot Effect Called");
    let _tarot = tarot_effect.as_ref().expect("Tarot Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Taroted", "TarotEffect");
}

pub fn undo_invincible_effect_magician(_ctx: &ReducerContext, target: &mut Magician, invincible_effect: &Option<InvincibleEffectInformation>) 
{
    log::info!("Undo Invincible Effect Called");
    let _dust = invincible_effect.as_ref().expect("Invincible Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Invincibled", "InvincibleEffect");
}


// ---------------
// Match Functions
// ---------------

pub fn match_and_apply_single_effect(ctx: &ReducerContext, target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Damage => { apply_damage_effect_magician(ctx, target, &effect.damage_information); },

        _ => {}
    }
}

pub fn match_and_apply_duration_effect(ctx: &ReducerContext, target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Dust => { apply_dust_effect_magician(ctx, target, &effect.dust_information); },

        EffectType::Cloak => { apply_cloak_effect_magician(ctx, target, &effect.cloak_information); },

        EffectType::Speed => { apply_speed_effect_magician(ctx, target, &effect.speed_information); },

        EffectType::Hypnosis => { apply_hypnosis_effect_magician(ctx, target, &effect.hypnosis_informaton ); }

        EffectType::Tarot => { apply_tarot_effect_magician(ctx, target, &effect.tarot_information); },

        EffectType::Invincible => { apply_invincible_effect_magician(ctx, target, &effect.invincible_information); },

        _ => {}
    }
}

pub fn match_and_apply_reapply_effect(_ctx: &ReducerContext, _target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        _ => {}
    }
}

pub fn match_and_apply_indefinite_effect(ctx: &ReducerContext, target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {

        EffectType::Stunned => { apply_stunned_effect_magician(ctx, target, &effect.stunned_information);}

        _ => {}
    }
}


pub fn match_and_undo_duration_effect(ctx: &ReducerContext, target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        EffectType::Dust => { undo_dust_effect_magician(ctx, target, &effect.dust_information); },

        EffectType::Cloak => { undo_cloak_effect_magician(ctx, target, &effect.cloak_information); },

        EffectType::Speed => { undo_speed_effect_magician(ctx, target, &effect.speed_information); },

        EffectType::Hypnosis => { undo_hypnosis_effect_magician(ctx, target, &effect.hypnosis_informaton ); }

        EffectType::Tarot => { undo_tarot_effect_magician(ctx, target, &effect.tarot_information); },

        EffectType::Invincible => { undo_invincible_effect_magician(ctx, target, &effect.invincible_information); },

        _ => {}
    }
}

pub fn match_and_undo_reapply_effect(_ctx: &ReducerContext, _target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        _ => {}
    }
}


// ------------------------------
// Effect Interupption & Deletion
// ------------------------------


pub fn try_interrupt_cloak_and_speed_effects_magician(ctx: &ReducerContext, magician: &mut Magician)
{
    let mut cloak_iterator = ctx.db.player_effects().target_sender_and_type().filter((magician.id, magician.id, EffectType::Cloak));
    let mut speed_iterator = ctx.db.player_effects().target_sender_and_type().filter((magician.id, magician.id, EffectType::Speed));

    let cloak_effect_option = match (cloak_iterator.next(), cloak_iterator.next()) {
        (None, _) => None,
        (Some(effect), None) => Some(effect),
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Cloak Effect At Most!"),
    };

    let speed_effect_option = match (speed_iterator.next(), speed_iterator.next()) {
        (None, _) => None,
        (Some(effect), None) => Some(effect),
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Self Applied Speed Effect At Most!"),
    };

    if let Some(cloak_effect) = cloak_effect_option {
        undo_and_delete_cloak_effect_magician(ctx, magician, cloak_effect.id);
    }

    if let Some(speed_effect) = speed_effect_option {
        undo_and_delete_speed_effect_magician(ctx, magician, speed_effect.id);
    }
}

pub fn try_interrupt_invincible_effect_magician(ctx: &ReducerContext, magician: &mut Magician)
{
    let mut invincible_iterator = ctx.db.player_effects().target_sender_and_type().filter((magician.id, magician.id, EffectType::Invincible));

    let invincible_effect_option = match (invincible_iterator.next(), invincible_iterator.next()) {
        (None, _) => None,
        (Some(effect), None) => Some(effect),
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Invincible Effect At Most!"),
    };

    if let Some(invincible_effect) = invincible_effect_option {
        undo_and_delete_invincible_effect_magician(ctx, magician, invincible_effect.id);
    }
}

pub fn undo_and_delete_invincible_effect_magician(ctx: &ReducerContext, target: &mut Magician, invincible_effect_id: u64)
{
    log::info!("Undo & Delete Invincible Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Invincibled", "InvincibleEffect");
    ctx.db.player_effects().id().delete(invincible_effect_id);
}

pub fn undo_and_delete_stunned_effect_magician(ctx: &ReducerContext, target: &mut Magician, stunned_effect_id: u64)
{
    log::info!("Undo & Delete Stunned Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Stunned", "StunEffect");
    target.state = MagicianState::Default;
    ctx.db.player_effects().id().delete(stunned_effect_id);
}

pub fn undo_and_delete_cloak_effect_magician(ctx: &ReducerContext, target: &mut Magician, cloak_effect_id: u64)
{
    log::info!("Undo & Delete Cloak Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Cloaked", "CloakEffect");
    ctx.db.player_effects().id().delete(cloak_effect_id);
}

pub fn undo_and_delete_speed_effect_magician(ctx: &ReducerContext, target: &mut Magician, speed_effect_id: u64)
{
    log::info!("Undo & Delete Speed Effect Called");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = 1.0;
    ctx.db.player_effects().id().delete(speed_effect_id);
}
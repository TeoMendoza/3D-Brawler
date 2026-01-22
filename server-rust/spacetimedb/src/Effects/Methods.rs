use spacetimedb::{ReducerContext};
use crate::*;

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

pub fn apply_dust_effect_magician(ctx: &ReducerContext, target: &mut Magician, dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Apply Dust Effect Called");
    let _dust = dust_effect.as_ref().expect("Dust Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Dusted", "DustEffect");
}

pub fn apply_cloak_effect_magician(ctx: &ReducerContext, target: &mut Magician, cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Apply Cloak Effect Called");
    let _cloak = cloak_effect.as_ref().expect("Cloak Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Cloaked", "CloakEffect");
}

pub fn apply_speed_effect_magician(ctx: &ReducerContext, target: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Apply Speed Effect Called");
    let speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = speed.speed_multiplier;
}   

pub fn apply_hypnosis_effect_magician(ctx: &ReducerContext, target: &mut Magician, hypnosis_effect: &Option<HypnosisEffectInformation>) 
{
    log::info!("Apply Hypnosis Effect Called On Magician With Id {}", target.id);
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

pub fn apply_tarot_effect_magician(ctx: &ReducerContext, target: &mut Magician, tarot_effect: &Option<TarotEffectInformation>) 
{
    log::info!("Apply Tarot Effect Called");
    let _tarot = tarot_effect.as_ref().expect("Tarot Effect Must Have Information!");
    add_subscriber_to_permission(&mut target.permissions, "Taroted", "TarotEffect");
}

pub fn undo_dust_effect_magician(ctx: &ReducerContext, target: &mut Magician, dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Undo Dust Effect Called");
    let _dust = dust_effect.as_ref().expect("Dust Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Dusted", "DustEffect");
}

pub fn undo_cloak_effect_magician(ctx: &ReducerContext, target: &mut Magician, cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Undo Cloak Effect Called");
    let _cloak = cloak_effect.as_ref().expect("Cloak Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Cloaked", "CloakEffect");
}

pub fn undo_speed_effect_magician(ctx: &ReducerContext, target: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
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
        let mut stunned_magician = ctx.db.magician().id().find(last_target_id).expect("Hypnosis Last Target Magician Must Exist!"); // Not Necessarily True - Player Could Have Left - Unlikely But Handle That Case Later
        let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, target.id, EffectType::Stunned));

        let stunned_effect = match (stunned_iterator.next(), stunned_iterator.next()) {
            (None, _) => panic!("Stunned Iterator Should Have First Element!"),
            (Some(effect), None) => effect,
            (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"),
        };

        undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id);
        ctx.db.magician().id().update(stunned_magician);
    }
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

pub fn undo_tarot_effect_magician(ctx: &ReducerContext, target: &mut Magician, tarot_effect: &Option<TarotEffectInformation>) 
{
    log::info!("Undo Tarot Effect Called");
    let _tarot = tarot_effect.as_ref().expect("Tarot Effect Must Have Information!");
    remove_subscriber_from_permission(&mut target.permissions, "Taroted", "TarotEffect");
}

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

        _ => {}
    }
}

pub fn match_and_apply_reapply_effect(ctx: &ReducerContext, target: &mut Magician, effect: &PlayerEffect) 
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

        _ => {}
    }
}

pub fn match_and_undo_reapply_effect(ctx: &ReducerContext, target: &mut Magician, effect: &PlayerEffect) 
{
    match effect.effect_type {
        _ => {}
    }
}

pub fn match_and_undo_indefinite_effect(ctx: &ReducerContext) 
{
    // Match And Undo Indefinite Effects Is Empty 
    // Indefinite Effects Must Be Manually Undone & Removed From Database From An Outward Source That Is Not The Scheduled Effect Reducer
    // This Is Because They Have No Defined Time To Be Undone / Removed, It Is Dynamic And Depends On The Situation (Example: Stun Effect)
    // Application Follows Normally, When Added To Table, Apply And Mark As Applied
    // Removal / Undoing Is More Complicated - Depends On The Sender Themselves (Magician Ultimate) Because The Magician Looking At The Target Stuns Them, But It's Variable How Long The Magician Is Looking
}

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



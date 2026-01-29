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

    if combat_info.health <= 0.0 { // Kills target magician
        handle_magician_death(ctx, magician);
    }

    else { // Target magician can have cloak ability effects interrupted by incoming damage
        try_interrupt_cloak_and_speed_effects_magician(ctx, magician);
    }
}

pub fn apply_dust_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Apply Dust Effect Called");
    add_subscriber_to_permission(&mut target.permissions, "Dusted", "DustEffect"); // Dust application - Client will read and update visually
}

pub fn apply_cloak_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _loak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Apply Cloak Effect Called");
    add_subscriber_to_permission(&mut target.permissions, "Cloaked", "CloakEffect"); // Cloak application - Client will read and update visually
}

pub fn apply_speed_effect_magician(_ctx: &ReducerContext, target: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Apply Speed Effect Called");
    let speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = speed.speed_multiplier; // Speed application - Modifies movement request speed server side
}   

pub fn apply_hypnosis_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _hypnosis_effect: &Option<HypnosisEffectInformation>) 
{
    log::info!("Apply Hypnosis Effect Called");
    add_subscriber_to_permission(&mut target.permissions, "Hypnosised", "HypnosisEffect"); // Hypnosis application - Client will read and update visually
}

pub fn apply_stunned_effect_magician(ctx: &ReducerContext, target: &mut Magician, _stunned_effect: &Option<StunnedEffectInformation>) 
{
    log::info!("Apply Stunned Effect Called");
    add_subscriber_to_permission(&mut target.permissions, "Stunned", "StunEffect"); // Stun application - Client will read and update visually and block requests server side

    try_interrupt_cloak_and_speed_effects_magician(ctx, target); // Target magician can have cloak ability effects interrupted by incoming stun
    adjust_timer_for_stunnable_state(target, target.state); // Interrupts target magician current state and state timer
    target.state = MagicianState::Stunned;
}

pub fn apply_tarot_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _tarot_effect: &Option<TarotEffectInformation>) 
{
    log::info!("Apply Tarot Effect Called");
    add_subscriber_to_permission(&mut target.permissions, "Taroted", "TarotEffect"); // Tarot application - Reverses movement request server side
}

pub fn apply_invincible_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _invincible_effect: &Option<InvincibleEffectInformation>) 
{
    log::info!("Apply Invincible Effect Called");
    add_subscriber_to_permission(&mut target.permissions, "Invincibled", "InvincibleEffect"); // Invincible application - Client will read and update visually
}


// ---------------
// Effect Removals
// ---------------

pub fn undo_dust_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _dust_effect: &Option<DustEffectInformation>) 
{
    log::info!("Undo Dust Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Dusted", "DustEffect"); // Dust undo - Client will read and update visually
}

pub fn undo_cloak_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _cloak_effect: &Option<CloakEffectInformation>) 
{
    log::info!("Undo Cloak Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Cloaked", "CloakEffect"); // Cloak undo - Client will read and update visually
}

pub fn undo_speed_effect_magician(_ctx: &ReducerContext, target: &mut Magician, speed_effect: &Option<SpeedEffectInformation>) 
{
    log::info!("Undo Speed Effect Called");
    let _speed = speed_effect.as_ref().expect("Speed Effect Must Have Information!");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = 1.0; // Speed undo - Reverts movement request speed server side
}

pub fn undo_hypnosis_effect_magician(ctx: &ReducerContext, magician: &mut Magician, hypnosis_effect: &Option<HypnosisEffectInformation>) // Undoes hypnosis effect - Subsequently manually undoes stun effect if applicable
{
    log::info!("Undo Hypnosis Effect Called");
    let hypnosis = hypnosis_effect.as_ref().expect("Hypnosis Effect Must Have Information!");
    remove_subscriber_from_permission(&mut magician.permissions, "Hypnosised", "HypnosisEffect"); // Hypnosis undo - Client will read and update visually

    if let Some(last_target_id) = hypnosis.last_target_id { // Checks if magician still has a target when hypnosis ends and removes stun effect manually
        let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, magician.id, EffectType::Stunned));
        let stunned_effect_option = match (stunned_iterator.next(), stunned_iterator.next()) {
            (None, _) => None,
            (Some(effect), None) => Some(effect),
            (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"), // Only unacceptable case - Means duplicate effect entries
        };

        if let Some(stunned_effect) = stunned_effect_option {
            let stunned_magician_option = ctx.db.magician().id().find(last_target_id); 
            if let Some(mut stunned_magician) = stunned_magician_option {
                undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id); // If target still exists, undoes and removes stun effect from db
                ctx.db.magician().id().update(stunned_magician);
            }
        } 
    }
}

pub fn undo_tarot_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _tarot_effect: &Option<TarotEffectInformation>) 
{
    log::info!("Undo Tarot Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Taroted", "TarotEffect"); // Tarot undo - Returns movement request back to normal server side
}

pub fn undo_invincible_effect_magician(_ctx: &ReducerContext, target: &mut Magician, _invincible_effect: &Option<InvincibleEffectInformation>) 
{
    log::info!("Undo Invincible Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Invincibled", "InvincibleEffect"); // Invincible undo - Client will read and update visually
}


// ---------------
// Match Functions
// ---------------

pub fn match_and_apply_single_effect(ctx: &ReducerContext, target: &mut Magician, effect: &Effect) // Matches single effect with proper application logic
{
    match effect.effect_type {
        EffectType::Damage => { apply_damage_effect_magician(ctx, target, &effect.damage_information); },

        _ => {}
    }
}

pub fn match_and_apply_duration_effect(ctx: &ReducerContext, target: &mut Magician, effect: &Effect) // Matches duration effect with proper application logic
{
    match effect.effect_type {
        EffectType::Dust => { apply_dust_effect_magician(ctx, target, &effect.dust_information); },

        EffectType::Cloak => { apply_cloak_effect_magician(ctx, target, &effect.cloak_information); },

        EffectType::Speed => { apply_speed_effect_magician(ctx, target, &effect.speed_information); },

        EffectType::Hypnosis => { apply_hypnosis_effect_magician(ctx, target, &effect.hypnosis_information ); }

        EffectType::Tarot => { apply_tarot_effect_magician(ctx, target, &effect.tarot_information); },

        EffectType::Invincible => { apply_invincible_effect_magician(ctx, target, &effect.invincible_information); },

        _ => {}
    }
}

pub fn match_and_apply_reapply_effect(_ctx: &ReducerContext, _target: &mut Magician, effect: &Effect) // Matches reapply effect with proper application logic
{
    match effect.effect_type {
        _ => {}
    }
}

pub fn match_and_apply_indefinite_effect(ctx: &ReducerContext, target: &mut Magician, effect: &Effect) // Matches indefinite effect with proper application logic
{
    match effect.effect_type {

        EffectType::Stunned => { apply_stunned_effect_magician(ctx, target, &effect.stunned_information);}

        _ => {}
    }
}


pub fn match_and_undo_duration_effect(ctx: &ReducerContext, target: &mut Magician, effect: &Effect) // Matches duration effect with proper undo logic
{
    match effect.effect_type {
        EffectType::Dust => { undo_dust_effect_magician(ctx, target, &effect.dust_information); },

        EffectType::Cloak => { undo_cloak_effect_magician(ctx, target, &effect.cloak_information); },

        EffectType::Speed => { undo_speed_effect_magician(ctx, target, &effect.speed_information); },

        EffectType::Hypnosis => { undo_hypnosis_effect_magician(ctx, target, &effect.hypnosis_information ); }

        EffectType::Tarot => { undo_tarot_effect_magician(ctx, target, &effect.tarot_information); },

        EffectType::Invincible => { undo_invincible_effect_magician(ctx, target, &effect.invincible_information); },

        _ => {}
    }
}

pub fn match_and_undo_reapply_effect(_ctx: &ReducerContext, _target: &mut Magician, effect: &Effect) // Matches reapply effect with proper undo logic
{
    match effect.effect_type {
        _ => {}
    }
}


// ------------------------------
// Effect Interupption & Deletion - Effects that can be interrupted have manual interrupt logic
// ------------------------------


pub fn try_interrupt_cloak_and_speed_effects_magician(ctx: &ReducerContext, magician: &mut Magician) // Undoes magician cloak effects if existing
{
    let mut cloak_iterator = ctx.db.player_effects().target_sender_and_type().filter((magician.id, magician.id, EffectType::Cloak));
    let mut speed_iterator = ctx.db.player_effects().target_sender_and_type().filter((magician.id, magician.id, EffectType::Speed));

    let cloak_effect_option = match (cloak_iterator.next(), cloak_iterator.next()) {
        (None, _) => None,
        (Some(effect), None) => Some(effect),
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Cloak Effect At Most!"), // Unacceptable case - Magician can only have one cloak effect (self applied)
    };

    let speed_effect_option = match (speed_iterator.next(), speed_iterator.next()) {
        (None, _) => None,
        (Some(effect), None) => Some(effect),
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Self Applied Speed Effect At Most!"), // Unacceptable case - Magician can only have one speed effect (self applied)
    };

    if let Some(cloak_effect) = cloak_effect_option {
        undo_and_delete_cloak_effect_magician(ctx, magician, cloak_effect.id);
    }

    if let Some(speed_effect) = speed_effect_option {
        undo_and_delete_speed_effect_magician(ctx, magician, speed_effect.id);
    }
}

pub fn try_interrupt_invincible_effect_magician(ctx: &ReducerContext, magician: &mut Magician) // Undoes magician invincible effect if existing
{
    let mut invincible_iterator = ctx.db.player_effects().target_sender_and_type().filter((magician.id, magician.id, EffectType::Invincible));

    let invincible_effect_option = match (invincible_iterator.next(), invincible_iterator.next()) {
        (None, _) => None,
        (Some(effect), None) => Some(effect),
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Invincible Effect At Most!"), // Unacceptable case - Magician can only have one invincible effect (self applied)
    };

    if let Some(invincible_effect) = invincible_effect_option {
        undo_and_delete_invincible_effect_magician(ctx, magician, invincible_effect.id);
    }
}

pub fn undo_and_delete_invincible_effect_magician(ctx: &ReducerContext, target: &mut Magician, invincible_effect_id: u64)
{
    log::info!("Undo & Delete Invincible Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Invincibled", "InvincibleEffect"); // Invincible undo - Client will read and update visually
    ctx.db.player_effects().id().delete(invincible_effect_id);
}

pub fn undo_and_delete_stunned_effect_magician(ctx: &ReducerContext, target: &mut Magician, stunned_effect_id: u64)
{
    log::info!("Undo & Delete Stunned Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Stunned", "StunEffect"); // Stun undo - Client will read and update visually and will unblock requests server side
    target.state = MagicianState::Default;
    ctx.db.player_effects().id().delete(stunned_effect_id);
}

pub fn undo_and_delete_cloak_effect_magician(ctx: &ReducerContext, target: &mut Magician, cloak_effect_id: u64)
{
    log::info!("Undo & Delete Cloak Effect Called");
    remove_subscriber_from_permission(&mut target.permissions, "Cloaked", "CloakEffect"); // Cloak undo - Client will read and update visually
    ctx.db.player_effects().id().delete(cloak_effect_id);
}

pub fn undo_and_delete_speed_effect_magician(ctx: &ReducerContext, target: &mut Magician, speed_effect_id: u64) 
{
    log::info!("Undo & Delete Speed Effect Called");
    let combat_info = &mut target.combat_information;
    combat_info.speed_multiplier = 1.0; // Speed undo - Reverts movement request speed server side
    ctx.db.player_effects().id().delete(speed_effect_id);
}
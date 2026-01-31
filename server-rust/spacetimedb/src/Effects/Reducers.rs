use spacetimedb::{reducer, ReducerContext, Table};
use crate::*;

#[reducer]
pub fn handle_player_effects_table(ctx: &ReducerContext, timer: PlayerEffectsTableTimer) { // Handles effects on players in specified game - Effects Type Cases: single, duration, reapply, indefinite
    let time = timer.tick_rate;
    for mut player_effect in ctx.db.player_effects().game_id().filter(timer.game_id) {
        let target_option = ctx.db.magician().id().find(player_effect.target_id);
        if target_option.is_none() { // Deletes and continues if target no longer exists
            ctx.db.player_effects().id().delete(player_effect.id);
            continue;
        }

        let mut target = target_option.expect("Target Magician Existence Already Confirmed!");
        let mut _sender_option = ctx.db.magician().id().find(player_effect.sender_id); // Will be used to reward points to attacker (WIP)
        let player_effect_clone = player_effect.effect_info.clone(); // Double mutable borrow bypass
        let app_info = &mut player_effect.effect_info.application_information;

        match app_info.application_type {
            ApplicationType::Single => { // Case: single - Single effects are applied once then subsequently deleted
                match_and_apply_single_effect(ctx, &mut target, &player_effect_clone);
                ctx.db.player_effects().id().delete(player_effect.id);
            },

            ApplicationType::Duration => { // Case: duration - Duration effects are applied once at the beginning, then undone and deleted once duration has passed
                let current_time = app_info.current_time.as_mut().expect("Duration effect must have a current time");
                if *current_time == 0.0 {
                    match_and_apply_duration_effect(ctx, &mut target, &player_effect_clone);
                }
                *current_time += time;

                let end_time = app_info.end_time.as_ref().expect("Duration effect must have an end time");
                if *current_time >= *end_time {
                    match_and_undo_duration_effect(ctx, &mut target, &player_effect_clone);
                    ctx.db.player_effects().id().delete(player_effect.id);
                }

                else { 
                    ctx.db.player_effects().id().update(player_effect);
                }

            },
            
            ApplicationType::Reapply => { // Case: reapply - Reapply effects are applied in intervals every "reapply_time", then undone (if applicable) and deleted once duration has passed
                let current_reapply_time = app_info.current_reapply_time.as_mut().expect("Reapply effect must have current reapply time");
                if *current_reapply_time == 0.0 {
                    match_and_apply_reapply_effect(ctx, &mut target, &player_effect_clone);
                }
                *current_reapply_time += time;

                let reapply_time = app_info.reapply_time.as_ref().expect("Reapply effect must have reapply time");
                if *current_reapply_time >= *reapply_time {
                    *current_reapply_time = 0.0;
                }

                let current_time = app_info.current_time.as_mut().expect("Reapply effect must have current time");
                *current_time += time;
                
                let end_time = app_info.end_time.as_ref().expect("Reapply effect must have an end time");
                if *current_time >= *end_time {
                    match_and_undo_reapply_effect(ctx, &mut target, &player_effect_clone);
                    ctx.db.player_effects().id().delete(player_effect.id);
                }
                
                else {
                    ctx.db.player_effects().id().update(player_effect);
                }
            }

            ApplicationType::Indefinite => { // Case: idefinite - Indefinite effects are applied once, then stay active for an indefinite amount of time. Indefinite effects must be manually undone and deleted
                let applied = app_info.applied.as_mut().expect("Indefinite effect must have applied boolean");
                if *applied == false {
                    match_and_apply_indefinite_effect(ctx, &mut target, &player_effect_clone);
                    *applied = true;
                    ctx.db.player_effects().id().update(player_effect);
                }
            }
        }
        
        if ctx.db.magician().id().find(target.id).is_some() {
            ctx.db.magician().id().update(target); // Updates target of effect - Recheck existence because effect can kill them in which case they will not exist anymore
        } 
    }
}

pub fn add_effects_to_table(ctx: &ReducerContext, effects: Vec<Effect>, target_id: u64, sender_id: u64, game_id: u32) -> bool { // Adds effect(s) to database - Insert blocked if invincible
    let magician_option = ctx.db.magician().id().find(target_id);
    if let Some(magician) = magician_option {
        if is_permission_unblocked(&magician.permissions, "Invincibled") { // If target is invincible, skip effect inserts
            for effect in effects {
                let effect_to_add = PlayerEffect { id: 0, target_id: target_id, sender_id: sender_id, game_id: game_id, effect_info: effect, effect_type: effect.effect_type};
                ctx.db.player_effects().insert(effect_to_add);
            } 
            return true;
        }

        else if sender_id == target_id {  // Effects can still be added if invincible, but they must be self applied. N/A to magician kit - safegaurd for future characters
            for effect in effects {
                let effect_to_add = PlayerEffect { id: 0, target_id: target_id, sender_id: sender_id, game_id: game_id, effect_info: effect, effect_type: effect.effect_type};
                ctx.db.player_effects().insert(effect_to_add);
            }
            return true;
        }
    }

    false  
} 
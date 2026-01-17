use spacetimedb::{reducer, ReducerContext, Table};
use crate::*;

#[reducer]
pub fn handle_player_effects_table(ctx: &ReducerContext, timer: PlayerEffectsTableTimer) 
{
    let time = timer.tick_rate;
    for mut player_effect in ctx.db.player_effects().game_id().filter(timer.game_id) {
        let mut magician = ctx.db.magician().id().find(player_effect.target_id).expect("Magician Not Found!");
        let player_effect_clone = player_effect.clone();
        let app_info = &mut player_effect.application_information;

        match app_info.application_type {
            
            ApplicationType::Single => {
                match_single_effect_to_method(ctx, &mut magician, &player_effect_clone);
                ctx.db.player_effects().id().delete(player_effect.id);
            },

            ApplicationType::Duration => {
                let current_time = app_info.current_time.as_mut().expect("Duration effect must have a current time");
                if *current_time == 0.0 {
                    match_duration_effect_to_method(ctx, &mut magician, &player_effect_clone);
                }
                *current_time += time;

                let end_time = app_info.end_time.as_ref().expect("Duration effect must have an end time");
                if *current_time >= *end_time {
                    // create & call method to undo duration effect on player once a duration effect is added
                    ctx.db.player_effects().id().delete(player_effect.id);
                }

                else { 
                    ctx.db.player_effects().id().update(player_effect);
                }

            },
            
            ApplicationType::Reapply => {
                let current_reapply_time = app_info.current_reapply_time.as_mut().expect("Reapply effect must have current reapply time");
                if *current_reapply_time == 0.0 {
                    match_reapply_effect_to_method(ctx, &mut magician, &player_effect_clone);
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
                    // create & call method to undo reapply effect on player
                    ctx.db.player_effects().id().delete(player_effect.id);
                }
                
                else {
                    ctx.db.player_effects().id().update(player_effect);
                }
                
            }
        }

        ctx.db.magician().id().update(magician);
    }
}

#[reducer]
pub fn add_effects_to_table(ctx: &ReducerContext, effects: Vec<Effect>, target_id: u64, sender_id: u64, game_id: u32) 
{
    for effect in effects {
        let effect_to_add = PlayerEffect { id: 0, target_id: target_id, sender_id: sender_id, game_id: game_id, effect_type: effect.effect_type, application_information: effect.application_information, damage_information: effect.damage_information, cloak_information: effect.cloak_information, dust_information: effect.dust_information, speed_information: effect.speed_information};
        ctx.db.player_effects().insert(effect_to_add);
    }
} 
use spacetimedb::{reducer, Identity, ReducerContext, Table};
use crate::*;

#[reducer]
pub fn handle_movement_request_magician(ctx: &ReducerContext, request: MovementRequest) 
{
    let mut character = ctx.db.magician().identity().find(ctx.sender).expect("Magician To Move Not Found");

    character.rotation = request.aim;
    character.velocity = DbVector3 { x: 0.0, y: character.velocity.y, z: 0.0 };
    character.kinematic_information.jump = false;
    let speed_mutiplier = character.combat_information.speed_multiplier;
    let stunned = is_permission_unblocked(&character.permissions, "Stunned") == false;
    let taroted = is_permission_unblocked(&character.permissions, "Taroted") == false;

    if is_permission_unblocked(&character.permissions, "CanWalk") && stunned == false {
        let mut local_x: f32 = 0.0;
        let mut local_z: f32 = 0.0;

        if request.move_forward && !request.move_backward {
            local_z = 2.0;
        } 
        
        else if request.move_backward && !request.move_forward {
            local_z = -2.0;
        }

        if request.move_right && !request.move_left {
            local_x = 2.0;
        } 
        
        else if request.move_left && !request.move_right {
            local_x = -2.0;
        }

        if is_permission_unblocked(&character.permissions, "CanRun") && request.sprint && request.move_forward && !request.move_backward{
            local_z *= 2.5;
        }

        if is_permission_unblocked(&character.permissions, "CanRun") && request.sprint {
            local_x *= 1.5;
        }

        let yaw_radians: f32 = to_radians(character.rotation.yaw);
        let cos_yaw: f32 = yaw_radians.cos();
        let sin_yaw: f32 = yaw_radians.sin();

        let world_x: f32 = cos_yaw * local_x + sin_yaw * local_z;
        let world_z: f32 = -sin_yaw * local_x + cos_yaw * local_z;

        character.velocity = DbVector3 { x: world_x, y: character.velocity.y, z: world_z };
    }

    if is_permission_unblocked(&character.permissions, "CanJump") && request.jump && stunned == false {
        character.kinematic_information.jump = true;
        character.velocity.y = 7.5;
    }

    if is_permission_unblocked(&character.permissions, "CanCrouch") && request.crouch && stunned == false {
        character.velocity = DbVector3 { x: character.velocity.x * 0.5, y: character.velocity.y, z: character.velocity.z * 0.5 };
        character.kinematic_information.crouched = true;
        add_subscriber_to_permission(&mut character.permissions, "CanRun", "Crouch");
    }

    if !request.crouch || stunned == true {
        character.kinematic_information.crouched = false;
        remove_subscriber_from_permission(&mut character.permissions, "CanRun", "Crouch");
    }

    character.velocity = if taroted { DbVector3 { x: character.velocity.x * speed_mutiplier * -1.0, y: character.velocity.y, z: character.velocity.z * speed_mutiplier -1.0 } } else { DbVector3 { x: character.velocity.x * speed_mutiplier, y: character.velocity.y, z: character.velocity.z * speed_mutiplier } };
    ctx.db.magician().identity().update(character);
}

#[reducer]
pub fn handle_action_change_request_magician(ctx: &ReducerContext, request: ActionRequestMagician) 
{
    let mut magician = ctx.db.magician().identity().find(ctx.sender).expect("Magician Not Found");
    let stunned = is_permission_unblocked(&magician.permissions, "Stunned") == false;
    let old_state: MagicianState = magician.state;

    if request.state == MagicianState::Attack && is_permission_unblocked(&magician.permissions, "CanAttack") && magician.bullets.len() > 0 && stunned == false {
        magician.state = MagicianState::Attack;
        add_subscriber_to_permission(&mut magician.permissions, "CanAttack", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanDust", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanHypnosis", "Attack");
        try_perform_attack(ctx, &mut magician, request.attack_information);
    } 
    
    else if request.state == MagicianState::Reload && is_permission_unblocked(&magician.permissions, "CanReload") && (magician.bullets.len() as i32) < magician.bullet_capacity && stunned == false {
        magician.state = MagicianState::Reload;
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Reload");
    }

    else if request.state == MagicianState::Dust && is_permission_unblocked(&magician.permissions, "CanDust") && stunned == false {
        magician.state = MagicianState::Dust;
        add_subscriber_to_permission(&mut magician.permissions, "CanDust", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanAttack", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanHypnosis", "Dust");
        try_perform_dust(ctx, &mut magician, request.dust_information)
    }
    
    else if request.state == MagicianState::Cloak && is_permission_unblocked(&magician.permissions, "CanCloak") && stunned == false {
        magician.state = MagicianState::Cloak;
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Cloak");
    }

    else if request.state == MagicianState::Hypnosis && is_permission_unblocked(&magician.permissions, "CanHypnosis") && stunned == false {
        magician.state = MagicianState::Hypnosis;
        add_subscriber_to_permission(&mut magician.permissions, "CanHypnosis", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanDust", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanAttack", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Hypnosis");
    }

    if old_state != magician.state {
        adjust_timer_for_interruptable_state(&mut magician, old_state);
        match old_state {
            MagicianState::Reload => {
                remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Reload");
            }

            MagicianState::Cloak => { }

            _ => {}
        }
    }

    if magician.state != MagicianState::Default && magician.state != MagicianState::Reload && magician.state != MagicianState::Cloak {
        try_interrupt_cloak_and_speed_effects_magician(ctx, &mut magician);
    }

    ctx.db.magician().identity().update(magician);
}

#[reducer]
pub fn handle_stateless_action_request_magician(ctx: &ReducerContext, request: StatelessActionRequestMagician) 
{
    let mut magician = ctx.db.magician().identity().find(ctx.sender).expect("Magician Not Found");

    if request.action == MagicianStatelessAction::Tarot && is_permission_unblocked(&magician.permissions, "CanTarot") {
        try_tarot(ctx, &mut magician);
        add_subscriber_to_permission(&mut magician.permissions, "CanTarot", "Tarot");
    }

    ctx.db.magician().identity().update(magician);
}

#[reducer]
pub fn handle_magician_timers(ctx: &ReducerContext, timer: HandleMagicianTimersTimer) {
    let time: f32 = timer.tick_rate;
    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) {
        match magician.state {
            MagicianState::Attack => {
                if tick_active_timer_and_check_expired(&mut magician, "Attack", time) {
                    if magician.bullets.len() > 0 {
                        magician.state = MagicianState::Default;
                    } 
                    
                    else {
                        magician.state = MagicianState::Reload;
                        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Reload"); // Add To Every Other State To Ensure Transition Back To Reload If Necessary
                    }

                    remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Attack");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanDust", "Attack");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Attack");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanHypnosis", "Attack");
                }
            }

            MagicianState::Reload => {  
                if tick_active_timer_and_check_expired(&mut magician, "Reload", time) {
                    magician.state = MagicianState::Default;
                    try_reload(ctx, &mut magician);
                }
            }

            MagicianState::Dust => {
                if tick_active_timer_and_check_expired(&mut magician, "Dust", time) {
                    magician.state = MagicianState::Default;
                    remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Dust");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanAttack", "Dust");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Dust");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanHypnosis", "Dust");
                }
            }

            MagicianState::Cloak => {
                if tick_active_timer_and_check_expired(&mut magician, "Cloak", time) {
                    magician.state = MagicianState::Default;
                    try_cloak(ctx, &mut magician);
                }
            }

            MagicianState::Hypnosis => {
                if tick_active_timer_and_check_expired(&mut magician, "Hypnosis", time) {
                    magician.state = MagicianState::Default;
                    remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Hypnosis");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanAttack", "Hypnosis");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Hypnosis");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanDust", "Hypnosis");
                    try_hypnosis(ctx, &mut magician);
                }
            }

            MagicianState::Default => {}
        }

        for i in 0..magician.timers.len() {
            if let Some(expired_timer_name) = tick_cooldown_timer_and_check_expired(&mut magician.timers[i], time) {
                match expired_timer_name.as_str() {
                    "Attack" => remove_subscriber_from_permission(&mut magician.permissions, "CanAttack", "Attack"),
                    "Reload" => remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Reload"),
                    "Dust" => remove_subscriber_from_permission(&mut magician.permissions, "CanDust", "Dust"),
                    "Cloak" => remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Cloak"),
                    "Hypnosis" => remove_subscriber_from_permission(&mut magician.permissions, "CanHypnosis", "Hypnosis"),
                    _ => {}
                }
            }
        }

        ctx.db.magician().identity().update(magician);
    }
}

#[reducer]
pub fn handle_magician_stateless_timers(ctx: &ReducerContext, timer: HandleMagicianStatelessTimersTimer) 
{ 
    let time: f32 = timer.tick_rate;
    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) { 
        for i in 0..magician.stateless_timers.len() {
            if let Some(expired_timer_name) = tick_stateless_cooldown_timer_and_check_expired(&mut magician.stateless_timers[i], time) { 
                match expired_timer_name.as_str() {
                    "Tarot" => remove_subscriber_from_permission(&mut magician.permissions, "CanTarot", "Tarot"),
                    _ => {}

                }
            }
        }
        
        ctx.db.magician().identity().update(magician);
    }
}

#[reducer]
pub fn apply_gravity_magician(ctx: &ReducerContext, timer: GravityTimerMagician) 
{
    let time: f32 = timer.tick_rate;
    for character_row in ctx.db.magician().game_id().filter(timer.game_id) {
        let mut character = character_row;

        if character.velocity.y > -10.0 { 
            character.velocity.y -= timer.gravity * time; 
        }

        else { 
            character.velocity.y = -10.0; 
        }

        ctx.db.magician().identity().update(character);
    }
}

#[reducer]
pub fn add_collision_entry_magician(ctx: &ReducerContext, entry: CollisionEntry, target_identity: Identity) 
{
    let mut magician = ctx.db.magician().identity().find(target_identity).expect("Magician (Sender) Not Found");
    if magician.collision_entries.contains(&entry) == false { 
        magician.collision_entries.push(entry); 
        ctx.db.magician().identity().update(magician);
    }  
}

#[reducer]
pub fn remove_collision_entry_magician(ctx: &ReducerContext, entry: CollisionEntry, target_identity: Identity) 
{
    let mut magician = ctx.db.magician().identity().find(target_identity).expect("Magician (Sender) Not Found");
    if let Some(index) = magician.collision_entries.iter().position(|existing| *existing == entry) {
        magician.collision_entries.swap_remove(index);
        ctx.db.magician().identity().update(magician);
    }
}

#[reducer]
pub fn move_magicians(ctx: &ReducerContext, timer: MoveAllMagiciansTimer) 
{
    let tick_time: f32 = timer.tick_rate;
    let min_time_step: f32 = 1e-4;
    let max_substeps: i32 = 4;

    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) {
        let was_grounded: bool = magician.kinematic_information.grounded;
        magician.kinematic_information.grounded = false;
        magician.is_colliding = false;
        magician.corrected_velocity = magician.velocity;

        let mut pre_contacts: Vec<CollisionContact> = Vec::new();
        for entry in magician.collision_entries.iter() {
            try_build_contact_for_entry(ctx, &magician, entry, &mut pre_contacts);
        }

        if pre_contacts.is_empty() == false {
            let input_velocity = magician.velocity;
            resolve_contacts(&mut magician, &pre_contacts, input_velocity);
        }

        let mut remaining_time: f32 = tick_time;
        let mut substep_count: i32 = 0;

        let mut post_contacts: Vec<CollisionContact> = Vec::new();

        while remaining_time > min_time_step && substep_count < max_substeps {
            substep_count += 1;
            let step_time: f32 = remaining_time / ((max_substeps - substep_count + 1) as f32);
            let step_velocity = if magician.is_colliding { magician.corrected_velocity } else { magician.velocity };

            magician.position = add(magician.position, mul(step_velocity, step_time));

            let collision_entry_count: usize = magician.collision_entries.len();
            for entry_index in 0..collision_entry_count {
                let entry: CollisionEntry = magician.collision_entries[entry_index];
                if try_force_overlap_for_entry(ctx, &mut magician, &entry, was_grounded) {
                    break;
                }
            }

            post_contacts.clear();
            for entry in magician.collision_entries.iter() {
                try_build_contact_for_entry(ctx, &magician, entry, &mut post_contacts);
            }

            if post_contacts.is_empty() == false {
                let input_velocity = magician.velocity;
                resolve_contacts(&mut magician, &post_contacts, input_velocity);
            }

            remaining_time -= step_time;
        }

        let final_step_velocity = if magician.is_colliding { magician.corrected_velocity } else { magician.velocity };

        let ground_stick_velocity_threshold: f32 = 2.0;
        let grounded_this_tick: bool = magician.kinematic_information.grounded;

        if grounded_this_tick == false && was_grounded && final_step_velocity.y.abs() < ground_stick_velocity_threshold {
            magician.kinematic_information.grounded = true;
        }

        adjust_grounded(ctx, was_grounded, &final_step_velocity, &mut magician);
        ctx.db.magician().identity().update(magician);
    }
}

#[reducer]
pub fn move_magicians_lag_test(ctx: &ReducerContext, timer: MoveAllMagiciansTimer)
{
    let delta_time: f32 = timer.tick_rate;

    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) {
        magician.position.x += magician.velocity.x * delta_time;
        magician.position.y += magician.velocity.y * delta_time;
        magician.position.z += magician.velocity.z * delta_time;

        if magician.position.y < 0.0 {
            magician.position.y = 0.0;

            if magician.velocity.y < 0.0 {
                magician.velocity.y = 0.0;
            }
        }

        ctx.db.magician().identity().update(magician);
    }
}

#[reducer]
pub fn hypnotise(ctx: &ReducerContext, camera_info: HypnosisCameraInformation)
{
    let mut magician = ctx.db.magician().identity().find(ctx.sender).expect("Magician Not Found!");

    let mut hypnosis_iterator = ctx.db.player_effects().target_and_type().filter((magician.id, EffectType::Hypnosis));
    let mut hypnosis_effect = match (hypnosis_iterator.next(), hypnosis_iterator.next()) {
        (None, _) => { return; },
        (Some(effect), None) => effect,
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Hypnosis Effect At Most!"),
    };

    let raycast = try_hypnotise(ctx, &mut magician, camera_info);
    let raycast_target_id_option: Option<u64> = match raycast.hit_type {
        RaycastHitType::Magician => Some(raycast.hit_entity_id),
        _ => None,
    };

    let hypnosis_information = hypnosis_effect.hypnosis_informaton.as_mut().expect("Hypnosis Effect Must Have Hypnosis Information");
    let last_target_id_option: Option<u64> = hypnosis_information.last_target_id;

    match (last_target_id_option, raycast_target_id_option) {
        (Some(last_target_id), Some(raycast_target_id)) if last_target_id == raycast_target_id => { } // Last Target Id And Raycast Target Id Are Both Not None And Are The Same Id Value

        (Some(last_target_id), Some(raycast_target_id)) => { // Last Target Id And Raycast Target Id Are Both Not None But Are Different Id Values
            let mut stunned_magician = ctx.db.magician().id().find(last_target_id).expect("Stunned Magician Must Exist!");
            let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, magician.id, EffectType::Stunned));

            let stunned_effect = match (stunned_iterator.next(), stunned_iterator.next()) {
                (None, _) => panic!("Stunned Iterator Should Have First Element!"),
                (Some(effect), None) => effect,
                (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"),
            };

            undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id);
            ctx.db.magician().id().update(stunned_magician);

            add_effects_to_table(ctx, vec![create_stunned_effect()], raycast_target_id, magician.id, magician.game_id);
            hypnosis_information.last_target_id = Some(raycast_target_id);
        }

        (None, Some(raycast_target_id)) => { // Last Target Id Is None But Raycast Target Has Id Value
            add_effects_to_table(ctx, vec![create_stunned_effect()], raycast_target_id, magician.id, magician.game_id);
            hypnosis_information.last_target_id = Some(raycast_target_id);
        }

        (Some(last_target_id), None) => { // Last Target Id Has Id Value But Raycast Target Is None
            let mut stunned_magician = ctx.db.magician().id().find(last_target_id).expect("Stunned Magician Must Exist!");
            let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, magician.id, EffectType::Stunned));

            let stunned_effect = match (stunned_iterator.next(), stunned_iterator.next()) {
                (None, _) => panic!("Stunned Iterator Should Have First Element!"),
                (Some(effect), None) => effect,
                (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"),
            };

            undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id);
            ctx.db.magician().id().update(stunned_magician);

            hypnosis_information.last_target_id = None;
        }

        (None, None) => { } // Both Are None
    }

    ctx.db.player_effects().id().update(hypnosis_effect);
}



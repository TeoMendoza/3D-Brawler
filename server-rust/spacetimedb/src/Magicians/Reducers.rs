use spacetimedb::{reducer, Identity, ReducerContext};
use crate::*;

#[reducer]
pub fn handle_movement_request_magician(ctx: &ReducerContext, request: MovementRequest) // Handles player request for movement and looking
{
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    if magician_option.is_none() { return; }
    let mut magician = magician_option.unwrap();

    magician.rotation = request.aim;
    magician.velocity = DbVector3 { x: 0.0, y: magician.velocity.y, z: 0.0 }; // Y velocity processed in gravity reducer
    magician.kinematic_information.jump = false;

    let speed_mutiplier = magician.combat_information.speed_multiplier;
    let stunned = is_permission_unblocked(&magician.permissions, "Stunned") == false;
    let taroted = is_permission_unblocked(&magician.permissions, "Taroted") == false;

    if is_permission_unblocked(&magician.permissions, "CanWalk") && stunned == false {
        let mut local_x: f32 = 0.0;
        let mut local_z: f32 = 0.0;

        if request.move_forward && !request.move_backward { // Opposite direction requests cancel
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

        if is_permission_unblocked(&magician.permissions, "CanRun") && request.sprint && request.move_forward && !request.move_backward { // Stronger forward sprint - No backward sprint currently
            local_z *= 2.5;
        }

        if is_permission_unblocked(&magician.permissions, "CanRun") && request.sprint { // Weaker sideways sprint
            local_x *= 1.5;
        }

        let yaw_radians: f32 = to_radians(magician.rotation.yaw);
        let cos_yaw: f32 = yaw_radians.cos();
        let sin_yaw: f32 = yaw_radians.sin();

        let world_x: f32 = cos_yaw * local_x + sin_yaw * local_z;
        let world_z: f32 = -sin_yaw * local_x + cos_yaw * local_z;

        magician.velocity = DbVector3 { x: world_x, y: magician.velocity.y, z: world_z }; // Converts where player is looking into properly adjusted world speeds
    }

    if is_permission_unblocked(&magician.permissions, "CanJump") && request.jump && stunned == false {
        magician.kinematic_information.jump = true;
        magician.velocity.y = 7.5;
    }

    if is_permission_unblocked(&magician.permissions, "CanCrouch") && request.crouch && stunned == false {
        magician.velocity = DbVector3 { x: magician.velocity.x * 0.5, y: magician.velocity.y, z: magician.velocity.z * 0.5 };
        magician.kinematic_information.crouched = true;
        add_subscriber_to_permission(&mut magician.permissions, "CanRun", "Crouch"); // Can't sprint while crouching
    }

    if !request.crouch || stunned == true { // Resets crouch if stunned
        magician.kinematic_information.crouched = false;
        remove_subscriber_from_permission(&mut magician.permissions, "CanRun", "Crouch"); // Sprint permission block removed
    }

    magician.velocity = if taroted { DbVector3 { x: magician.velocity.x * speed_mutiplier * -1.0, y: magician.velocity.y, z: magician.velocity.z * speed_mutiplier -1.0 } } else { DbVector3 { x: magician.velocity.x * speed_mutiplier, y: magician.velocity.y, z: magician.velocity.z * speed_mutiplier } }; // Reverses movement if taroted
    ctx.db.magician().identity().update(magician);
}

#[reducer]
pub fn handle_action_change_request_magician(ctx: &ReducerContext, request: ActionRequestMagician) // Handles player request for action state - State cases: full block, interuptable, interuptable with cooldown
{
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    if magician_option.is_none() { return; }
    let mut magician = magician_option.unwrap();

    let stunned = is_permission_unblocked(&magician.permissions, "Stunned") == false; // Prevents actions from being taken if stunned
    let old_state: MagicianState = magician.state;

    if request.state == MagicianState::Attack && is_permission_unblocked(&magician.permissions, "CanAttack") && magician.bullets.len() > 0 && stunned == false { // Case: full block
        magician.state = MagicianState::Attack;
        add_subscriber_to_permission(&mut magician.permissions, "CanAttack", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanDust", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Attack");
        add_subscriber_to_permission(&mut magician.permissions, "CanHypnosis", "Attack");
        try_perform_attack(ctx, &mut magician, request.attack_information); // Attack performed at request
    } 
    
    else if request.state == MagicianState::Reload && is_permission_unblocked(&magician.permissions, "CanReload") && (magician.bullets.len() as u8) < magician.bullet_capacity && stunned == false { // Case: interuptable
        magician.state = MagicianState::Reload;
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Reload");
    }

    else if request.state == MagicianState::Dust && is_permission_unblocked(&magician.permissions, "CanDust") && stunned == false { // Case: full block
        magician.state = MagicianState::Dust;
        add_subscriber_to_permission(&mut magician.permissions, "CanDust", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanAttack", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Dust");
        add_subscriber_to_permission(&mut magician.permissions, "CanHypnosis", "Dust");
        try_perform_dust(ctx, &mut magician, request.dust_information) // Dust performed at request
    }
    
    else if request.state == MagicianState::Cloak && is_permission_unblocked(&magician.permissions, "CanCloak") && stunned == false { // Case: interruptable with cooldown
        magician.state = MagicianState::Cloak;
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Cloak");
    }

    else if request.state == MagicianState::Hypnosis && is_permission_unblocked(&magician.permissions, "CanHypnosis") && stunned == false { // Case: full block
        magician.state = MagicianState::Hypnosis;
        add_subscriber_to_permission(&mut magician.permissions, "CanHypnosis", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanDust", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanAttack", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Hypnosis");
        add_subscriber_to_permission(&mut magician.permissions, "CanCloak", "Hypnosis");
    }

    if old_state != magician.state {
        adjust_timer_for_interruptable_state(&mut magician, old_state); // Adjusts timer according to type of state
        match old_state {
            MagicianState::Reload => { remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Reload"); } // Reload always available after interrupt (Case: interruptable)

            MagicianState::Cloak => { } // Cloak not avaialable after interrupt due to cooldown (Case: interruptable with cooldown)

            _ => {}
        }
    }

    if magician.state != MagicianState::Default && magician.state != MagicianState::Reload && magician.state != MagicianState::Cloak && magician.state != MagicianState::Stunned { // If magician takes offensive action, cloak effects if existing are interrupted
        try_interrupt_cloak_and_speed_effects_magician(ctx, &mut magician);
    }

    if magician.state != MagicianState::Default { // If magician takes any action, invincible effect if existing is interrupted
        try_interrupt_invincible_effect_magician(ctx, &mut magician);
    }

    ctx.db.magician().identity().update(magician);
}

#[reducer]
pub fn handle_stateless_action_request_magician(ctx: &ReducerContext, request: StatelessActionRequestMagician) // Handles player request for state that does not have a consume/use time
{
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    if magician_option.is_none() { return; }
    let mut magician = magician_option.unwrap();

    let stunned = is_permission_unblocked(&magician.permissions, "Stunned") == false;
    let mut took_stateless_action = false;

    if request.action == MagicianStatelessAction::Tarot && is_permission_unblocked(&magician.permissions, "CanTarot") && stunned == false { // Stateless actions still have cooldowns - Handled within the ability method itself
        try_tarot(ctx, &mut magician);
        add_subscriber_to_permission(&mut magician.permissions, "CanTarot", "Tarot");
        took_stateless_action = true;
    }

    if took_stateless_action { // If magician takes any stateless action, invincible effect if existing is interrupted
        try_interrupt_invincible_effect_magician(ctx, &mut magician);
    }

    ctx.db.magician().identity().update(magician);
}

#[reducer]
pub fn handle_magician_timers(ctx: &ReducerContext, timer: HandleMagicianTimersTimer) { // Handles timers for magician action states (stateful timers) - Cases: active state, inactive state
    let time = timer.tick_rate;
    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) {
        match magician.state { // Case: active state
            MagicianState::Attack => {
                if tick_active_timer_and_check_expired(&mut magician, "Attack", time) { // Checks if consume/use time is over, resets permissions and state accordingly
                    if magician.bullets.len() > 0 { // Switches to reload state automatically if no bullets
                        magician.state = MagicianState::Default;
                    } 
                    
                    else {
                        magician.state = MagicianState::Reload;
                        add_subscriber_to_permission(&mut magician.permissions, "CanReload", "Reload"); // Add To Every Other State To Ensure Transition Back To Reload (WIP)
                    }

                    remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Attack");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanDust", "Attack");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Attack");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanHypnosis", "Attack");
                }
            }

            MagicianState::Reload => {  
                if tick_active_timer_and_check_expired(&mut magician, "Reload", time) { // Checks if consume/use time is over, resets permissions and state accordingly
                    magician.state = MagicianState::Default;
                    try_reload(ctx, &mut magician); // Reload performed at end of use time
                }
            }

            MagicianState::Dust => {
                if tick_active_timer_and_check_expired(&mut magician, "Dust", time) { // Checks if consume/use time is over, resets permissions and state accordingly
                    magician.state = MagicianState::Default;
                    remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Dust");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanAttack", "Dust");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Dust");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanHypnosis", "Dust");
                }
            }

            MagicianState::Cloak => {
                if tick_active_timer_and_check_expired(&mut magician, "Cloak", time) { // Checks if consume/use time is over, resets permissions and state accordingly
                    magician.state = MagicianState::Default;
                    try_cloak(ctx, &mut magician); // Cloak performed at end of use time - Ensures grace period for preventing cloak effect interruption
                }
            }

            MagicianState::Hypnosis => {
                if tick_active_timer_and_check_expired(&mut magician, "Hypnosis", time) { // Checks if consume/use time is over, resets permissions and state accordingly
                    magician.state = MagicianState::Default;
                    remove_subscriber_from_permission(&mut magician.permissions, "CanReload", "Hypnosis");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanAttack", "Hypnosis");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanCloak", "Hypnosis");
                    remove_subscriber_from_permission(&mut magician.permissions, "CanDust", "Hypnosis");
                    try_hypnosis(ctx, &mut magician); // Hypnosis performed at end of use time
                }
            }

            MagicianState::Stunned => {}, // Stun state needs no permission blocking, as it is simpler to check whether the stun permission itself is active

            MagicianState::Default => {}
        }

        for i in 0..magician.timers.len() { // Case: inactive state(s)
            if let Some(expired_timer_name) = tick_cooldown_timer_and_check_expired(&mut magician.timers[i], time) { // Checks if cooldown time is over, resets permissions accordingly
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
pub fn handle_magician_stateless_timers(ctx: &ReducerContext, timer: HandleMagicianStatelessTimersTimer) // Handles timers for magician actionless states (stateless timers)
{ 
    let time = timer.tick_rate;
    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) { 
        for i in 0..magician.stateless_timers.len() {
            if let Some(expired_timer_name) = tick_stateless_cooldown_timer_and_check_expired(&mut magician.stateless_timers[i], time) { // Checks if cooldown time is over, resets permissions accordingly
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
pub fn apply_gravity_magician(ctx: &ReducerContext, timer: GravityTimerMagician) // Applies gravity to players
{
    let time = timer.tick_rate;
    for mut character in ctx.db.magician().game_id().filter(timer.game_id) {
        if character.velocity.y > -10.0 { 
            character.velocity.y -= timer.gravity * time; 
        }

        else { 
            character.velocity.y = -10.0; // Max gravity to ensure no huge speeds when jumping from high places
        }

        ctx.db.magician().identity().update(character);
    }
}

#[reducer]
pub fn add_collision_entry_magician(ctx: &ReducerContext, entry: CollisionEntry, _target_identity: Identity) // Adds a collision entry to a magician - Collision entry can be thought of as a possible object colliding with the target player
{
    let magician_option = ctx.db.magician().id().find(entry.id); // Self collision registry blocked on client side - If entry.id poses issues, use target_identity (ctx.sender works except with test player since test player same ctx.sender)
    if let Some(mut magician) = magician_option {
        if magician.collision_entries.contains(&entry) == false { // No duplicate entries - Should not happen 
            magician.collision_entries.push(entry); 
            ctx.db.magician().identity().update(magician);
        }  
    }
    
}

#[reducer]
pub fn remove_collision_entry_magician(ctx: &ReducerContext, entry: CollisionEntry, _target_identity: Identity) // Removes a collision entry to a magician
{
     let magician_option = ctx.db.magician().id().find(entry.id); // If entry.id poses issues, use target_identity (ctx.sender works except with test player since test player same ctx.sender)
     if let Some(mut magician) = magician_option { 
        if let Some(index) = magician.collision_entries.iter().position(|existing| *existing == entry) {
            magician.collision_entries.swap_remove(index);
            ctx.db.magician().identity().update(magician);
        }
    }
    
}

#[reducer]
pub fn move_magicians(ctx: &ReducerContext, timer: MoveAllMagiciansTimer) // Handles moving players - Collision detection and response handled in this reducer
{
    let tick_time: f32 = timer.tick_rate;
    let min_time_step: f32 = 1e-4;
    let max_substeps: i32 = 8; // Splits work into repeated smaller work to reduce phasing

    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) {
        let was_grounded: bool = magician.kinematic_information.grounded; // Store previous tick grounded data - Used to prevent jitter switching between grounded and falling due to collision response
        magician.kinematic_information.grounded = false; // Grounded is false unless proven otherwise
        magician.is_colliding = false;
        magician.corrected_velocity = magician.velocity; // Corrected velocity is what is used as the move velocity, .velocity is requested velocity - Requested velocity gets processed and adjusted to due collisions and output as a move velocity

        let mut pre_contacts: Vec<CollisionContact> = Vec::new();
        for entry in magician.collision_entries.iter() {
            try_build_contact_for_entry(ctx, &magician, entry, &mut pre_contacts); // Builds initial contacts at start of tick - Collisions are processed by iteration (discrete) and not continuously, thus we cannot assume player has been fully resolved of contacts
        }

        if pre_contacts.is_empty() == false {
            resolve_contacts(&mut magician, &pre_contacts); // Resolves contacts at beginning of tick
        }

        let mut remaining_time = tick_time; // Substep work begins here
        let mut substep_count: i32 = 0;
        let mut post_contacts: Vec<CollisionContact> = Vec::new();

        while remaining_time > min_time_step && substep_count < max_substeps {
            substep_count += 1;
            post_contacts.clear(); // Clears incase populated from last substep
            let step_time = remaining_time / ((max_substeps - substep_count + 1) as f32);
            let move_velocity = if magician.is_colliding { magician.corrected_velocity } else { magician.velocity }; // If colliding we use corrected velocity, otherwise we can use requested
            magician.position = add(magician.position, mul(move_velocity, step_time));

            let collision_entry_count: usize = magician.collision_entries.len();
            for entry_index in 0..collision_entry_count {
                let entry: CollisionEntry = magician.collision_entries[entry_index];
                if try_force_overlap_for_entry(ctx, &mut magician, &entry, was_grounded) { // Tries to force a collision - Only used to make ramps more sticky, otherwise unecessary (hoping to remove this functionality through optimization of physics simulation)
                    break;
                }
            }
            
            for entry in magician.collision_entries.iter() {
                try_build_contact_for_entry(ctx, &magician, entry, &mut post_contacts); // Builds contacts after move within substep
            }

            if post_contacts.is_empty() == false {
                resolve_contacts(&mut magician, &post_contacts); // Resolves contacts within substep
            }

            remaining_time -= step_time;
        }

        let final_step_velocity = if magician.is_colliding { magician.corrected_velocity } else { magician.velocity }; // Grab final moving velocity after tick

        let ground_stick_velocity_threshold: f32 = 2.0;
        let grounded_this_tick: bool = magician.kinematic_information.grounded; // Grounded after tick

        if grounded_this_tick == false && was_grounded && final_step_velocity.y.abs() < ground_stick_velocity_threshold { // If previous tick and current tick grounded do not agree, ensure y velocities are larger than threshold before treating as truly not grounded
            magician.kinematic_information.grounded = true;
        }

        adjust_grounded(ctx, was_grounded, &final_step_velocity, &mut magician); // Adjusts movement permissions based on whether grounded or in air
        ctx.db.magician().identity().update(magician);
    }
}

#[reducer]
pub fn move_magicians_lag_test(ctx: &ReducerContext, timer: MoveAllMagiciansTimer) // Test reducer to see lag difference and effect between basic movement simulation and our current physics simulation
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
pub fn hypnotise(ctx: &ReducerContext, camera_info: HypnosisCameraInformation) // Handles hypnosis ability camera information and subsequent effect application
{
    let magician_option = ctx.db.magician().identity().find(ctx.sender);
    if magician_option.is_none() { return; }
    let mut magician = magician_option.unwrap();

    let mut hypnosis_iterator = ctx.db.player_effects().target_and_type().filter((magician.id, EffectType::Hypnosis));
    let mut hypnosis_effect = match (hypnosis_iterator.next(), hypnosis_iterator.next()) {
        (None, _) => { return; },
        (Some(effect), None) => effect,
        (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Hypnosis Effect At Most!"), // Only unacceptable case, magician cannot have more than one hypnosis effect active (self applied)
    };

    let raycast = try_hypnotise(ctx, &mut magician, camera_info);
    let raycast_target_id_option = match raycast.hit_type {
        RaycastHitType::Magician => Some(raycast.hit_entity_id),
        _ => None
    };

    let hypnosis_information = hypnosis_effect.effect_info.hypnosis_information.as_mut().expect("Hypnosis Effect Must Have Hypnosis Information");
    let last_target_id_option = hypnosis_information.last_target_id;

    match (last_target_id_option, raycast_target_id_option) { // Matches current target and last target to corresponding logic - Cases: some & current = last, some & current != last, none & some, some & none, none & none
        (Some(last_target_id), Some(raycast_target_id)) if last_target_id == raycast_target_id => { } // Case: current = last

        (Some(last_target_id), Some(raycast_target_id)) => { // Case: current != last
            let stunned_magician_option = ctx.db.magician().id().find(last_target_id);
            if let Some(mut stunned_magician) = stunned_magician_option {
                let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, magician.id, EffectType::Stunned));
                let stunned_effect_option = match (stunned_iterator.next(), stunned_iterator.next()) {
                    (None, _) => None,
                    (Some(effect), None) => Some(effect),
                    (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"), // Only unacceptable case, target cannot have multiple stun effects from same sender
                };
                
                if let Some(stunned_effect) = stunned_effect_option {
                    undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id);
                    ctx.db.magician().id().update(stunned_magician);
                }
            }
            
            let applied = add_effects_to_table(ctx, vec![create_stunned_effect()], raycast_target_id, magician.id, magician.game_id);
            if applied { // Only store last target if effects get applied - Effects can be blocked if invincible
                hypnosis_information.last_target_id = Some(raycast_target_id);
            }
            
        }

        (None, Some(raycast_target_id)) => { // Case: none & some
            let applied = add_effects_to_table(ctx, vec![create_stunned_effect()], raycast_target_id, magician.id, magician.game_id);
            if applied { // Only store last target if effects get applied - Effects can be blocked if invincible
                hypnosis_information.last_target_id = Some(raycast_target_id);
            }
        }

        (Some(last_target_id), None) => { // Case: some & none
            let stunned_magician_option = ctx.db.magician().id().find(last_target_id);
            if let Some(mut stunned_magician) = stunned_magician_option {
                let mut stunned_iterator = ctx.db.player_effects().target_sender_and_type().filter((last_target_id, magician.id, EffectType::Stunned));

                let stunned_effect_option = match (stunned_iterator.next(), stunned_iterator.next()) {
                    (None, _) => None,
                    (Some(effect), None) => Some(effect),
                    (Some(_), Some(_)) => panic!("Target Magician Should Only Have One Stun Effect From Sender At Most!"), // Only unacceptable case, target cannot have multiple stun effects from same sender
                };
                
                if let Some(stunned_effect) = stunned_effect_option {
                    undo_and_delete_stunned_effect_magician(ctx, &mut stunned_magician, stunned_effect.id);
                    ctx.db.magician().id().update(stunned_magician);
                }  
            }
            
            hypnosis_information.last_target_id = None;
        }

        (None, None) => { } // Case: none & none
    }

    ctx.db.player_effects().id().update(hypnosis_effect);
}



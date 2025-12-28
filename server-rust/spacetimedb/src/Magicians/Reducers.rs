use spacetimedb::{reducer, Identity, ReducerContext};
use crate::*;

#[reducer]
pub fn handle_movement_request_magician(ctx: &ReducerContext, request: MovementRequest) 
{
    let mut character = ctx.db.magician().identity().find(ctx.sender).expect("Magician To Move Not Found");

    character.rotation = request.aim;
    character.velocity = DbVector3 { x: 0.0, y: character.velocity.y, z: 0.0 };
    character.kinematic_information.jump = false;

    if is_permission_unblocked(&character.player_permission_config, "CanWalk") {
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

        if is_permission_unblocked(&character.player_permission_config, "CanRun") && request.sprint && request.move_forward&& !request.move_backward{
            local_z *= 2.5;
        }

        if is_permission_unblocked(&character.player_permission_config, "CanRun") && request.sprint {
            local_x *= 1.5;
        }

        let yaw_radians: f32 = to_radians(character.rotation.yaw);
        let cos_yaw: f32 = yaw_radians.cos();
        let sin_yaw: f32 = yaw_radians.sin();

        let world_x: f32 = cos_yaw * local_x + sin_yaw * local_z;
        let world_z: f32 = -sin_yaw * local_x + cos_yaw * local_z;

        character.velocity = DbVector3 { x: world_x, y: character.velocity.y, z: world_z };
    }

    if is_permission_unblocked(&character.player_permission_config, "CanJump") && request.jump {
        character.kinematic_information.jump = true;
        character.velocity.y = 7.5;
    }

    if is_permission_unblocked(&character.player_permission_config, "CanCrouch") && request.crouch {
        character.velocity = DbVector3 { x: character.velocity.x * 0.5, y: character.velocity.y, z: character.velocity.z * 0.5 };
        character.kinematic_information.crouched = true;
        add_subscriber_to_permission(&mut character.player_permission_config, "CanRun", "Crouch");
    }

    if !request.crouch {
        character.kinematic_information.crouched = false;
        remove_subscriber_from_permission(&mut character.player_permission_config, "CanRun", "Crouch");
    }

    ctx.db.magician().identity().update(character);
}

#[reducer]
pub fn handle_action_change_request_magician(ctx: &ReducerContext, request: ActionRequestMagician) 
{
    let mut character = ctx.db.magician().identity().find(ctx.sender).expect("Magician Not Found");
    let old_state: MagicianState = character.state;

    if request.state == MagicianState::Attack && is_permission_unblocked(&character.player_permission_config, "CanAttack") && character.bullets.len() > 0 {
        character.state = MagicianState::Attack;
        add_subscriber_to_permission(&mut character.player_permission_config, "CanAttack", "Attack");
        add_subscriber_to_permission(&mut character.player_permission_config, "CanReload", "Attack");
        try_perform_attack(ctx, &mut character, request.attack_information);
    } 
    
    else if request.state == MagicianState::Reload && is_permission_unblocked(&character.player_permission_config, "CanReload") && (character.bullets.len() as i32) < character.bullet_capacity {
        character.state = MagicianState::Reload;
        add_subscriber_to_permission(&mut character.player_permission_config, "CanReload", "Reload");
    }

    if old_state != character.state {
        reset_all_timers(&mut character);
        match old_state {
            MagicianState::Attack => {
                remove_subscriber_from_permission(&mut character.player_permission_config, "CanAttack", "Attack");
                remove_subscriber_from_permission(&mut character.player_permission_config, "CanReload", "Attack");
            }

            MagicianState::Reload => {
                remove_subscriber_from_permission(&mut character.player_permission_config, "CanReload", "Reload");
            }

            MagicianState::Default => {}
        }
    }

    ctx.db.magician().identity().update(character);
}


#[reducer]
pub fn handle_magician_timers(ctx: &ReducerContext, timer: HandleMagicianTimersTimer) {
    let time: f32 = timer.tick_rate;

    for mut magician in ctx.db.magician().game_id().filter(timer.game_id) {
        match magician.state {
            MagicianState::Attack => {
                if tick_timer_and_check_expired(&mut magician, "Attack", time) {
                    if magician.bullets.len() > 0 {
                        magician.state = MagicianState::Default;
                    } 
                    
                    else {
                        magician.state = MagicianState::Reload;
                        add_subscriber_to_permission(&mut magician.player_permission_config, "CanReload", "Reload");
                    }

                    remove_subscriber_from_permission(&mut magician.player_permission_config, "CanAttack", "Attack");
                    remove_subscriber_from_permission(&mut magician.player_permission_config, "CanReload", "Attack");
                }
            }

            MagicianState::Reload => {
                if tick_timer_and_check_expired(&mut magician, "Reload", time) {
                    magician.state = MagicianState::Default;
                    remove_subscriber_from_permission(&mut magician.player_permission_config, "CanReload", "Reload");
                    try_reload(ctx, &mut magician);
                }
            }

            MagicianState::Default => {}
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
    let max_substeps: i32 = 8;

    for mut character in ctx.db.magician().game_id().filter(timer.game_id) {
        let was_grounded: bool = character.kinematic_information.grounded;
        character.kinematic_information.grounded = false;
        character.is_colliding = false;
        character.corrected_velocity = character.velocity;

        let mut pre_contacts: Vec<CollisionContact> = Vec::new();
        for entry in character.collision_entries.iter() {
            try_build_contact_for_entry(ctx, &character, entry, &mut pre_contacts);
        }

        if pre_contacts.is_empty() == false {
            let input_velocity = character.velocity;
            resolve_contacts(&mut character, &pre_contacts, input_velocity);
        }

        let mut remaining_time: f32 = tick_time;
        let mut substep_count: i32 = 0;

        let mut post_contacts: Vec<CollisionContact> = Vec::new();

        while remaining_time > min_time_step && substep_count < max_substeps {
            substep_count += 1;
            let step_time: f32 = remaining_time / ((max_substeps - substep_count + 1) as f32);
            let step_velocity = if character.is_colliding { character.corrected_velocity } else { character.velocity };

            character.position = add(character.position, mul(step_velocity, step_time));

            let collision_entry_count: usize = character.collision_entries.len();
            for entry_index in 0..collision_entry_count {
                let entry: CollisionEntry = character.collision_entries[entry_index];
                if try_force_overlap_for_entry(ctx, &mut character, &entry, was_grounded) {
                    break;
                }
            }

            post_contacts.clear();
            for entry in character.collision_entries.iter() {
                try_build_contact_for_entry(ctx, &character, entry, &mut post_contacts);
            }

            if post_contacts.is_empty() == false {
                let input_velocity = character.velocity;
                resolve_contacts(&mut character, &post_contacts, input_velocity);
            }

            remaining_time -= step_time;
        }

        let final_step_velocity = if character.is_colliding { character.corrected_velocity } else { character.velocity };

        let ground_stick_velocity_threshold: f32 = 2.0;
        let grounded_this_tick: bool = character.kinematic_information.grounded;

        if grounded_this_tick == false && was_grounded && final_step_velocity.y.abs() < ground_stick_velocity_threshold {
            character.kinematic_information.grounded = true;
        }

        adjust_grounded(ctx, was_grounded, &final_step_velocity, &mut character);
        ctx.db.magician().identity().update(character);
    }
}


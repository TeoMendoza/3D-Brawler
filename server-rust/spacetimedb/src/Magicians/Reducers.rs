use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;


#[spacetimedb::reducer]
pub fn HandleMovementRequestMagician(ctx: &ReducerContext, request: MovementRequest) {
    let mut character = ctx.db.magician().identity().find(ctx.sender).expect("Magician To Move Not Found");

    character.rotation = request.aim;
    character.velocity = DbVector3 { x: 0.0, y: character.velocity.y, z: 0.0 };
    character.kinematic_information.jump = false;

    let can_walk_blocked: bool = {
        let entry = GetPermissionEntry(&mut character.player_permission_config, "CanWalk").expect("Permission entry not found: CanWalk");
        entry.subscribers.len() != 0
    };

    if can_walk_blocked == false {
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

        let can_run_blocked: bool = {
            let entry = GetPermissionEntry(&mut character.player_permission_config, "CanRun").expect("Permission entry not found: CanRun");
            entry.subscribers.len() != 0
        };

        if can_run_blocked == false && request.sprint && request.move_forward && !request.move_backward { 
            local_z *= 2.5; 
        }

        if can_run_blocked == false && request.sprint { 
            local_x *= 1.5; 
        }

        let yaw_radians: f32 = ToRadians(character.rotation.yaw);
        let cos_yaw: f32 = yaw_radians.cos();
        let sin_yaw: f32 = yaw_radians.sin();

        let world_x: f32 = cos_yaw * local_x + sin_yaw * local_z;
        let world_z: f32 = -sin_yaw * local_x + cos_yaw * local_z;

        character.velocity = DbVector3 { x: world_x, y: character.velocity.y, z: world_z };
    }

    let can_jump_blocked: bool = {
        let entry = GetPermissionEntry(&mut character.player_permission_config, "CanJump").expect("Permission entry not found: CanJump");
        entry.subscribers.len() != 0
    };

    if can_jump_blocked == false && request.jump {
        character.kinematic_information.jump = true;
        character.velocity.y = 7.5;
    }

    let can_crouch_blocked: bool = {
        let entry = GetPermissionEntry(&mut character.player_permission_config, "CanCrouch").expect("Permission entry not found: CanCrouch");
        entry.subscribers.len() != 0
    };

    if can_crouch_blocked == false && request.crouch {
        character.velocity = DbVector3 { x: character.velocity.x * 0.5, y: character.velocity.y, z: character.velocity.z * 0.5 };
        character.kinematic_information.crouched = true;
        let can_run_entry = GetPermissionEntry(&mut character.player_permission_config, "CanRun").expect("Permission entry not found: CanRun");
        AddSubscriberUnique(&mut can_run_entry.subscribers, "Crouch");
    }

    if request.crouch == false {
        character.kinematic_information.crouched = false;
        let can_run_entry = GetPermissionEntry(&mut character.player_permission_config, "CanRun").expect("Permission entry not found: CanRun");
        RemoveSubscriber(&mut can_run_entry.subscribers, "Crouch");
    }

    ctx.db.magician().identity().update(character);
}

#[spacetimedb::reducer]
pub fn HandleActionChangeRequestMagician(ctx: &ReducerContext, request: ActionRequestMagician) {
    let mut character = ctx.db.magician().identity().find(ctx.sender).expect("Magician Not Found");
    let old_state: MagicianState = character.state;

    let can_attack_blocked: bool = {
        let entry = GetPermissionEntry(&mut character.player_permission_config, "CanAttack").expect("Permission entry not found: CanAttack");
        entry.subscribers.len() != 0
    };

    let can_reload_blocked: bool = {
        let entry = GetPermissionEntry(&mut character.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
        entry.subscribers.len() != 0
    };

    if request.state == MagicianState::Attack && can_attack_blocked == false && character.bullets.len() > 0 {
        character.state = MagicianState::Attack;
        let can_attack_entry = GetPermissionEntry(&mut character.player_permission_config, "CanAttack").expect("Permission entry not found: CanAttack");
        AddSubscriberUnique(&mut can_attack_entry.subscribers, "Attack");

        let can_reload_entry = GetPermissionEntry(&mut character.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
        AddSubscriberUnique(&mut can_reload_entry.subscribers, "Attack");

        TryPerformAttack(ctx, &mut character, request.attack_information);
    } 
    
    else if request.state == MagicianState::Reload && can_reload_blocked == false && (character.bullets.len() as i32) < character.bullet_capacity {
        character.state = MagicianState::Reload;
        let can_reload_entry = GetPermissionEntry(&mut character.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
        AddSubscriberUnique(&mut can_reload_entry.subscribers, "Reload");
    }

    let state_switched: bool = old_state != character.state;
    if state_switched {
        ResetAllTimers(&mut character);
        match old_state {
            MagicianState::Attack => {
                let can_attack_entry = GetPermissionEntry(&mut character.player_permission_config, "CanAttack").expect("Permission entry not found: CanAttack");
                RemoveSubscriber(&mut can_attack_entry.subscribers, "Attack");

                let can_reload_entry = GetPermissionEntry(&mut character.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
                RemoveSubscriber(&mut can_reload_entry.subscribers, "Attack");
            }

            MagicianState::Reload => {
                let can_reload_entry = GetPermissionEntry(&mut character.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
                RemoveSubscriber(&mut can_reload_entry.subscribers, "Reload");
            }

            MagicianState::Default => {}
        }
    }

    ctx.db.magician().identity().update(character);
}

#[spacetimedb::reducer]
pub fn HandleMagicianTimers(ctx: &ReducerContext, timer: HandleMagicianTimersTimer) {
    let time: f32 = timer.tick_rate;

    for magician_row in ctx.db.magician().game_id().filter(timer.game_id) {
        let mut magician = magician_row;

        match magician.state {
            MagicianState::Attack => {
                let timer_index = TryFindTimerIndex(&magician, "Attack");
                let mut current_timer = magician.timers[timer_index].clone();
                current_timer.current_time -= time;

                if current_timer.current_time <= 0.0 {
                    current_timer.current_time = current_timer.reset_time;

                    if magician.bullets.len() > 0 {
                        magician.state = MagicianState::Default;
                    } 
                    
                    else {
                        magician.state = MagicianState::Reload;
                        let can_reload_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
                        AddSubscriberUnique(&mut can_reload_entry.subscribers, "Reload");
                    }

                    let can_attack_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanAttack").expect("Permission entry not found: CanAttack");
                    RemoveSubscriber(&mut can_attack_entry.subscribers, "Attack");

                    let can_reload_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
                    RemoveSubscriber(&mut can_reload_entry.subscribers, "Attack");
                }

                magician.timers[timer_index] = current_timer;
            }

            MagicianState::Reload => {
                let timer_index = TryFindTimerIndex(&magician, "Reload");
                let mut current_timer = magician.timers[timer_index].clone();
                current_timer.current_time -= time;

                if current_timer.current_time <= 0.0 {
                    current_timer.current_time = current_timer.reset_time;
                    magician.state = MagicianState::Default;
                    let can_reload_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanReload").expect("Permission entry not found: CanReload");
                    RemoveSubscriber(&mut can_reload_entry.subscribers, "Reload");
                    TryReload(ctx, &mut magician);
                }

                magician.timers[timer_index] = current_timer;
            }

            MagicianState::Default => {}
        }

        ctx.db.magician().identity().update(magician);
    }
}

#[spacetimedb::reducer]
pub fn ApplyGravityMagician(ctx: &ReducerContext, timer: GravityTimerMagician) {
    let time: f32 = timer.tick_rate;

    for character_row in ctx.db.magician().game_id().filter(timer.game_id) {
        let mut character = character_row;

        if character.velocity.y > -10.0 { character.velocity.y -= timer.gravity * time; }
        else { character.velocity.y = -10.0; }

        ctx.db.magician().identity().update(character);
    }
}

#[spacetimedb::reducer]
pub fn AddCollisionEntryMagician(ctx: &ReducerContext, entry: CollisionEntry, target_identity: Identity) {
    let mut magician = ctx.db.magician().identity().find(target_identity).expect("Magician (Sender) Not Found");
    if magician.collision_entries.contains(&entry) == false { magician.collision_entries.push(entry); }
    ctx.db.magician().identity().update(magician);
}

#[spacetimedb::reducer]
pub fn RemoveCollisionEntryMagician(ctx: &ReducerContext, entry: CollisionEntry, target_identity: Identity) {
    let mut magician = ctx.db.magician().identity().find(target_identity).expect("Magician (Sender) Not Found");
    if magician.collision_entries.contains(&entry) { magician.collision_entries.retain(|existing| existing != &entry); }
    ctx.db.magician().identity().update(magician);
}

#[spacetimedb::reducer]
pub fn MoveMagicians(ctx: &ReducerContext, timer: MoveAllMagiciansTimer) {
    let tick_time: f32 = timer.tick_rate;
    let min_time_step: f32 = 1e-4;
    let max_substeps: i32 = 4;

    for character_row in ctx.db.magician().game_id().filter(timer.game_id) {
        let mut character = character_row;

        let was_grounded: bool = character.kinematic_information.grounded;
        character.kinematic_information.grounded = false;
        character.is_colliding = false;
        character.corrected_velocity = character.velocity;

        let mut pre_contacts: Vec<CollisionContact> = Vec::new();
        for entry in character.collision_entries.iter() {
            TryBuildContactForEntry(ctx, &character, entry, &mut pre_contacts);
        }

        if pre_contacts.len() > 0 {
            let input_velocity = character.velocity;
            ResolveContacts(&mut character, &pre_contacts, input_velocity);
        }

        let mut remaining_time: f32 = tick_time;
        let mut substep_count: i32 = 0;

        while remaining_time > min_time_step && substep_count < max_substeps {
            substep_count += 1;
            let step_time: f32 = remaining_time / ((max_substeps - substep_count + 1) as f32);
            let step_velocity = if character.is_colliding { character.corrected_velocity } else { character.velocity };

            character.position = DbVector3 {
                x: character.position.x + step_velocity.x * step_time,
                y: character.position.y + step_velocity.y * step_time,
                z: character.position.z + step_velocity.z * step_time,
            };

            let collision_entries_snapshot = character.collision_entries.clone();
            for entry in collision_entries_snapshot.iter() {
                if TryForceOverlapForEntry(ctx, &mut character, entry, was_grounded) {
                    break;
                }
            }

            let mut post_contacts: Vec<CollisionContact> = Vec::new();
            for entry in character.collision_entries.iter() {
                TryBuildContactForEntry(ctx, &character, entry, &mut post_contacts);
            }

            if post_contacts.len() > 0 {
                let input_velocity = character.velocity;
                ResolveContacts(&mut character, &post_contacts, input_velocity);
            }

            remaining_time -= step_time;
        }

        let final_step_velocity = if character.is_colliding { character.corrected_velocity } else { character.velocity };

        let ground_stick_velocity_threshold: f32 = 2.0;
        let grounded_this_tick: bool = character.kinematic_information.grounded;

        if grounded_this_tick == false && was_grounded && final_step_velocity.y.abs() < ground_stick_velocity_threshold {
            character.kinematic_information.grounded = true;
        }

        AdjustGrounded(ctx, &final_step_velocity, &mut character);
        ctx.db.magician().identity().update(character);
    }
}

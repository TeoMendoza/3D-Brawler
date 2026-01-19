use spacetimedb::{ReducerContext};
use glam::Quat;
use crate::*;

pub fn adjust_grounded(_ctx: &ReducerContext, was_grounded: bool, move_velocity: &DbVector3, magician: &mut Magician) 
{
    let grounded_now: bool = magician.kinematic_information.grounded;
    if grounded_now {
        magician.kinematic_information.falling = false;
    } 
    
    else {
        magician.kinematic_information.falling = move_velocity.y < -2.0;
    }

    if was_grounded == grounded_now { return; }
    if grounded_now {
        remove_subscriber_from_permission(&mut magician.permissions, "CanJump", "Jump");
        remove_subscriber_from_permission(&mut magician.permissions, "CanCrouch", "Jump");
    } 
    
    else {
        add_subscriber_to_permission(&mut magician.permissions, "CanJump", "Jump");
        add_subscriber_to_permission(&mut magician.permissions, "CanCrouch", "Jump");
    }
}


pub fn resolve_contacts(magician: &mut Magician, contacts: &Vec<CollisionContact>, input_velocity: DbVector3) 
{
    let world_up = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let min_ground_dot: f32 = 0.75;
    let depth_epsilon: f32 = 2e-3;
    let max_depth: f32 = 0.08;
    let correction_factor: f32 = 0.5;
    let target_penetration: f32 = 0.01;
    let max_position_correction: f32 = 0.015;
    let ground_stick_up_threshold: f32 = 0.03;
    let input_up_cancel_threshold: f32 = 0.03;

    let mut corrected_velocity = input_velocity;
    let mut has_any_position_correction: bool = false;
    let mut is_grounded_on_map: bool = false;
    let mut total_position_correction = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    for contact in contacts.iter() {
        let normal = contact.normal;

        let up_dot: f32 = dot(normal, world_up);
        if contact.collision_type == CollisionEntryType::Map && up_dot >= min_ground_dot {
            is_grounded_on_map = true;
        }

        let normal_velocity_component: f32 = dot(normal, corrected_velocity);
        if normal_velocity_component < 0.0 {
            corrected_velocity = sub(corrected_velocity, mul(normal, normal_velocity_component));
        }

        let mut depth: f32 = contact.penetration_depth - target_penetration;
        if depth > depth_epsilon {
            if depth > max_depth { depth = max_depth; }
            has_any_position_correction = true;
            total_position_correction = add(total_position_correction, mul(normal, depth));
        }
    }

    if has_any_position_correction {
        let correction_magnitude_sq: f32 = dot(total_position_correction, total_position_correction);
        if correction_magnitude_sq > 1e-8 {
            let mut total_position_correction_local = total_position_correction;
            let correction_magnitude: f32 = correction_magnitude_sq.sqrt();

            if correction_magnitude > max_position_correction {
                total_position_correction_local = mul(normalize_small_vector(total_position_correction_local, world_up), max_position_correction);
            }

            total_position_correction_local = mul(total_position_correction_local, correction_factor);
            magician.position = add(magician.position, total_position_correction_local);
        }
    }

    if is_grounded_on_map {
        let desired_horizontal_speed_sq: f32 = input_velocity.x * input_velocity.x + input_velocity.z * input_velocity.z;

        if desired_horizontal_speed_sq < 0.001 {
            corrected_velocity.x = 0.0;
            corrected_velocity.z = 0.0;
        } 
        
        if corrected_velocity.y <= ground_stick_up_threshold { 
            corrected_velocity.y = 0.0; 
        }

        if input_velocity.y <= input_up_cancel_threshold { 
            magician.velocity.y = 0.0; 
        }
    }

    magician.is_colliding = contacts.len() > 0;
    magician.corrected_velocity = corrected_velocity;
    magician.kinematic_information.grounded = magician.kinematic_information.grounded || is_grounded_on_map;
}

pub fn try_build_contact_for_entry(ctx: &ReducerContext, character_local: &Magician, collision_entry: &CollisionEntry, contacts: &mut Vec<CollisionContact>) -> bool 
{
    let position_a = character_local.position;
    let yaw_radians_a: f32 = to_radians(character_local.rotation.yaw);

    let collider_a: &Vec<ConvexHullCollider> = &character_local.collider.convex_hulls;
    let center_a_world = get_collider_center_world(&character_local.collider, position_a, yaw_radians_a);

    let mut gjk_result: GjkResult = Default::default();
    let mut epa_contact: Contact = Default::default();

    let gjk_iterations: i32 = 24;

    if collision_entry.entry_type == CollisionEntryType::Magician {
        let other_magician = ctx.db.magician().id().find(collision_entry.id).expect("Colliding Magician Not Found");
        if other_magician.id == character_local.id { return false; }

        let collider_b: &Vec<ConvexHullCollider> = &other_magician.collider.convex_hulls;
        let position_b = other_magician.position;
        let yaw_radians_b: f32 = to_radians(other_magician.rotation.yaw);

        if solve_gjk(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut gjk_result, gjk_iterations) == false { 
            return false; 
        }

        let center_b_world = get_collider_center_world(&other_magician.collider, position_b, yaw_radians_b);

        if epa_solve(&gjk_result, collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut epa_contact) {
            let contact_normal = compute_contact_normal(epa_contact.normal, center_a_world, center_b_world);
            contacts.push(CollisionContact { normal: contact_normal, penetration_depth: epa_contact.depth, collision_type: CollisionEntryType::Magician });
            return true;
        }

        return false;
    }

    if collision_entry.entry_type == CollisionEntryType::Map {
        let map_piece = ctx.db.map().id().find(collision_entry.id).expect("Colliding Map Piece Not Found");

        let collider_b: &Vec<ConvexHullCollider> = &map_piece.collider.convex_hulls;
        let position_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        let yaw_radians_b: f32 = 0.0;

        if solve_gjk(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut gjk_result, gjk_iterations) == false { 
            return false; 
        }

        let center_b_world = get_collider_center_world(&map_piece.collider, position_b, yaw_radians_b);

        if epa_solve(&gjk_result, collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut epa_contact) {
            let contact_normal = compute_contact_normal(epa_contact.normal, center_a_world, center_b_world);
            contacts.push(CollisionContact { normal: contact_normal, penetration_depth: epa_contact.depth, collision_type: CollisionEntryType::Map });
            return true;
        }

        return false;
    }

    false
}

pub fn try_force_overlap_for_entry(ctx: &ReducerContext, character: &mut Magician, entry: &CollisionEntry, was_grounded: bool) -> bool 
{
    if entry.entry_type != CollisionEntryType::Map { return false; }
    if was_grounded == false && character.kinematic_information.grounded == false { return false; }

    let upward_velocity_block_threshold: f32 = 0.03;
    if character.velocity.y > upward_velocity_block_threshold { return false; }

    let world_up = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };

    let min_ground_dot: f32 = 0.75;
    let floor_up_dot: f32 = 0.98;

    let max_vertical_gap_ramp: f32 = 0.045;
    let max_vertical_snap: f32 = 0.01;

    let tiny_overlap: f32 = 0.0005;
    let overlap_enable_gap: f32 = 0.01;

    let collider_a = &character.collider;

    let map_piece = ctx.db.map().id().find(entry.id).expect("Colliding Map Piece Not Found");
    let collider_b = &map_piece.collider;

    let position_a = character.position;
    let position_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    let yaw_a: f32 = to_radians(character.rotation.yaw);
    let yaw_b: f32 = 0.0;

    let mut distance_result: GjkDistanceResult = Default::default();

    let distance_iterations: i32 = 12;
    if solve_gjk_distance(collider_a, position_a, yaw_a, collider_b, position_b, yaw_b, &mut distance_result, distance_iterations) == false { 
        return false; 
    }

    let center_a_world = get_collider_center_world(&collider_a, position_a, yaw_a);
    let center_b_world = get_collider_center_world(&collider_b, position_b, yaw_b);

    let contact_normal = compute_contact_normal(distance_result.separation_direction, center_a_world, center_b_world);

    let up_dot: f32 = dot(contact_normal, world_up);
    if up_dot < min_ground_dot { return false; }
    if up_dot > floor_up_dot { return false; }

    let delta = sub(distance_result.point_on_a, distance_result.point_on_b);
    let vertical_gap: f32 = dot(delta, world_up);

    if vertical_gap <= 0.0 { return false; }
    if vertical_gap > max_vertical_gap_ramp { return false; }

    let mut snap_down: f32 = vertical_gap;
    if vertical_gap <= overlap_enable_gap { snap_down = vertical_gap + tiny_overlap; }
    if snap_down > max_vertical_snap { snap_down = max_vertical_snap; }
    if snap_down <= 1e-6 { return false; }

    character.position = add(character.position, mul(world_up, -snap_down));
    true
}

pub fn try_reload(_ctx: &ReducerContext, magician: &mut Magician) 
{
    let bullet_capacity: i32 = magician.bullet_capacity;
    let missing_bullets: i32 = bullet_capacity - magician.bullets.len() as i32;
    if missing_bullets <= 0 { return; }

    let mut new_bullets: Vec<ThrowingCard> = Vec::with_capacity(missing_bullets as usize);
    for _bullet_index in 0..missing_bullets {
        new_bullets.push(create_throwing_card());
    }

    magician.bullets.splice(0..0, new_bullets);
}


pub fn try_perform_attack(ctx: &ReducerContext, magician: &mut Magician, attack_information: AttackInformation) 
{
    let bullet = magician.bullets.pop().expect("No bullets");
    let effects = bullet.effects;

    let magician_position = magician.position;
    let magician_yaw_radians: f32 = to_radians(magician.rotation.yaw);
    let magician_yaw_only = Quat::from_rotation_y(magician_yaw_radians);

    let spawn_point = add(magician_position, rotate(attack_information.spawn_point_offset, magician_yaw_only));

    let camera_yaw_radians: f32 = to_radians(magician.rotation.yaw + attack_information.camera_yaw_offset);
    let camera_pitch_radians: f32 = to_radians(magician.rotation.pitch + attack_information.camera_pitch_offset);
    let camera_rotation = Quat::from_euler(glam::EulerRot::YXZ, camera_yaw_radians, camera_pitch_radians, 0.0);

    let camera_position = add(magician_position, rotate(attack_information.camera_position_offset, camera_rotation));
    let camera_forward = normalize_small_vector(rotate(DbVector3 { x: 0.0, y: 0.0, z: 1.0 }, camera_rotation), DbVector3 { x: 0.0, y: 0.0, z: 1.0 });

    let camera_to_spawn_distance = magnitude(sub(spawn_point, camera_position));
    let camera_max_distance = attack_information.max_distance + camera_to_spawn_distance;

    let camera_hit = raycast_match(ctx, camera_position, camera_forward, camera_max_distance);
    let aim_point = if camera_hit.hit { camera_hit.hit_point } else { add(camera_position, mul(camera_forward, camera_max_distance)) };

    let shot_delta = sub(aim_point, spawn_point);
    let shot_direction = normalize_small_vector(shot_delta, camera_forward);

    let shot_hit = raycast_match(ctx, spawn_point, shot_direction, attack_information.max_distance);
    if shot_hit.hit && shot_hit.hit_type == RaycastHitType::Magician {
        add_effects_to_table(ctx, effects, shot_hit.hit_entity_id, magician.id, magician.game_id);
    }
} 

pub fn try_perform_dust(ctx: &ReducerContext, magician: &mut Magician, dust_information: DustInformation)
{
    let magician_position = magician.position;
    let magician_yaw_radians: f32 = to_radians(magician.rotation.yaw);
    let magician_yaw_only = Quat::from_rotation_y(magician_yaw_radians);

    let spawn_point = add(magician_position, rotate(dust_information.spawn_point_offset, magician_yaw_only));

    let camera_yaw_radians: f32 = to_radians(magician.rotation.yaw + dust_information.camera_yaw_offset);
    let camera_pitch_radians: f32 = to_radians(magician.rotation.pitch + dust_information.camera_pitch_offset);
    let camera_rotation = Quat::from_euler(glam::EulerRot::YXZ, camera_yaw_radians, camera_pitch_radians, 0.0);

    let camera_position = add(magician_position, rotate(dust_information.camera_position_offset, camera_rotation));
    let camera_forward = normalize_small_vector(rotate(DbVector3 { x: 0.0, y: 0.0, z: 1.0 }, camera_rotation), DbVector3 { x: 0.0, y: 0.0, z: 1.0 });

    let camera_to_spawn_distance = magnitude(sub(spawn_point, camera_position));
    let camera_max_distance = dust_information.max_distance + camera_to_spawn_distance;

    let camera_hit: Raycast = raycast_match(ctx, camera_position, camera_forward, camera_max_distance);
    let aim_point: DbVector3 = if camera_hit.hit { camera_hit.hit_point } else { add(camera_position, mul(camera_forward, camera_max_distance)) };

    let cone_delta: DbVector3 = sub(aim_point, spawn_point);
    let cone_direction: DbVector3 = normalize_small_vector(cone_delta, camera_forward);

    let hits: Vec<Raycast> = raycast_cone_match(ctx, spawn_point, cone_direction, dust_information.max_distance, dust_information.cone_half_angle_degrees);
    for hit in hits {
        let dust_effect = create_dust_effect(2.0);
        let effects: Vec<Effect> = vec![dust_effect];

        add_effects_to_table(ctx, effects, hit.hit_entity_id, magician.id, magician.game_id);
    }
}

pub fn try_cloak(ctx: &ReducerContext, magician: &mut Magician)
{
    let cloak_effect = create_cloak_effect(10.0);
    let speed_effect = create_speed_multiplier_effect(1.25, 10.0);
    let effects: Vec<Effect> = vec![cloak_effect, speed_effect];

    add_effects_to_table(ctx, effects, magician.id, magician.id, magician.game_id);
}

pub fn try_hypnosis(ctx: &ReducerContext, magician: &mut Magician)
{
    let hypnosis_effect = create_hypnosis_effect(10.0);
    let effects: Vec<Effect> = vec![hypnosis_effect];

    add_effects_to_table(ctx, effects, magician.id, magician.id, magician.game_id);
}

pub fn try_hypnotise(ctx: &ReducerContext, magician: &mut Magician, camera_information: HypnosisCameraInformation) -> Raycast
{
    let magician_position = magician.position;
    let magician_yaw_radians: f32 = to_radians(magician.rotation.yaw);
    let magician_yaw_only = Quat::from_rotation_y(magician_yaw_radians);

    let spawn_point = add(magician_position, rotate(camera_information.spawn_point_offset, magician_yaw_only));

    let camera_yaw_radians: f32 = to_radians(magician.rotation.yaw + camera_information.camera_yaw_offset);
    let camera_pitch_radians: f32 = to_radians(magician.rotation.pitch + camera_information.camera_pitch_offset);
    let camera_rotation = Quat::from_euler(glam::EulerRot::YXZ, camera_yaw_radians, camera_pitch_radians, 0.0);

    let camera_position = add(magician_position, rotate(camera_information.camera_position_offset, camera_rotation));
    let camera_forward = normalize_small_vector(rotate(DbVector3 { x: 0.0, y: 0.0, z: 1.0 }, camera_rotation), DbVector3 { x: 0.0, y: 0.0, z: 1.0 });

    let camera_to_spawn_distance = magnitude(sub(spawn_point, camera_position));
    let camera_max_distance = camera_information.max_distance + camera_to_spawn_distance;

    let camera_hit = raycast_match(ctx, camera_position, camera_forward, camera_max_distance);
    let aim_point = if camera_hit.hit { camera_hit.hit_point } else { add(camera_position, mul(camera_forward, camera_max_distance)) };

    let shot_delta = sub(aim_point, spawn_point);
    let shot_direction = normalize_small_vector(shot_delta, camera_forward);

    let raycast = raycast_match(ctx, spawn_point, shot_direction, camera_information.max_distance);

    raycast
}

pub fn adjust_timer_in_use(magician: &mut Magician, key: &str)
{
    let timer: &mut Timer = try_find_timer(&mut magician.timers, key);

    if key == "Reload" {
        timer.current_time = 0.0;
        timer.state = TimerState::Inactive;
    }

    else if key == "Cloak" {
        timer.state = TimerState::InCooldown;
    }  
}

pub fn adjust_timer_for_interruptable_state(magician: &mut Magician, state: MagicianState)
{
    match state {
        MagicianState::Reload => adjust_timer_in_use(magician, "Reload"),
        MagicianState::Cloak => adjust_timer_in_use(magician, "Cloak"),
        _ => {}
    }
}

pub fn tick_active_timer_and_check_expired(magician: &mut Magician, key: &str, delta_time: f32) -> bool {
    let timer = try_find_timer(&mut magician.timers, key);
    timer.current_time += delta_time;

    if timer.state == TimerState::Inactive {
        timer.state = TimerState::InUse;
    }

    if timer.state == TimerState::InUse && timer.current_time >= timer.use_finished_time {
        timer.state = TimerState::InCooldown;
        return true;
    }

    false
}

pub fn tick_cooldown_timer_and_check_expired(timer: &mut Timer, delta_time: f32) -> Option<String> {
    if timer.state != TimerState::InCooldown {
        return None;
    }

    timer.current_time += delta_time;

    if timer.current_time >= timer.cooldown_time {
        timer.current_time = 0.0;
        timer.state = TimerState::Inactive;
        return Some(timer.name.clone());
    }

    None
}

pub fn try_find_timer<'a>(timers: &'a mut [Timer], key: &str) -> &'a mut Timer 
{
    for timer in timers.iter_mut() {
        if timer.name == key {
            return timer;
        }
    }
    panic!("Timer not found: {}", key);
}
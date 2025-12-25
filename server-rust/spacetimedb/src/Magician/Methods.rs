use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};

pub fn AdjustGrounded(ctx: &ReducerContext, move_velocity: &DbVector3, magician: &mut Magician) 
{
    if magician.kinematic_information.grounded {
        magician.kinematic_information.falling = false;
        let can_jump_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanJump").expect("Permission entry not found: CanJump");
        RemoveSubscriber(&mut can_jump_entry.subscribers, "Jump");

        let can_crouch_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanCrouch").expect("Permission entry not found: CanCrouch");
        RemoveSubscriber(&mut can_crouch_entry.subscribers, "Jump");
    } 
    
    else {
        magician.kinematic_information.falling = move_velocity.y < -2.0;
        let can_jump_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanJump").expect("Permission entry not found: CanJump");
        AddSubscriberUnique(&mut can_jump_entry.subscribers, "Jump");

        let can_crouch_entry = GetPermissionEntry(&mut magician.player_permission_config, "CanCrouch").expect("Permission entry not found: CanCrouch");
        AddSubscriberUnique(&mut can_crouch_entry.subscribers, "Jump");
    }
}

pub fn ResolveContacts(magician: &mut Magician, contacts: &Vec<CollisionContact>, input_velocity: &DbVector3) {
    let world_up = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let min_ground_dot: f32 = 0.75;
    let depth_epsilon: f32 = 2e-3;
    let max_depth: f32 = 0.08;
    let correction_factor: f32 = 0.5;
    let target_penetration: f32 = 0.01;
    let max_position_correction: f32 = 0.015;
    let ground_stick_up_threshold: f32 = 0.03;
    let input_up_cancel_threshold: f32 = 0.03;

    let mut corrected_velocity = input_velocity.clone();
    let mut is_grounded_on_map: bool = false;
    let mut ground_support_normal = world_up.clone();
    let mut has_any_position_correction: bool = false;
    let mut total_position_correction = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    for contact in contacts.iter() {
        let normal = contact.normal;

        let up_dot: f32 = Dot(&normal, &world_up);
        let is_walkable_map_contact: bool = contact.collision_type == CollisionEntryType::Map && up_dot >= min_ground_dot;

        if is_walkable_map_contact {
            is_grounded_on_map = true;
            ground_support_normal = normal;
        }

        let normal_velocity_component: f32 = Dot(&normal, &corrected_velocity);
        if normal_velocity_component < 0.0 {
            corrected_velocity = Sub(&corrected_velocity, &Mul(&normal, normal_velocity_component));
        }

        let mut depth: f32 = contact.penetration_depth - target_penetration;
        if depth > depth_epsilon {
            if depth > max_depth { depth = max_depth; }
            has_any_position_correction = true;
            total_position_correction = Add(&total_position_correction, &Mul(&normal, depth));
        }
    }

    if has_any_position_correction {
        let correction_magnitude_sq: f32 = Dot(&total_position_correction, &total_position_correction);
        if correction_magnitude_sq > 1e-8 {
            let mut total_position_correction_local = total_position_correction.clone();
            let correction_magnitude: f32 = correction_magnitude_sq.sqrt();

            if correction_magnitude > max_position_correction {
                total_position_correction_local = Mul(&Normalize(&total_position_correction_local), max_position_correction);
            }

            total_position_correction_local = Mul(&total_position_correction_local, correction_factor);
            magician.position = Add(&magician.position, &total_position_correction_local);
        }
    }

    if is_grounded_on_map {
        let desired_horizontal_speed_sq: f32 = input_velocity.x * input_velocity.x + input_velocity.z * input_velocity.z;

        if desired_horizontal_speed_sq < 0.001 {
            corrected_velocity.x = 0.0;
            corrected_velocity.z = 0.0;
        } else {
            let desired_horizontal_speed: f32 = desired_horizontal_speed_sq.sqrt();
            let preserved_corrected_y: f32 = corrected_velocity.y;

            let tangent_velocity = Sub(&corrected_velocity, &Mul(&ground_support_normal, Dot(&corrected_velocity, &ground_support_normal)));
            let tangent_speed_sq: f32 = Dot(&tangent_velocity, &tangent_velocity);

            if tangent_speed_sq > 1e-8 {
                let tangent_direction = Mul(&tangent_velocity, 1.0 / tangent_speed_sq.sqrt());

                let tangent_direction_horizontal_sq: f32 = tangent_direction.x * tangent_direction.x + tangent_direction.z * tangent_direction.z;
                if tangent_direction_horizontal_sq > 1e-8 {
                    let tangent_direction_horizontal: f32 = tangent_direction_horizontal_sq.sqrt();
                    let scale: f32 = desired_horizontal_speed / tangent_direction_horizontal;

                    let desired_tangent_velocity = Mul(&tangent_direction, scale);

                    corrected_velocity.x = desired_tangent_velocity.x;
                    corrected_velocity.z = desired_tangent_velocity.z;
                    corrected_velocity.y = preserved_corrected_y;
                }
            }
        }

        if corrected_velocity.y <= ground_stick_up_threshold { corrected_velocity.y = 0.0; }
        if input_velocity.y <= input_up_cancel_threshold { magician.velocity.y = 0.0; }
    }

    magician.is_colliding = contacts.len() > 0;
    magician.corrected_velocity = corrected_velocity;
    magician.kinematic_information.grounded = magician.kinematic_information.grounded || is_grounded_on_map;
}

pub fn TryBuildContactForEntry(ctx: &ReducerContext, character_local: &Magician, collision_entry: &CollisionEntry, contacts: &mut Vec<CollisionContact>) -> bool {
    let position_a = character_local.position;
    let yaw_radians_a: f32 = ToRadians(character_local.rotation.yaw);

    if collision_entry.entry_type == CollisionEntryType::Magician {
        let other_magician = ctx.db.magician().id().find(collision_entry.id).expect("Colliding Magician Not Found");
        if other_magician.id == character_local.id { return false; }

        let collider_a: &Vec<ConvexHullCollider> = &character_local.collider.convex_hulls;
        let collider_b: &Vec<ConvexHullCollider> = &other_magician.collider.convex_hulls;

        let position_b = other_magician.position;
        let yaw_radians_b: f32 = ToRadians(other_magician.rotation.yaw);

        let mut gjk_result_magician: GjkResult = Default::default();
        let intersects_magician: bool = SolveGjk(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut gjk_result_magician);
        if intersects_magician == false { return false; }

        let center_a_world = GetColliderCenterWorld(&character_local.collider, position_a, yaw_radians_a);
        let center_b_world = GetColliderCenterWorld(&other_magician.collider, position_b, yaw_radians_b);

        let mut epa_contact: Contact = Default::default();
        if EpaSolve(&gjk_result_magician, collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut epa_contact) {
            let contact_normal = ComputeContactNormal(epa_contact.normal, center_a_world, center_b_world);
            let penetration_depth = epa_contact.depth;
            contacts.push(CollisionContact { normal: contact_normal, penetration_depth, collision_type: CollisionEntryType::Magician });
            return true;
        }
    }

    if collision_entry.entry_type == CollisionEntryType::Map {
        let map_piece = ctx.db.map().id().find(collision_entry.id).expect("Colliding Map Piece Not Found");

        let collider_a: &Vec<ConvexHullCollider> = &character_local.collider.convex_hulls;
        let collider_b: &Vec<ConvexHullCollider> = &map_piece.collider.convex_hulls;

        let position_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        let yaw_radians_b: f32 = 0.0;

        let mut gjk_result_map: GjkResult = Default::default();
        let intersects_map: bool = SolveGjk(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut gjk_result_map);
        if intersects_map == false { return false; }

        let center_a_world = GetColliderCenterWorld(&character_local.collider, position_a, yaw_radians_a);
        let center_b_world = GetColliderCenterWorld(&map_piece.collider, position_b, yaw_radians_b);

        let mut epa_contact: Contact = Default::default();
        if EpaSolve(&gjk_result_map, collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, &mut epa_contact) {
            let contact_normal = ComputeContactNormal(epa_contact.normal, center_a_world, center_b_world);
            let penetration_depth = epa_contact.depth;
            contacts.push(CollisionContact { normal: contact_normal, penetration_depth, collision_type: CollisionEntryType::Map });
            return true;
        }
    }

    false
}

pub fn TryForceOverlapForEntry(ctx: &ReducerContext, character: &mut Magician, entry: &CollisionEntry, was_grounded: bool) -> bool {
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

    let collider_a_complex = character.collider;
    let collider_a: &Vec<ConvexHullCollider> = &collider_a_complex.convex_hulls;

    let map_piece = ctx.db.map().id().find(entry.id).expect("Colliding Map Piece Not Found");
    let collider_b_complex = map_piece.collider;
    let collider_b: &Vec<ConvexHullCollider> = &collider_b_complex.convex_hulls;

    let position_a = character.position;
    let position_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    let yaw_a: f32 = ToRadians(character.rotation.yaw);
    let yaw_b: f32 = 0.0;

    let mut distance_result: GjkDistanceResult = Default::default();
    if SolveGjkDistance(collider_a, position_a, yaw_a, collider_b, position_b, yaw_b, &mut distance_result) == false { return false; }

    let center_a_world = GetColliderCenterWorld(&collider_a_complex, position_a, yaw_a);
    let center_b_world = GetColliderCenterWorld(&collider_b_complex, position_b, yaw_b);

    let contact_normal = ComputeContactNormal(distance_result.separation_direction, center_a_world, center_b_world);

    let up_dot: f32 = Dot(&contact_normal, &world_up);
    if up_dot < min_ground_dot { return false; }
    if up_dot > floor_up_dot { return false; }

    let delta = Sub(&distance_result.point_on_a, &distance_result.point_on_b);
    let vertical_gap: f32 = Dot(&delta, &world_up);

    if vertical_gap <= 0.0 { return false; }
    if vertical_gap > max_vertical_gap_ramp { return false; }

    let mut snap_down: f32 = vertical_gap;
    if vertical_gap <= overlap_enable_gap { snap_down = vertical_gap + tiny_overlap; }
    if snap_down > max_vertical_snap { snap_down = max_vertical_snap; }
    if snap_down <= 1e-6 { return false; }

    character.position = Add(&character.position, &Mul(&world_up, -snap_down));
    true
}

pub fn TryReload(ctx: &ReducerContext, magician: &mut Magician) {
    let bullet_capacity: i32 = magician.bullet_capacity;
    let missing_bullets: i32 = bullet_capacity - magician.bullets.len() as i32;
    if missing_bullets <= 0 { return; }

    let mut new_bullets: Vec<ThrowingCard> = Vec::with_capacity(missing_bullets as usize);
    for _bullet_index in 0..missing_bullets {
        new_bullets.push(CreateThrowingCard());
    }

    magician.bullets.splice(0..0, new_bullets);
}

pub fn TryPerformAttack(ctx: &ReducerContext, magician: &mut Magician, attack_information: AttackInformation) {
    let last_index: usize = magician.bullets.len() - 1;
    let bullet: ThrowingCard = magician.bullets[last_index].clone();
    magician.bullets.remove(last_index);

    let magician_position = magician.position;

    let magician_yaw_radians: f32 = ToRadians(magician.rotation.yaw);
    let magician_yaw_only = Quat::from_rotation_y(magician_yaw_radians);

    let spawn_point = Add(magician_position, Rotate(attack_information.spawn_point_offset, magician_yaw_only));

    let camera_yaw_radians: f32 = ToRadians(magician.rotation.yaw + attack_information.camera_yaw_offset);
    let camera_pitch_radians: f32 = ToRadians(magician.rotation.pitch + attack_information.camera_pitch_offset);
    let camera_rotation = Quat::from_euler(glam::EulerRot::YXZ, camera_yaw_radians, camera_pitch_radians, 0.0);

    let camera_position = Add(magician_position, Rotate(attack_information.camera_position_offset, camera_rotation));
    let camera_forward = Normalize(Rotate(DbVector3 { x: 0.0, y: 0.0, z: 1.0 }, camera_rotation));

    let camera_hit = RaycastMatch(ctx, camera_position, camera_forward, attack_information.max_distance);
    let aim_point = if camera_hit.hit { camera_hit.hit_point } else { Add(camera_position, Mul(camera_forward, attack_information.max_distance)) };

    let shot_delta = Sub(aim_point, spawn_point);
    let shot_direction = Normalize(shot_delta);
    let shot_hit = RaycastMatch(ctx, spawn_point, shot_direction, attack_information.max_distance);

    if shot_hit.hit {
        log::info!("Hitscan Hit Type={} Distance={} EntityId={}", shot_hit.hit_type, shot_hit.hit_distance, shot_hit.hit_entity_id);
    } 
    
    else {
        log::info!("Hitscan Miss");
    }
}

pub fn ResetAllTimers(magician: &mut Magician) {
    for timer_index in 0..magician.timers.len() {
        let mut current_timer = magician.timers[timer_index].clone();
        current_timer.current_time = current_timer.reset_time;
        magician.timers[timer_index] = current_timer;
    }
}

pub fn TryFindTimerIndex(magician: &Magician, timer_name: &str) -> usize {
    for timer_index in 0..magician.timers.len() {
        if magician.timers[timer_index].name == timer_name { return timer_index; }
    }
    panic!("Requested Timer Not Found");
}
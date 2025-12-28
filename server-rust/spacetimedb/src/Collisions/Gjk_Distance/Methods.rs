use crate::*;

pub fn compute_distance_and_unit_separation_from_closest_point(closest_minkowski_point: DbVector3, distance_out: &mut f32, separation_direction_out: &mut DbVector3) {
    let len_sq: f32 = length_sq(closest_minkowski_point);

    if len_sq <= 1e-12 {
        *distance_out = 0.0;
        *separation_direction_out = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
        return;
    }

    let inv_len: f32 = 1.0 / len_sq.sqrt();
    *distance_out = 1.0 / inv_len;

    let direction = negate(closest_minkowski_point);
    *separation_direction_out = DbVector3 { x: direction.x * inv_len, y: direction.y * inv_len, z: direction.z * inv_len };
}

pub fn solve_gjk_distance(collider_a: &ComplexCollider, position_a: DbVector3, yaw_radians_a: f32, collider_b: &ComplexCollider, position_b: DbVector3, yaw_radians_b: f32, result_out: &mut GjkDistanceResult, max_iterations: i32) -> bool {
    let progress_epsilon: f32 = 1e-4;
    let mut simplex: Vec<GjkVertex> = Vec::with_capacity(4);
    let mut search_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };

    let convex_hulls_a = &collider_a.convex_hulls;
    let convex_hulls_b = &collider_b.convex_hulls;

    simplex.push(support_pair_world(convex_hulls_a, position_a, yaw_radians_a, convex_hulls_b, position_b, yaw_radians_b, search_direction));

    let mut current_distance: f32 = 0.0;
    let mut current_point_on_a = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut current_point_on_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut current_separation_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let mut closest_minkowski_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    for _iteration_index in 0..max_iterations {
        compute_distance_from_simplex(&simplex, &mut current_distance, &mut current_point_on_a, &mut current_point_on_b, &mut current_separation_direction, &mut closest_minkowski_point);

        if current_distance <= 1e-6 {
            *result_out = GjkDistanceResult { intersects: true, distance: 0.0, separation_direction: current_separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex: simplex.clone(), last_direction: search_direction };
            return true;
        }

        search_direction = negate(closest_minkowski_point);

        if length_sq(closest_minkowski_point) <= 1e-12 {
            *result_out = GjkDistanceResult { intersects: false, distance: current_distance, separation_direction: current_separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex: simplex.clone(), last_direction: search_direction };
            return true;
        }

        let support_vertex = support_pair_world(convex_hulls_a, position_a, yaw_radians_a, convex_hulls_b, position_b, yaw_radians_b, search_direction);

        let closest_dot: f32 = dot(closest_minkowski_point, search_direction);
        let support_dot: f32 = dot(support_vertex.minkowski_point, search_direction);

        if support_dot - closest_dot <= progress_epsilon {
            *result_out = GjkDistanceResult { intersects: false, distance: current_distance, separation_direction: current_separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex: simplex.clone(), last_direction: search_direction };
            return true;
        }

        simplex.push(support_vertex);

        let mut update_direction = search_direction;
        if update_simplex(&mut simplex, &mut update_direction) {
            let separation_direction = normalize_small_vector(update_direction, DbVector3 { x: 0.0, y: 1.0, z: 0.0 });
            let last_vertex = simplex[simplex.len() - 1];
            *result_out = GjkDistanceResult { intersects: true, distance: 0.0, separation_direction, point_on_a: last_vertex.support_point_a, point_on_b: last_vertex.support_point_b, simplex: simplex.clone(), last_direction: update_direction };
            return true;
        }
    }

    compute_distance_from_simplex(&simplex, &mut current_distance, &mut current_point_on_a, &mut current_point_on_b, &mut current_separation_direction, &mut closest_minkowski_point);
    *result_out = GjkDistanceResult { intersects: false, distance: current_distance, separation_direction: current_separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex, last_direction: negate(closest_minkowski_point) };
    true
}

pub fn compute_distance_from_simplex(simplex: &Vec<GjkVertex>, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) {
    if simplex.len() == 0 {
        *distance_out = 0.0;
        *point_on_a_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        *point_on_b_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        *separation_direction_out = DbVector3 { x: 0.0, y: 0.0, z: 1.0 };
        *closest_minkowski_point_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        return;
    }

    if simplex.len() == 1 {
        let a = simplex[0];

        *closest_minkowski_point_out = a.minkowski_point;
        *point_on_a_out = a.support_point_a;
        *point_on_b_out = a.support_point_b;

        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    if simplex.len() == 2 {
        compute_distance_from_segment(simplex[0], simplex[1], distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
        return;
    }

    if simplex.len() == 3 {
        compute_distance_from_triangle(simplex[0], simplex[1], simplex[2], distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
        return;
    }

    compute_distance_from_tetrahedron(simplex[0], simplex[1], simplex[2], simplex[3], distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
}

pub fn compute_distance_from_segment(vertex_a: GjkVertex, vertex_b: GjkVertex, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) {
    let a = vertex_a.minkowski_point;
    let b = vertex_b.minkowski_point;
    let ab = sub(b, a);

    let denominator: f32 = dot(ab, ab);
    let mut t: f32 = 0.0;

    if denominator > 1e-12 { t = clamp_01(-dot(a, ab) / denominator); }

    *closest_minkowski_point_out = add(a, mul(ab, t));

    *point_on_a_out = add(vertex_a.support_point_a, mul(sub(vertex_b.support_point_a, vertex_a.support_point_a), t));
    *point_on_b_out = add(vertex_a.support_point_b, mul(sub(vertex_b.support_point_b, vertex_a.support_point_b), t));

    compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
}

pub fn compute_distance_from_triangle(vertex_a: GjkVertex, vertex_b: GjkVertex, vertex_c: GjkVertex, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) {
    let a = vertex_a.minkowski_point;
    let b = vertex_b.minkowski_point;
    let c = vertex_c.minkowski_point;

    let ab = sub(b, a);
    let ac = sub(c, a);
    let ao = negate(a);

    let d1: f32 = dot(ab, ao);
    let d2: f32 = dot(ac, ao);

    if d1 <= 0.0 && d2 <= 0.0 {
        *closest_minkowski_point_out = a;
        *point_on_a_out = vertex_a.support_point_a;
        *point_on_b_out = vertex_a.support_point_b;
        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let bo = negate(b);
    let d3: f32 = dot(ab, bo);
    let d4: f32 = dot(ac, bo);

    if d3 >= 0.0 && d4 <= d3 {
        *closest_minkowski_point_out = b;
        *point_on_a_out = vertex_b.support_point_a;
        *point_on_b_out = vertex_b.support_point_b;
        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let vc: f32 = d1 * d4 - d3 * d2;
    if vc <= 0.0 && d1 >= 0.0 && d3 <= 0.0 {
        let v: f32 = d1 / (d1 - d3);

        *closest_minkowski_point_out = add(a, mul(ab, v));

        *point_on_a_out = add(vertex_a.support_point_a, mul(sub(vertex_b.support_point_a, vertex_a.support_point_a), v));
        *point_on_b_out = add(vertex_a.support_point_b, mul(sub(vertex_b.support_point_b, vertex_a.support_point_b), v));

        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let co = negate(c);
    let d5: f32 = dot(ab, co);
    let d6: f32 = dot(ac, co);

    if d6 >= 0.0 && d5 <= d6 {
        *closest_minkowski_point_out = c;
        *point_on_a_out = vertex_c.support_point_a;
        *point_on_b_out = vertex_c.support_point_b;
        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let vb: f32 = d5 * d2 - d1 * d6;
    if vb <= 0.0 && d2 >= 0.0 && d6 <= 0.0 {
        let w: f32 = d2 / (d2 - d6);

        *closest_minkowski_point_out = add(a, mul(ac, w));

        *point_on_a_out = add(vertex_a.support_point_a, mul(sub(vertex_c.support_point_a, vertex_a.support_point_a), w));
        *point_on_b_out = add(vertex_a.support_point_b, mul(sub(vertex_c.support_point_b, vertex_a.support_point_b), w));

        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let va: f32 = d3 * d6 - d5 * d4;
    if va <= 0.0 && (d4 - d3) >= 0.0 && (d5 - d6) >= 0.0 {
        let w: f32 = (d4 - d3) / ((d4 - d3) + (d5 - d6));

        let bc = sub(c, b);
        *closest_minkowski_point_out = add(b, mul(bc, w));

        *point_on_a_out = add(vertex_b.support_point_a, mul(sub(vertex_c.support_point_a, vertex_b.support_point_a), w));
        *point_on_b_out = add(vertex_b.support_point_b, mul(sub(vertex_c.support_point_b, vertex_b.support_point_b), w));

        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let denominator: f32 = va + vb + vc;
    if denominator <= 1e-12 {
        *closest_minkowski_point_out = a;
        *point_on_a_out = vertex_a.support_point_a;
        *point_on_b_out = vertex_a.support_point_b;
        compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
        return;
    }

    let inverse_denominator: f32 = 1.0 / denominator;
    let v_face: f32 = vb * inverse_denominator;
    let w_face: f32 = vc * inverse_denominator;
    let u_face: f32 = 1.0 - v_face - w_face;

    *closest_minkowski_point_out = add(add(mul(a, u_face), mul(b, v_face)), mul(c, w_face));

    *point_on_a_out = add(add(mul(vertex_a.support_point_a, u_face), mul(vertex_b.support_point_a, v_face)), mul(vertex_c.support_point_a, w_face));
    *point_on_b_out = add(add(mul(vertex_a.support_point_b, u_face), mul(vertex_b.support_point_b, v_face)), mul(vertex_c.support_point_b, w_face));

    compute_distance_and_unit_separation_from_closest_point(*closest_minkowski_point_out, distance_out, separation_direction_out);
}

pub fn compute_distance_from_tetrahedron(vertex_a: GjkVertex, vertex_b: GjkVertex, vertex_c: GjkVertex, vertex_d: GjkVertex, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) {
    let mut best_distance: f32 = f32::MAX;

    *distance_out = 0.0;
    *point_on_a_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    *point_on_b_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    *separation_direction_out = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    *closest_minkowski_point_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    evaluate_face(vertex_a, vertex_b, vertex_c, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
    evaluate_face(vertex_a, vertex_c, vertex_d, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
    evaluate_face(vertex_a, vertex_d, vertex_b, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
    evaluate_face(vertex_b, vertex_d, vertex_c, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
}

pub fn evaluate_face(face_a: GjkVertex, face_b: GjkVertex, face_c: GjkVertex, best_distance: &mut f32, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) {
    let mut face_distance: f32 = 0.0;
    let mut face_point_on_a = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut face_point_on_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut face_separation_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let mut face_closest_minkowski_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    compute_distance_from_triangle(face_a, face_b, face_c, &mut face_distance, &mut face_point_on_a, &mut face_point_on_b, &mut face_separation_direction, &mut face_closest_minkowski_point);

    if face_distance < *best_distance {
        *best_distance = face_distance;
        *distance_out = face_distance;
        *point_on_a_out = face_point_on_a;
        *point_on_b_out = face_point_on_b;
        *separation_direction_out = face_separation_direction;
        *closest_minkowski_point_out = face_closest_minkowski_point;
    }
}

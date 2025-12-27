use crate::*;

pub fn SolveGjkDistance(collider_a: &ComplexCollider, position_a: DbVector3, yaw_radians_a: f32, collider_b: &ComplexCollider, position_b: DbVector3, yaw_radians_b: f32, result_out: &mut GjkDistanceResult, max_iterations: i32) -> bool 
{
    let progress_epsilon: f32 = 1e-4;
    let mut simplex: Vec<GjkVertex> = Vec::with_capacity(4);
    let mut search_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    
    let convex_hulls_a = &collider_a.convex_hulls;
    let convex_hulls_b = &collider_b.convex_hulls;

    let initial_vertex = SupportPairWorld(convex_hulls_a, position_a, yaw_radians_a, convex_hulls_b, position_b, yaw_radians_b, search_direction);
    simplex.push(initial_vertex);

    for _iteration_index in 0..max_iterations {
        let mut current_distance: f32 = 0.0;
        let mut current_point_on_a = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        let mut current_point_on_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
        let mut current_separation_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
        let mut closest_minkowski_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

        ComputeDistanceFromSimplex(&simplex,&mut current_distance, &mut current_point_on_a, &mut current_point_on_b, &mut current_separation_direction, &mut closest_minkowski_point,);

        if current_distance <= 1e-6 {
            let separation_direction = if NearZero(current_separation_direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(current_separation_direction) };
            *result_out = GjkDistanceResult { intersects: true, distance: 0.0, separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex: simplex.clone(), last_direction: search_direction };
            return true;
        }

        search_direction = Negate(closest_minkowski_point);

        if NearZero(search_direction) {
            *result_out = GjkDistanceResult { intersects: false, distance: current_distance, separation_direction: current_separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex: simplex.clone(), last_direction: search_direction };
            return true;
        }

        let support_vertex = SupportPairWorld(convex_hulls_a, position_a, yaw_radians_a, convex_hulls_b, position_b, yaw_radians_b, search_direction);

        let closest_dot: f32 = Dot(closest_minkowski_point, search_direction);
        let support_dot: f32 = Dot(support_vertex.minkowski_point, search_direction);
        let progress: f32 = support_dot - closest_dot;

        if progress <= progress_epsilon {
            *result_out = GjkDistanceResult { intersects: false, distance: current_distance, separation_direction: current_separation_direction, point_on_a: current_point_on_a, point_on_b: current_point_on_b, simplex: simplex.clone(), last_direction: search_direction };
            return true;
        }

        simplex.push(support_vertex);

        let mut update_direction = search_direction;
        if UpdateSimplex(&mut simplex, &mut update_direction) {
            let separation_direction = if NearZero(update_direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(update_direction) };
            let last_vertex = simplex[simplex.len() - 1];
            *result_out = GjkDistanceResult { intersects: true, distance: 0.0, separation_direction, point_on_a: last_vertex.support_point_a, point_on_b: last_vertex.support_point_b, simplex: simplex.clone(), last_direction: update_direction };
            return true;
        }
    }

    let mut final_distance: f32 = 0.0;
    let mut final_point_on_a = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut final_point_on_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut final_separation_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let mut final_closest_minkowski_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    ComputeDistanceFromSimplex(&simplex, &mut final_distance, &mut final_point_on_a, &mut final_point_on_b, &mut final_separation_direction, &mut final_closest_minkowski_point);
    *result_out = GjkDistanceResult { intersects: false, distance: final_distance, separation_direction: final_separation_direction, point_on_a: final_point_on_a, point_on_b: final_point_on_b, simplex, last_direction: Negate(final_closest_minkowski_point) };
    true
}

pub fn ComputeDistanceFromSimplex(simplex: &Vec<GjkVertex>, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) 
{
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

        *distance_out = Length(a.minkowski_point);

        let direction = Negate(a.minkowski_point);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    if simplex.len() == 2 {
        ComputeDistanceFromSegment(simplex[0], simplex[1], distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
        return;
    }

    if simplex.len() == 3 {
        ComputeDistanceFromTriangle(simplex[0], simplex[1], simplex[2], distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
        return;
    }

    ComputeDistanceFromTetrahedron(simplex[0], simplex[1], simplex[2], simplex[3], distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
}

pub fn ComputeDistanceFromSegment(vertex_a: GjkVertex, vertex_b: GjkVertex, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) 
{
    let a = vertex_a.minkowski_point;
    let b = vertex_b.minkowski_point;
    let ab = Sub(b, a);

    let denominator: f32 = Dot(ab, ab);
    let mut t: f32 = 0.0;

    if denominator > 1e-12 { t = Clamp01(-Dot(a, ab) / denominator); }

    *closest_minkowski_point_out = Add(a, Mul(ab, t));

    *point_on_a_out = Add(vertex_a.support_point_a, Mul(Sub(vertex_b.support_point_a, vertex_a.support_point_a), t));
    *point_on_b_out = Add(vertex_a.support_point_b, Mul(Sub(vertex_b.support_point_b, vertex_a.support_point_b), t));

    *distance_out = Length(*closest_minkowski_point_out);

    let direction = Negate(*closest_minkowski_point_out);
    *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
}

pub fn ComputeDistanceFromTriangle(vertex_a: GjkVertex, vertex_b: GjkVertex, vertex_c: GjkVertex, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) 
{
    let a = vertex_a.minkowski_point;
    let b = vertex_b.minkowski_point;
    let c = vertex_c.minkowski_point;

    let ab = Sub(b, a);
    let ac = Sub(c, a);
    let ao = Negate(a);

    let d1: f32 = Dot(ab, ao);
    let d2: f32 = Dot(ac, ao);

    if d1 <= 0.0 && d2 <= 0.0 {
        *closest_minkowski_point_out = a;
        *point_on_a_out = vertex_a.support_point_a;
        *point_on_b_out = vertex_a.support_point_b;
        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let bo = Negate(b);
    let d3: f32 = Dot(ab, bo);
    let d4: f32 = Dot(ac, bo);

    if d3 >= 0.0 && d4 <= d3 {
        *closest_minkowski_point_out = b;
        *point_on_a_out = vertex_b.support_point_a;
        *point_on_b_out = vertex_b.support_point_b;
        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let vc: f32 = d1 * d4 - d3 * d2;
    if vc <= 0.0 && d1 >= 0.0 && d3 <= 0.0 {
        let v: f32 = d1 / (d1 - d3);

        *closest_minkowski_point_out = Add(a, Mul(ab, v));

        *point_on_a_out = Add(vertex_a.support_point_a, Mul(Sub(vertex_b.support_point_a, vertex_a.support_point_a), v));
        *point_on_b_out = Add(vertex_a.support_point_b, Mul(Sub(vertex_b.support_point_b, vertex_a.support_point_b), v));

        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let co = Negate(c);
    let d5: f32 = Dot(ab, co);
    let d6: f32 = Dot(ac, co);

    if d6 >= 0.0 && d5 <= d6 {
        *closest_minkowski_point_out = c;
        *point_on_a_out = vertex_c.support_point_a;
        *point_on_b_out = vertex_c.support_point_b;
        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let vb: f32 = d5 * d2 - d1 * d6;
    if vb <= 0.0 && d2 >= 0.0 && d6 <= 0.0 {
        let w: f32 = d2 / (d2 - d6);

        *closest_minkowski_point_out = Add(a, Mul(ac, w));

        *point_on_a_out = Add(vertex_a.support_point_a, Mul(Sub(vertex_c.support_point_a, vertex_a.support_point_a), w));
        *point_on_b_out = Add(vertex_a.support_point_b, Mul(Sub(vertex_c.support_point_b, vertex_a.support_point_b), w));

        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let va: f32 = d3 * d6 - d5 * d4;
    if va <= 0.0 && (d4 - d3) >= 0.0 && (d5 - d6) >= 0.0 {
        let w: f32 = (d4 - d3) / ((d4 - d3) + (d5 - d6));

        let bc = Sub(c, b);
        *closest_minkowski_point_out = Add(b, Mul(bc, w));

        *point_on_a_out = Add(vertex_b.support_point_a, Mul(Sub(vertex_c.support_point_a, vertex_b.support_point_a), w));
        *point_on_b_out = Add(vertex_b.support_point_b, Mul(Sub(vertex_c.support_point_b, vertex_b.support_point_b), w));

        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let denominator: f32 = va + vb + vc;
    if denominator <= 1e-12 {
        *closest_minkowski_point_out = a;
        *point_on_a_out = vertex_a.support_point_a;
        *point_on_b_out = vertex_a.support_point_b;
        *distance_out = Length(*closest_minkowski_point_out);
        let direction = Negate(*closest_minkowski_point_out);
        *separation_direction_out = if NearZero(direction) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction) };
        return;
    }

    let inverse_denominator: f32 = 1.0 / denominator;
    let v_face: f32 = vb * inverse_denominator;
    let w_face: f32 = vc * inverse_denominator;
    let u_face: f32 = 1.0 - v_face - w_face;

    *closest_minkowski_point_out = Add(Add(Mul(a, u_face), Mul(b, v_face)), Mul(c, w_face));

    *point_on_a_out = Add(Add(Mul(vertex_a.support_point_a, u_face), Mul(vertex_b.support_point_a, v_face)), Mul(vertex_c.support_point_a, w_face));
    *point_on_b_out = Add(Add(Mul(vertex_a.support_point_b, u_face), Mul(vertex_b.support_point_b, v_face)), Mul(vertex_c.support_point_b, w_face));

    *distance_out = Length(*closest_minkowski_point_out);

    let direction_face = Negate(*closest_minkowski_point_out);
    *separation_direction_out = if NearZero(direction_face) { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { Normalize(direction_face) };
}

pub fn ComputeDistanceFromTetrahedron(vertex_a: GjkVertex, vertex_b: GjkVertex, vertex_c: GjkVertex, vertex_d: GjkVertex, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) 
{
    let mut best_distance: f32 = f32::MAX;

    *distance_out = 0.0;
    *point_on_a_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    *point_on_b_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    *separation_direction_out = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    *closest_minkowski_point_out = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    EvaluateFace(vertex_a, vertex_b, vertex_c, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
    EvaluateFace(vertex_a, vertex_c, vertex_d, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
    EvaluateFace(vertex_a, vertex_d, vertex_b, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
    EvaluateFace(vertex_b, vertex_d, vertex_c, &mut best_distance, distance_out, point_on_a_out, point_on_b_out, separation_direction_out, closest_minkowski_point_out);
}

pub fn EvaluateFace(face_a: GjkVertex, face_b: GjkVertex, face_c: GjkVertex, best_distance: &mut f32, distance_out: &mut f32, point_on_a_out: &mut DbVector3, point_on_b_out: &mut DbVector3, separation_direction_out: &mut DbVector3, closest_minkowski_point_out: &mut DbVector3) 
{
    let mut face_distance: f32 = 0.0;
    let mut face_point_on_a = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut face_point_on_b = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut face_separation_direction = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let mut face_closest_minkowski_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    ComputeDistanceFromTriangle(face_a, face_b, face_c, &mut face_distance, &mut face_point_on_a, &mut face_point_on_b, &mut face_separation_direction, &mut face_closest_minkowski_point);

    if face_distance < *best_distance {
        *best_distance = face_distance;
        *distance_out = face_distance;
        *point_on_a_out = face_point_on_a;
        *point_on_b_out = face_point_on_b;
        *separation_direction_out = face_separation_direction;
        *closest_minkowski_point_out = face_closest_minkowski_point;
    }
}

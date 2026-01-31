use crate::*;

pub fn solve_gjk(collider_a: &Vec<ConvexHullCollider>, position_a: DbVector3, yaw_radians_a: f32, collider_b: &Vec<ConvexHullCollider>, position_b: DbVector3, yaw_radians_b: f32, result_out: &mut GjkResult, max_iterations: i32) -> bool { // Returns whether two objects are colliding with information to rebuild accurate normal and depth
    let mut simplex: Vec<GjkVertex> = Vec::with_capacity(4);
    let mut search_direction = DbVector3 { x: 0.0, y: 0.0, z: 1.0 };

    let initial_vertex = support_pair_world(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, search_direction);
    let initial_dot: f32 = dot(initial_vertex.minkowski_point, search_direction);

    if initial_dot <= 0.0 {
        *result_out = GjkResult { intersects: false, simplex, last_direction: negate(search_direction) };
        return false;
    }

    simplex.push(initial_vertex);
    search_direction = negate(simplex[0].minkowski_point);

    for _iteration_index in 0..max_iterations {
        let support_vertex = support_pair_world(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, search_direction);
        let support_dot: f32 = dot(support_vertex.minkowski_point, search_direction);

        if support_dot <= 0.0 {
            *result_out = GjkResult { intersects: false, simplex, last_direction: negate(search_direction) };
            return false;
        }

        simplex.push(support_vertex);

        if update_simplex(&mut simplex, &mut search_direction) {
            *result_out = GjkResult { intersects: true, simplex, last_direction: negate(search_direction) };
            return true;
        }
    }

    *result_out = GjkResult { intersects: false, simplex, last_direction: negate(search_direction) };
    false
}

pub fn update_simplex(simplex: &mut Vec<GjkVertex>, search_direction: &mut DbVector3) -> bool {
    let simplex_count: usize = simplex.len();

    if simplex_count == 2 {
        let point_a = simplex[1].minkowski_point;
        let point_b = simplex[0].minkowski_point;

        let segment_ba = sub(point_b, point_a);
        let vector_to_origin_from_a = negate(point_a);

        if dot(segment_ba, vector_to_origin_from_a) <= 0.0 {
            simplex[0] = simplex[1];
            simplex.pop();
            *search_direction = vector_to_origin_from_a;
            return false;
        }

        let mut new_direction = triple_cross(segment_ba, vector_to_origin_from_a, segment_ba);
        if near_zero(new_direction) { new_direction = perpendicular(segment_ba); }

        *search_direction = new_direction;
        return false;
    }

    if simplex_count == 3 {
        let point_a = simplex[2].minkowski_point;
        let point_b = simplex[1].minkowski_point;
        let point_c = simplex[0].minkowski_point;

        let edge_ab = sub(point_b, point_a);
        let edge_ac = sub(point_c, point_a);
        let vector_to_origin_from_a = negate(point_a);
        let triangle_normal_abc = cross(edge_ab, edge_ac);

        let dot_ab_ao: f32 = dot(edge_ab, vector_to_origin_from_a);
        let dot_ac_ao: f32 = dot(edge_ac, vector_to_origin_from_a);

        if dot_ab_ao <= 0.0 && dot_ac_ao <= 0.0 {
            let vertex_a = simplex[2];
            simplex.clear();
            simplex.push(vertex_a);
            *search_direction = vector_to_origin_from_a;
            return false;
        }

        if dot_ab_ao > 0.0 {
            let mut perpendicular_toward_ab = triple_cross(edge_ac, edge_ab, edge_ab);
            if dot(perpendicular_toward_ab, vector_to_origin_from_a) > 0.0 {
                simplex[0] = simplex[1];
                simplex[1] = simplex[2];
                simplex.pop();
                if near_zero(perpendicular_toward_ab) { perpendicular_toward_ab = perpendicular(edge_ab); }
                *search_direction = perpendicular_toward_ab;
                return false;
            }
        }

        if dot_ac_ao > 0.0 {
            let mut perpendicular_toward_ac = triple_cross(edge_ab, edge_ac, edge_ac);
            if dot(perpendicular_toward_ac, vector_to_origin_from_a) > 0.0 {
                simplex[1] = simplex[2];
                simplex.pop();
                if near_zero(perpendicular_toward_ac) { perpendicular_toward_ac = perpendicular(edge_ac); }
                *search_direction = perpendicular_toward_ac;
                return false;
            }
        }

        let mut oriented_normal = triangle_normal_abc;
        if dot(oriented_normal, vector_to_origin_from_a) < 0.0 { oriented_normal = negate(oriented_normal); }

        *search_direction = oriented_normal;
        return false;
    }

    if simplex_count == 4 {
        let point_a = simplex[3].minkowski_point;
        let point_b = simplex[2].minkowski_point;
        let point_c = simplex[1].minkowski_point;
        let point_d = simplex[0].minkowski_point;

        let vector_to_origin_from_a = negate(point_a);
        let edge_ab = sub(point_b, point_a);
        let edge_ac = sub(point_c, point_a);
        let edge_ad = sub(point_d, point_a);

        let face_normal_abc = cross(edge_ab, edge_ac);
        let face_normal_acd = cross(edge_ac, edge_ad);
        let face_normal_adb = cross(edge_ad, edge_ab);

        if dot(face_normal_abc, vector_to_origin_from_a) > 0.0 {
            simplex[0] = simplex[1];
            simplex[1] = simplex[2];
            simplex[2] = simplex[3];
            simplex.pop();
            *search_direction = face_normal_abc;
            return false;
        }

        if dot(face_normal_acd, vector_to_origin_from_a) > 0.0 {
            simplex[2] = simplex[3];
            simplex.pop();
            *search_direction = face_normal_acd;
            return false;
        }

        if dot(face_normal_adb, vector_to_origin_from_a) > 0.0 {
            simplex[1] = simplex[2];
            simplex[2] = simplex[3];
            simplex.pop();
            *search_direction = face_normal_adb;
            return false;
        }

        return true;
    }

    let point_single = simplex[0].minkowski_point;
    *search_direction = negate(point_single);
    false
}

pub fn support_pair_world(complex_collider_a: &Vec<ConvexHullCollider>, position_a: DbVector3, yaw_radians_a: f32, complex_collider_b: &Vec<ConvexHullCollider>, position_b: DbVector3, yaw_radians_b: f32, direction_world: DbVector3) -> GjkVertex {
    let direction_len_sq: f32 = dot(direction_world, direction_world);
    let direction_inv_len: f32 = if direction_len_sq > 1e-8 { 1.0 / direction_len_sq.sqrt() } else { 0.0 };

    let support_point_a_world = support_world_complex(complex_collider_a, position_a, yaw_radians_a, direction_world, direction_inv_len);
    let support_point_b_world = support_world_complex(complex_collider_b, position_b, yaw_radians_b, negate(direction_world), direction_inv_len);

    GjkVertex { support_point_a: support_point_a_world, support_point_b: support_point_b_world, minkowski_point: sub(support_point_a_world, support_point_b_world) }
}

pub fn support_world_complex(complex_collider: &Vec<ConvexHullCollider>, world_position: DbVector3, yaw_radians: f32, direction_world: DbVector3, direction_inv_len: f32) -> DbVector3 {
    let direction_local = rotate_around_y_axis(direction_world, -yaw_radians);
    let support_local_point = support_local_complex(complex_collider, direction_local, direction_inv_len);
    let support_world_rotated = rotate_around_y_axis(support_local_point, yaw_radians);
    add(support_world_rotated, world_position)
}

pub fn support_local_complex(complex_collider: &Vec<ConvexHullCollider>, direction_local: DbVector3, direction_inv_len: f32) -> DbVector3 {
    let mut best_dot: f32 = f32::NEG_INFINITY;
    let mut best_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    for index in 0..complex_collider.len() {
        let (hull_support_point, hull_support_dot) = support_local_and_dot(&complex_collider[index], direction_local, direction_inv_len);

        if hull_support_dot > best_dot {
            best_dot = hull_support_dot;
            best_point = hull_support_point;
        }
    }

    best_point
}

pub fn support_local_and_dot(collider: &ConvexHullCollider, direction: DbVector3, direction_inv_len: f32) -> (DbVector3, f32) {
    let vertices: &Vec<DbVector3> = &collider.vertices_local;

    let mut best_vertex_index: usize = 0;
    let mut best_dot_product: f32 = dot(vertices[0], direction);

    for vertex_index in 1..vertices.len() {
        let dot_product: f32 = dot(vertices[vertex_index], direction);
        if dot_product > best_dot_product {
            best_dot_product = dot_product;
            best_vertex_index = vertex_index;
        }
    }

    let mut best_vertex = vertices[best_vertex_index];

    let margin: f32 = collider.margin;
    if margin > 0.0 && direction_inv_len > 0.0 {
        best_vertex = DbVector3 { x: best_vertex.x + direction.x * direction_inv_len * margin, y: best_vertex.y + direction.y * direction_inv_len * margin, z: best_vertex.z + direction.z * direction_inv_len * margin };
        best_dot_product += margin / direction_inv_len;
    }

    (best_vertex, best_dot_product)
}

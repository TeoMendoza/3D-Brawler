use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;

pub fn SolveGjk(
    collider_a: &Vec<ConvexHullCollider>,
    position_a: DbVector3,
    yaw_radians_a: f32,
    collider_b: &Vec<ConvexHullCollider>,
    position_b: DbVector3,
    yaw_radians_b: f32,
    result_out: &mut GjkResult,
    max_iterations: i32,
) -> bool {
    let mut simplex: Vec<GjkVertex> = Vec::with_capacity(4);
    let mut search_direction = DbVector3 { x: 0.0, y: 0.0, z: 1.0 };

    let initial_vertex = SupportPairWorld(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, search_direction);
    let initial_dot: f32 = Dot(initial_vertex.minkowski_point, search_direction);

    if initial_dot <= 0.0 {
        *result_out = GjkResult { intersects: false, simplex, last_direction: Negate(search_direction) };
        return false;
    }

    simplex.push(initial_vertex);
    search_direction = Negate(simplex[0].minkowski_point);

    for _iteration_index in 0..max_iterations {
        let support_vertex = SupportPairWorld(collider_a, position_a, yaw_radians_a, collider_b, position_b, yaw_radians_b, search_direction);
        let support_dot: f32 = Dot(support_vertex.minkowski_point, search_direction);

        if support_dot <= 0.0 {
            *result_out = GjkResult { intersects: false, simplex, last_direction: Negate(search_direction) };
            return false;
        }

        simplex.push(support_vertex);

        if UpdateSimplex(&mut simplex, &mut search_direction) {
            *result_out = GjkResult { intersects: true, simplex, last_direction: Negate(search_direction) };
            return true;
        }
    }

    *result_out = GjkResult { intersects: false, simplex, last_direction: Negate(search_direction) };
    false
}

pub fn UpdateSimplex(simplex: &mut Vec<GjkVertex>, search_direction: &mut DbVector3) -> bool {
    let simplex_count: usize = simplex.len();

    if simplex_count == 2 {
        let point_a = simplex[1].minkowski_point;
        let point_b = simplex[0].minkowski_point;

        let segment_ba = Sub(point_b, point_a);
        let vector_to_origin_from_a = Negate(point_a);

        let dot_segment_with_ao: f32 = Dot(segment_ba, vector_to_origin_from_a);

        if dot_segment_with_ao <= 0.0 {
            simplex.remove(0);
            *search_direction = vector_to_origin_from_a;
            return false;
        }

        let mut new_direction = TripleCross(segment_ba, vector_to_origin_from_a, segment_ba);

        if NearZero(new_direction) {
            new_direction = Perp(segment_ba);
        }

        *search_direction = new_direction;
        return false;
    }

    if simplex_count == 3 {
        let point_a = simplex[2].minkowski_point;
        let point_b = simplex[1].minkowski_point;
        let point_c = simplex[0].minkowski_point;

        let edge_ab = Sub(point_b, point_a);
        let edge_ac = Sub(point_c, point_a);
        let vector_to_origin_from_a = Negate(point_a);
        let triangle_normal_abc = Cross(edge_ab, edge_ac);

        let dot_ab_ao: f32 = Dot(edge_ab, vector_to_origin_from_a);
        let dot_ac_ao: f32 = Dot(edge_ac, vector_to_origin_from_a);

        if dot_ab_ao <= 0.0 && dot_ac_ao <= 0.0 {
            let vertex_a = simplex[2];
            simplex.clear();
            simplex.push(vertex_a);
            *search_direction = vector_to_origin_from_a;
            return false;
        }

        if dot_ab_ao > 0.0 {
            let mut perpendicular_toward_ab = TripleCross(edge_ac, edge_ab, edge_ab);
            let dot_ab_region: f32 = Dot(perpendicular_toward_ab, vector_to_origin_from_a);

            if dot_ab_region > 0.0 {
                simplex.remove(0);
                if NearZero(perpendicular_toward_ab) {
                    perpendicular_toward_ab = Perp(edge_ab);
                }
                *search_direction = perpendicular_toward_ab;
                return false;
            }
        }

        if dot_ac_ao > 0.0 {
            let mut perpendicular_toward_ac = TripleCross(edge_ab, edge_ac, edge_ac);
            let dot_ac_region: f32 = Dot(perpendicular_toward_ac, vector_to_origin_from_a);

            if dot_ac_region > 0.0 {
                simplex.remove(1);
                if NearZero(perpendicular_toward_ac) {
                    perpendicular_toward_ac = Perp(edge_ac);
                }
                *search_direction = perpendicular_toward_ac;
                return false;
            }
        }

        let mut oriented_normal = triangle_normal_abc;
        if Dot(oriented_normal, vector_to_origin_from_a) < 0.0 { oriented_normal = Negate(oriented_normal); }

        *search_direction = oriented_normal;
        return false;
    }

    if simplex_count == 4 {
        let point_a = simplex[3].minkowski_point;
        let point_b = simplex[2].minkowski_point;
        let point_c = simplex[1].minkowski_point;
        let point_d = simplex[0].minkowski_point;

        let vector_to_origin_from_a = Negate(point_a);
        let edge_ab = Sub(point_b, point_a);
        let edge_ac = Sub(point_c, point_a);
        let edge_ad = Sub(point_d, point_a);

        let face_normal_abc = Cross(edge_ab, edge_ac);
        let face_normal_acd = Cross(edge_ac, edge_ad);
        let face_normal_adb = Cross(edge_ad, edge_ab);

        let dot_abc: f32 = Dot(face_normal_abc, vector_to_origin_from_a);
        let dot_acd: f32 = Dot(face_normal_acd, vector_to_origin_from_a);
        let dot_adb: f32 = Dot(face_normal_adb, vector_to_origin_from_a);

        if dot_abc > 0.0 {
            simplex.remove(0);
            *search_direction = face_normal_abc;
            return false;
        }

        if dot_acd > 0.0 {
            simplex.remove(2);
            *search_direction = face_normal_acd;
            return false;
        }

        if dot_adb > 0.0 {
            simplex.remove(1);
            *search_direction = face_normal_adb;
            return false;
        }

        return true;
    }

    let point_single = simplex[0].minkowski_point;
    *search_direction = Negate(point_single);
    false
}

pub fn SupportPairWorld(
    complex_collider_a: &Vec<ConvexHullCollider>,
    position_a: DbVector3,
    yaw_radians_a: f32,
    complex_collider_b: &Vec<ConvexHullCollider>,
    position_b: DbVector3,
    yaw_radians_b: f32,
    direction_world: DbVector3,
) -> GjkVertex {
    let support_point_a_world = SupportWorldComplex(complex_collider_a, position_a, yaw_radians_a, direction_world);
    let support_point_b_world = SupportWorldComplex(complex_collider_b, position_b, yaw_radians_b, Negate(direction_world));
    GjkVertex { support_point_a: support_point_a_world, support_point_b: support_point_b_world, minkowski_point: Sub(support_point_a_world, support_point_b_world) }
}

pub fn SupportWorldComplex(complex_collider: &Vec<ConvexHullCollider>, world_position: DbVector3, yaw_radians: f32, direction_world: DbVector3) -> DbVector3 {
    let direction_local = RotateAroundYAxis(direction_world, -yaw_radians);
    let support_local_point = SupportLocalComplex(complex_collider, direction_local);
    let support_world_rotated = RotateAroundYAxis(support_local_point, yaw_radians);
    Add(support_world_rotated, world_position)
}

pub fn SupportLocalComplex(complex_collider: &Vec<ConvexHullCollider>, direction_local: DbVector3) -> DbVector3 {
    let mut best_dot: f32 = f32::NEG_INFINITY;
    let mut best_point = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    for index in 0..complex_collider.len() {
        let hull_support_point = SupportLocal(&complex_collider[index], direction_local);
        let dot_value: f32 = Dot(hull_support_point, direction_local);

        if dot_value > best_dot {
            best_dot = dot_value;
            best_point = hull_support_point;
        }
    }

    best_point
}

pub fn SupportLocal(collider: &ConvexHullCollider, direction: DbVector3) -> DbVector3 {
    let vertices: &Vec<DbVector3> = &collider.vertices_local;

    let mut best_vertex_index: usize = 0;
    let mut best_dot_product: f32 = Dot(vertices[0], direction);

    for vertex_index in 1..vertices.len() {
        let dot_product: f32 = Dot(vertices[vertex_index], direction);
        if dot_product > best_dot_product {
            best_dot_product = dot_product;
            best_vertex_index = vertex_index;
        }
    }

    let mut best_vertex = vertices[best_vertex_index];

    let margin: f32 = collider.margin;
    if margin > 0.0 {
        let dir_len_sq: f32 = Dot(direction, direction);
        if dir_len_sq > 1e-8 {
            let inv_len: f32 = 1.0 / dir_len_sq.sqrt();
            let dir_norm = DbVector3 { x: direction.x * inv_len, y: direction.y * inv_len, z: direction.z * inv_len };

            best_vertex = DbVector3 { x: best_vertex.x + dir_norm.x * margin, y: best_vertex.y + dir_norm.y * margin, z: best_vertex.z + dir_norm.z * margin };
        }
    }

    best_vertex
}

pub fn RotateAroundYAxis(vector: DbVector3, yaw_radians: f32) -> DbVector3 {
    let cos_yaw: f32 = yaw_radians.cos();
    let sin_yaw: f32 = yaw_radians.sin();

    let rotated_x: f32 = vector.x * cos_yaw + vector.z * sin_yaw;
    let rotated_z: f32 = -vector.x * sin_yaw + vector.z * cos_yaw;

    DbVector3 { x: rotated_x, y: vector.y, z: rotated_z }
}

pub fn GetColliderCenterWorld(collider: &ComplexCollider, position: DbVector3, yaw_radians: f32) -> DbVector3 {
    let rotated_center = RotateAroundYAxis(collider.center_point, yaw_radians);
    Add(position, rotated_center)
}

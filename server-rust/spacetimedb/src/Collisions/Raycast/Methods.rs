use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;
use crate::Collisions::*;
use crate::Map::*;
use crate::Magician::*;

pub fn RaycastMatch(ctx: &ReducerContext, ray_origin: DbVector3, ray_direction: DbVector3, max_distance: f32) -> Raycast {
    let mut has_hit: bool = false;
    let mut best_distance: f32 = max_distance;
    let mut best_point: DbVector3 = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };
    let mut best_type: RaycastHitType = RaycastHitType::None;
    let mut best_identity: Identity = Identity::default();
    let mut best_entity_id: i64 = 0;

    let magician: Magician = ctx.db.magician().identity().find(ctx.sender).expect("Magician not found");

    for other in ctx.db.magician().match_id().filter(magician.match_id) {
        if other.identity == ctx.sender { continue; }

        let hit: Raycast = RaycastComplexCollider(ray_origin, ray_direction, best_distance, other.collider.clone(), other.position, ToRadians(other.rotation.yaw), RaycastHitType::Magician, other.identity, other.id as i64);
        if hit.hit && hit.hit_distance < best_distance {
            has_hit = true;
            best_distance = hit.hit_distance;
            best_point = hit.hit_point;
            best_type = hit.hit_type;
            best_identity = hit.hit_identity;
            best_entity_id = hit.hit_entity_id;
        }
    }

    for map_piece in ctx.db.map().iter() {
        let hit: Raycast = RaycastComplexColliderWorldSpace(ray_origin, ray_direction, best_distance, map_piece.collider.clone(), RaycastHitType::MapPiece, Identity::default(), map_piece.id as i64);
        if hit.hit && hit.hit_distance < best_distance {
            has_hit = true;
            best_distance = hit.hit_distance;
            best_point = hit.hit_point;
            best_type = hit.hit_type;
            best_identity = hit.hit_identity;
            best_entity_id = hit.hit_entity_id;
        }
    }

    Raycast { hit: has_hit, hit_distance: best_distance, hit_point: best_point, hit_type: best_type, hit_identity: best_identity, hit_entity_id: best_entity_id }
}

pub fn RaycastComplexCollider(ray_origin: DbVector3, ray_direction: DbVector3, max_distance: f32, collider: ComplexCollider, collider_world_position: DbVector3, collider_yaw_radians: f32, hit_type: RaycastHitType, hit_identity: Identity, hit_entity_id: i64) -> Raycast {
    let mut has_hit: bool = false;
    let mut best_distance: f32 = max_distance;
    let mut best_point: DbVector3 = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    let local_origin: DbVector3 = RotateAroundYAxis(Sub(ray_origin, collider_world_position), -collider_yaw_radians);
    let local_direction: DbVector3 = Normalize(RotateAroundYAxis(ray_direction, -collider_yaw_radians));

    for hull in collider.convex_hulls.iter() {
        let mut hit_distance_local: f32 = best_distance;
        if RaycastConvexHullTriangles(local_origin, local_direction, best_distance, hull, &mut hit_distance_local) {
            has_hit = true;
            best_distance = hit_distance_local;
            let local_hit_point: DbVector3 = Add(local_origin, Mul(local_direction, hit_distance_local));
            best_point = Add(collider_world_position, RotateAroundYAxis(local_hit_point, collider_yaw_radians));
        }
    }

    Raycast { hit: has_hit, hit_distance: best_distance, hit_point: best_point, hit_type: if has_hit { hit_type } else { RaycastHitType::None }, hit_identity, hit_entity_id }
}

pub fn RaycastComplexColliderWorldSpace(ray_origin: DbVector3, ray_direction: DbVector3, max_distance: f32, collider: ComplexCollider, hit_type: RaycastHitType, hit_identity: Identity, hit_entity_id: i64) -> Raycast {
    let mut has_hit: bool = false;
    let mut best_distance: f32 = max_distance;
    let mut best_point: DbVector3 = DbVector3 { x: 0.0, y: 0.0, z: 0.0 };

    for hull in collider.convex_hulls.iter() {
        let mut hit_distance: f32 = best_distance;
        if RaycastConvexHullTriangles(ray_origin, ray_direction, best_distance, hull, &mut hit_distance) {
            has_hit = true;
            best_distance = hit_distance;
            best_point = Add(ray_origin, Mul(ray_direction, hit_distance));
        }
    }

    Raycast { hit: has_hit, hit_distance: best_distance, hit_point: best_point, hit_type: if has_hit { hit_type } else { RaycastHitType::None }, hit_identity, hit_entity_id }
}

pub fn RaycastConvexHullTriangles(ray_origin_local: DbVector3, ray_direction_local: DbVector3, max_distance: f32, hull: &ConvexHullCollider, hit_distance_out: &mut f32) -> bool {
    *hit_distance_out = max_distance;
    let mut has_hit: bool = false;

    let vertices: &Vec<DbVector3> = &hull.vertices_local;
    let triangles: &Vec<i32> = &hull.triangle_indices_local;

    let mut index: usize = 0;
    while index + 2 < triangles.len() {
        let a: DbVector3 = vertices[triangles[index] as usize];
        let b: DbVector3 = vertices[triangles[index + 1] as usize];
        let c: DbVector3 = vertices[triangles[index + 2] as usize];

        let mut triangle_distance: f32 = 0.0;
        if RayIntersectsTriangle(ray_origin_local, ray_direction_local, a, b, c, &mut triangle_distance) {
            if triangle_distance >= 0.0 && triangle_distance < *hit_distance_out {
                has_hit = true;
                *hit_distance_out = triangle_distance;
            }
        }

        index += 3;
    }

    has_hit
}

pub fn RayIntersectsTriangle(ray_origin: DbVector3, ray_direction: DbVector3, a: DbVector3, b: DbVector3, c: DbVector3, distance_out: &mut f32) -> bool {
    *distance_out = 0.0;

    let edge1: DbVector3 = Sub(b, a);
    let edge2: DbVector3 = Sub(c, a);

    let pvec: DbVector3 = Cross(ray_direction, edge2);
    let det: f32 = Dot(edge1, pvec);

    let epsilon: f32 = 1e-7;
    if det > -epsilon && det < epsilon { return false; }

    let inverse_det: f32 = 1.0 / det;

    let tvec: DbVector3 = Sub(ray_origin, a);
    let u: f32 = Dot(tvec, pvec) * inverse_det;
    if u < 0.0 || u > 1.0 { return false; }

    let qvec: DbVector3 = Cross(tvec, edge1);
    let v: f32 = Dot(ray_direction, qvec) * inverse_det;
    if v < 0.0 || u + v > 1.0 { return false; }

    let t: f32 = Dot(edge2, qvec) * inverse_det;
    if t < 0.0 { return false; }

    *distance_out = t;
    true
}

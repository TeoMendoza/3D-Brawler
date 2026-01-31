use glam::{Quat, Vec3};
use crate::*;

pub fn add(x: DbVector3, y: DbVector3) -> DbVector3 { DbVector3 { x: x.x + y.x, y: x.y + y.y, z: x.z + y.z } }

pub fn sub(x: DbVector3, y: DbVector3) -> DbVector3 { DbVector3 { x: x.x - y.x, y: x.y - y.y, z: x.z - y.z } }

pub fn mul(x: DbVector3, s: f32) -> DbVector3 { DbVector3 { x: x.x * s, y: x.y * s, z: x.z * s } }

pub fn dot(x: DbVector3, y: DbVector3) -> f32 { x.x * y.x + x.y * y.y + x.z * y.z }

pub fn length_sq(x: DbVector3) -> f32 { dot(x, x) }

pub fn magnitude(x: DbVector3) -> f32 { dot(x, x).sqrt() }

pub fn clamp_01(t: f32) -> f32 { if t < 0.0 { 0.0 } else if t > 1.0 { 1.0 } else { t } }

pub fn clamp(x: f32, a: f32, b: f32) -> f32 { if x < a { a } else if x > b { b } else { x } }

pub fn to_radians(degrees: f32) -> f32 { degrees * (std::f32::consts::PI / 180.0) }

pub fn cross(a: DbVector3, b: DbVector3) -> DbVector3 { DbVector3 { x: a.y * b.z - a.z * b.y, y: a.z * b.x - a.x * b.z, z: a.x * b.y - a.y * b.x } }

pub fn to_vec3(v: DbVector3) -> Vec3 { Vec3::new(v.x, v.y, v.z) }

pub fn to_dbvec3(v: Vec3) -> DbVector3 { DbVector3 { x: v.x, y: v.y, z: v.z } }

pub fn rotate(v: DbVector3, q: Quat) -> DbVector3 { to_dbvec3(q * to_vec3(v)) }

pub fn negate(vector: DbVector3) -> DbVector3 { DbVector3 { x: -vector.x, y: -vector.y, z: -vector.z } }

pub fn triple_cross(vector_a: DbVector3, vector_b: DbVector3, vector_c: DbVector3) -> DbVector3 { cross(cross(vector_a, vector_b), vector_c) }

pub fn near_zero(vector: DbVector3) -> bool { length_sq(vector) <= 1e-12 }

pub fn normalize_small_vector(v: DbVector3, fallback: DbVector3) -> DbVector3 {
    let mag_sq: f32 = length_sq(v);
    if mag_sq <= 1e-12 { return fallback; }
    let inv_mag: f32 = 1.0 / mag_sq.sqrt();
    DbVector3 { x: v.x * inv_mag, y: v.y * inv_mag, z: v.z * inv_mag }
}

pub fn any_perpendicular_unit(unit_axis: DbVector3) -> DbVector3 {
    let ref_vec = if unit_axis.y.abs() < 0.99 { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { DbVector3 { x: 1.0, y: 0.0, z: 0.0 } };
    let perp = cross(unit_axis, ref_vec);
    normalize_small_vector(perp, DbVector3 { x: 1.0, y: 0.0, z: 0.0 })
}

pub fn normalize(v: DbVector3) -> DbVector3 {
    let mag_sq: f32 = length_sq(v);
    if mag_sq <= 1e-12 { return DbVector3 { x: 0.0, y: 0.0, z: 0.0 }; }
    let inv_mag: f32 = 1.0 / mag_sq.sqrt();
    DbVector3 { x: v.x * inv_mag, y: v.y * inv_mag, z: v.z * inv_mag }
}

pub fn perpendicular(vector: DbVector3) -> DbVector3 {
    if vector.x.abs() > vector.z.abs() { return DbVector3 { x: -vector.y, y: vector.x, z: 0.0 }; }
    DbVector3 { x: 0.0, y: -vector.z, z: vector.y }
}

pub fn rotate_around_y_axis(vector: DbVector3, yaw_radians: f32) -> DbVector3 {
    let cos_yaw: f32 = yaw_radians.cos();
    let sin_yaw: f32 = yaw_radians.sin();

    let rotated_x: f32 = vector.x * cos_yaw + vector.z * sin_yaw;
    let rotated_z: f32 = -vector.x * sin_yaw + vector.z * cos_yaw;

    DbVector3 { x: rotated_x, y: vector.y, z: rotated_z }
}

pub fn get_collider_center_world(collider: &ComplexCollider, position: DbVector3, yaw_radians: f32) -> DbVector3 {
    let rotated_center = rotate_around_y_axis(collider.center_point, yaw_radians);
    add(position, rotated_center)
}

pub fn compute_contact_normal(raw_normal: DbVector3, center_a: DbVector3, center_b: DbVector3) -> DbVector3 { // Orients collision normal outward towards target (center_a) - Rounds normals to approximate collisions
    let mut normal = raw_normal;
    if dot(normal, normal) < 1e-6 { return DbVector3 { x: 0.0, y: 1.0, z: 0.0 }; }
    normal = normalize(normal);

    let center_delta = sub(center_a, center_b);
    let center_delta_sq: f32 = dot(center_delta, center_delta);

    if center_delta_sq > 1e-8 {
        if dot(normal, center_delta) < 0.0 { normal = negate(normal); }
    }

    let world_up = DbVector3 { x: 0.0, y: 1.0, z: 0.0 };
    let up_dot: f32 = dot(normal, world_up);

    let floor_snap_dot: f32 = 0.98;
    let ceiling_snap_dot: f32 = -0.98;
    let wall_snap_abs_dot: f32 = 0.05;

    if up_dot >= floor_snap_dot { return world_up; } // Floor collision
    if up_dot <= ceiling_snap_dot { return negate(world_up); } // Ceiling collision

    if up_dot.abs() <= wall_snap_abs_dot { // Wall collision
        normal.y = 0.0;
        return normalize(normal);
    }

    normal
}

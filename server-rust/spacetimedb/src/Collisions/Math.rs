use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use glam::{Quat, Vec3};
use crate::*;

pub fn Add(x: DbVector3, y: DbVector3) -> DbVector3 { DbVector3 { x: x.x + y.x, y: x.y + y.y, z: x.z + y.z } }

pub fn Sub(x: DbVector3, y: DbVector3) -> DbVector3 { DbVector3 { x: x.x - y.x, y: x.y - y.y, z: x.z - y.z } }

pub fn Mul(x: DbVector3, s: f32) -> DbVector3 { DbVector3 { x: x.x * s, y: x.y * s, z: x.z * s } }

pub fn Dot(x: DbVector3, y: DbVector3) -> f32 { x.x * y.x + x.y * y.y + x.z * y.z }

pub fn LenSq(x: DbVector3) -> f32 { Dot(x, x) }

pub fn Sqrt(v: f32) -> f32 { v.sqrt() }

pub fn Length(x: DbVector3) -> f32 { Sqrt(Dot(x, x)) }

pub fn Clamp01(t: f32) -> f32 { if t < 0.0 { 0.0 } else if t > 1.0 { 1.0 } else { t } }

pub fn Clamp(x: f32, a: f32, b: f32) -> f32 { if x < a { a } else if x > b { b } else { x } }

pub fn ToRadians(degrees: f32) -> f32 { degrees * (std::f32::consts::PI / 180.0) }

pub fn Cross(a: DbVector3, b: DbVector3) -> DbVector3 { DbVector3 { x: a.y * b.z - a.z * b.y, y: a.z * b.x - a.x * b.z, z: a.x * b.y - a.y * b.x } }

pub fn ToVec3(v: DbVector3) -> Vec3 { Vec3::new(v.x, v.y, v.z) }

pub fn ToDbVector3(v: Vec3) -> DbVector3 { DbVector3 { x: v.x, y: v.y, z: v.z } }

pub fn Rotate(v: DbVector3, q: Quat) -> DbVector3 { ToDbVector3(q * ToVec3(v)) }

pub fn Negate(vector: DbVector3) -> DbVector3 { DbVector3 { x: -vector.x, y: -vector.y, z: -vector.z } }

pub fn TripleCross(vector_a: DbVector3, vector_b: DbVector3, vector_c: DbVector3) -> DbVector3 { Cross(Cross(vector_a, vector_b), vector_c) }

pub fn NormalizeSmallVector(v: DbVector3, fallback: DbVector3) -> DbVector3 {
    let mag_sq: f32 = LenSq(v);
    if mag_sq <= 1e-12 { return fallback; }
    let inv_mag: f32 = 1.0 / Sqrt(mag_sq);
    DbVector3 { x: v.x * inv_mag, y: v.y * inv_mag, z: v.z * inv_mag }
}

pub fn AnyPerpendicularUnit(unit_axis: DbVector3) -> DbVector3 {
    let ref_vec = if unit_axis.y.abs() < 0.99 { DbVector3 { x: 0.0, y: 1.0, z: 0.0 } } else { DbVector3 { x: 1.0, y: 0.0, z: 0.0 } };
    let perp = Cross(unit_axis, ref_vec);
    NormalizeSmallVector(perp, DbVector3 { x: 1.0, y: 0.0, z: 0.0 })
}

pub fn Normalize(v: DbVector3) -> DbVector3 {
    let magnitude: f32 = Length(v);
    if magnitude <= 1e-6 { return DbVector3 { x: 0.0, y: 0.0, z: 0.0 }; }
    DbVector3 { x: v.x / magnitude, y: v.y / magnitude, z: v.z / magnitude }
}

pub fn NearZero(vector: DbVector3) -> bool { Dot(vector, vector) <= 1e-12 }

pub fn Perp(vector: DbVector3) -> DbVector3 {
    if vector.x.abs() > vector.z.abs() { return DbVector3 { x: -vector.y, y: vector.x, z: 0.0 }; }
    DbVector3 { x: 0.0, y: -vector.z, z: vector.y }
}

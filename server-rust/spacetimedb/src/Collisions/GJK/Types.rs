use std::time::Duration;
use spacetimedb::{rand::Rng, Identity, SpacetimeType, ReducerContext, ScheduleAt, Table, Timestamp};
use crate::*;

#[derive(SpacetimeType, Clone, Debug)]
pub struct ComplexCollider { pub convex_hulls: Vec<ConvexHullCollider>, pub center_point: DbVector3 }

#[derive(SpacetimeType, Clone, Debug)]
pub struct ConvexHullCollider { pub vertices_local: Vec<DbVector3>, pub triangle_indices_local: Vec<i32>, pub margin: f32 }

#[derive(SpacetimeType, Copy, Clone, Debug)]
pub struct GjkVertex { pub support_point_a: DbVector3, pub support_point_b: DbVector3, pub minkowski_point: DbVector3 }

#[derive(SpacetimeType, Clone, Debug, Default)]
pub struct GjkResult { pub intersects: bool, pub simplex: Vec<GjkVertex>, pub last_direction: DbVector3 }

#[derive(SpacetimeType, Copy, Clone, Debug, PartialEq, Eq)]
pub struct CollisionEntry { pub entry_type: CollisionEntryType, pub id: u64 }

#[derive(SpacetimeType, Copy, Clone, Debug, PartialEq, Eq)]
pub enum CollisionEntryType { Magician, Map }

#[derive(SpacetimeType, Copy, Clone, Debug, Default)]
pub struct Contact { pub normal: DbVector3, pub depth: f32 }

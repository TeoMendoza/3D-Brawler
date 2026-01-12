use crate::*;

pub static PLATFORM_CONVEX_HULL0_VERTICES: &[DbVector3] = &[
    DbVector3 { x: -2.375, y: 1.9, z: 11.875 }, DbVector3 { x:  2.375, y: 1.9, z: 11.875 }, DbVector3 { x:  2.375, y: 1.9, z:  8.125 }, DbVector3 { x: -2.375, y: 1.9, z:  8.125 },
    DbVector3 { x: -2.375, y: 2.4, z: 11.875 }, DbVector3 { x:  2.375, y: 2.4, z: 11.875 }, DbVector3 { x:  2.375, y: 2.4, z:  8.125 }, DbVector3 { x: -2.375, y: 2.4, z:  8.125 },
];

pub static PLATFORM_CONVEX_HULL0_TRIANGLE_INDICES_LOCAL: &[i32] = &[0, 2, 1, 1, 2, 6, 0, 1, 4, 6, 2, 3, 2, 0, 3, 0, 4, 3, 1, 6, 5, 6, 4, 5, 4, 1, 5, 4, 6, 7, 6, 3, 7, 3, 4, 7];

pub fn platform_collider() -> ComplexCollider {
    let platform_convex_hull_0: ConvexHullCollider = ConvexHullCollider { vertices_local: PLATFORM_CONVEX_HULL0_VERTICES.to_vec(), triangle_indices_local: PLATFORM_CONVEX_HULL0_TRIANGLE_INDICES_LOCAL.to_vec(), margin: 0.0 };
    let platform_convex_hulls: Vec<ConvexHullCollider> = vec![platform_convex_hull_0];
    ComplexCollider { convex_hulls: platform_convex_hulls, center_point: DbVector3 { x: 0.0, y: 2.15, z: 10.0 } }
}

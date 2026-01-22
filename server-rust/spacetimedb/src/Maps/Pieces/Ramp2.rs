use crate::*;

pub static RAMP2_CONVEX_HULL0_VERTICES: &[DbVector3] = &[
    DbVector3 { x:  7.625, y: 0.0, z:  8.125 }, DbVector3 { x:  7.625, y: 0.0, z: 11.875 },
    DbVector3 { x:  2.375, y: 0.0, z:  8.125 }, DbVector3 { x:  2.375, y: 0.0, z: 11.875 },
    DbVector3 { x:  2.375, y: 2.4, z:  8.125 }, DbVector3 { x:  2.375, y: 2.4, z: 11.875 },
];

pub static RAMP2_CONVEX_HULL0_TRIANGLE_INDICES_LOCAL: &[i32] = &[2, 0, 1, 0, 5, 1, 2, 5, 4, 5, 0, 4, 0, 2, 4, 2, 1, 3, 1, 5, 3, 5, 2, 3];

pub fn ramp_2_collider() -> ComplexCollider {
    let ramp2_convex_hull_0: ConvexHullCollider = ConvexHullCollider { vertices_local: RAMP2_CONVEX_HULL0_VERTICES.to_vec(), triangle_indices_local: RAMP2_CONVEX_HULL0_TRIANGLE_INDICES_LOCAL.to_vec(), margin: 0.0 };
    let ramp2_convex_hulls: Vec<ConvexHullCollider> = vec![ramp2_convex_hull_0];
    ComplexCollider { convex_hulls: ramp2_convex_hulls, center_point: DbVector3 { x: 5.0, y: 0.6, z: 10.0 } }
}

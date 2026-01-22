use crate::*;

pub static FLOOR_CONVEX_HULL0_VERTICES: &[DbVector3] = &[
    DbVector3 { x: -100.0, y: 0.0, z: -100.0 },
    DbVector3 { x: -100.0, y: 0.0, z: 100.0 },
    DbVector3 { x: 100.0, y: 0.0, z: -100.0 },
    DbVector3 { x: 100.0, y: 0.0, z: 100.0 },
    DbVector3 { x: -100.0, y: -1.0, z: -100.0 },
    DbVector3 { x: -100.0, y: -1.0, z: 100.0 },
    DbVector3 { x: 100.0, y: -1.0, z: -100.0 },
    DbVector3 { x: 100.0, y: -1.0, z: 100.0 },
];

pub static FLOOR_CONVEX_HULL0_TRIANGLE_INDICES_LOCAL: &[i32] = &[
    0, 5, 1, 0, 1, 2, 0, 2, 4, 5, 0, 4, 2, 1, 3, 1, 5, 3, 5, 4, 6, 4, 2, 6, 2, 3, 6, 3, 5, 7, 5, 6, 7, 6, 3, 7,
];

pub fn floor_collider() -> ComplexCollider {
    let floor_convex_hull_0: ConvexHullCollider = ConvexHullCollider { vertices_local: FLOOR_CONVEX_HULL0_VERTICES.to_vec(), triangle_indices_local: FLOOR_CONVEX_HULL0_TRIANGLE_INDICES_LOCAL.to_vec(), margin: 0.0 };
    let plane_convex_hulls: Vec<ConvexHullCollider> = vec![floor_convex_hull_0];
    ComplexCollider { convex_hulls: plane_convex_hulls, center_point: DbVector3 { x: 0.0, y: -0.5, z: 0.0 } }
}

use crate::{ComplexCollider, ConvexHullCollider, DbVector3};

pub static RampConvexHull0Vertices: &[DbVector3] = &[
    DbVector3 { x: -7.625, y: 0.0, z: 11.875 }, DbVector3 { x: -7.625, y: 0.0, z:  8.125 },
    DbVector3 { x: -2.375, y: 0.0, z: 11.875 }, DbVector3 { x: -2.375, y: 0.0, z:  8.125 },
    DbVector3 { x: -2.375, y: 2.4, z: 11.875 }, DbVector3 { x: -2.375, y: 2.4, z:  8.125 },
];

pub static RampConvexHull0TriangleIndicesLocal: &[i32] = &[0, 5, 1, 0, 1, 2, 0, 2, 4, 2, 5, 4, 5, 0, 4, 2, 1, 3, 1, 5, 3, 5, 2, 3];

pub fn RampCollider() -> ComplexCollider {
    let ramp_convex_hull_0: ConvexHullCollider = ConvexHullCollider { vertices_local: RampConvexHull0Vertices.to_vec(), triangle_indices_local: RampConvexHull0TriangleIndicesLocal.to_vec(), margin: 0.0 };
    let ramp_convex_hulls: Vec<ConvexHullCollider> = vec![ramp_convex_hull_0];
    ComplexCollider { convex_hulls: ramp_convex_hulls, center_point: DbVector3 { x: -5.0, y: 0.6, z: 10.0 } }
}

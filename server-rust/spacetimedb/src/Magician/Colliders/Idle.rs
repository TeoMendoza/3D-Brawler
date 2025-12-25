pub static IdleConvexHull0Vertices: &[DbVector3] = &[
    DbVector3 { x: 0.000000, y: 0.000000, z: 0.000000 },
    DbVector3 { x: 0.209108, y: 0.240000, z: 0.000000 },
    DbVector3 { x: -0.209108, y: 0.240000, z: 0.000000 },
    DbVector3 { x: 0.000000, y: 0.240000, z: 0.209108 },
    DbVector3 { x: 0.000000, y: 0.240000, z: -0.209108 },
    DbVector3 { x: 0.209108, y: 1.718794, z: 0.000000 },
    DbVector3 { x: -0.209108, y: 1.718794, z: 0.000000 },
    DbVector3 { x: 0.000000, y: 1.718794, z: 0.209108 },
    DbVector3 { x: 0.000000, y: 1.718794, z: -0.209108 },
    DbVector3 { x: 0.000000, y: 1.958794, z: 0.000000 },
];

pub static IdleConvexHull0TriangleIndicesLocal: &[i32] = &[
    2, 3, 6, 4, 2, 6, 1, 3, 0, 3, 2, 0, 2, 4, 0, 4, 1, 0, 3, 1, 5, 1, 4, 5, 9, 6, 7, 6, 3, 7, 3, 5, 7, 5, 9, 7, 4, 6, 8, 6, 9, 8, 9, 5, 8, 5, 4, 8,
];

pub fn MagicianIdleCollider() -> ComplexCollider {
    let idle_convex_hull_0: ConvexHullCollider = ConvexHullCollider { vertices_local: IdleConvexHull0Vertices.to_vec(), triangle_indices_local: IdleConvexHull0TriangleIndicesLocal.to_vec(), margin: 0.0 };
    let magician_idle_convex_hulls: Vec<ConvexHullCollider> = vec![idle_convex_hull_0];
    ComplexCollider { convex_hulls: magician_idle_convex_hulls, center_point: DbVector3 { x: 0.0, y: 0.979397, z: 0.0 } }
}

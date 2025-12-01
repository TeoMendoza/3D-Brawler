// using System.Collections.Generic;
// using System.Numerics;
// using SpacetimeDB;

// public static partial class Module
// {

//     public static readonly List<DbVector3> JumpConvexHull0Vertices = new List<DbVector3>
// {
//     new DbVector3( 0.35f, 0.2f,   0f),
//     new DbVector3(-0.35f, 0.2f,   0f),
//     new DbVector3( 0f,    0.2f,   0.35f),
//     new DbVector3( 0f,    0.2f,  -0.35f),

//     new DbVector3( 0.35f, 1.9f,   0f),
//     new DbVector3(-0.35f, 1.9f,   0f),
//     new DbVector3( 0f,    1.9f,   0.35f),
//     new DbVector3( 0f,    1.9f,  -0.35f),
// };

//     public static readonly ConvexHullCollider JumpConvexHull0 = new ConvexHullCollider
//     {
//         VerticesLocal = JumpConvexHull0Vertices
//     };

//     public static readonly List<ConvexHullCollider> MagicianJumpingConvexHulls = new List<ConvexHullCollider>
//     {
//         JumpConvexHull0,
//     };

//     public static readonly ComplexCollider MagicianJumpingCollider = new ComplexCollider
//     {
//         ConvexHulls = MagicianJumpingConvexHulls
//     };
// }

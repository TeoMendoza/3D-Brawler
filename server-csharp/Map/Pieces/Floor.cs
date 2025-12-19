using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> FloorConvexHull0Vertices = new List<DbVector3>
    {
        // Top edge corners
        new DbVector3(-100f, 0.0f, -100f),
        new DbVector3(-100f, 0.0f,  100f),
        new DbVector3( 100f, 0.0f, -100f),
        new DbVector3( 100f, 0.0f,  100f),

        // Bottom edge corners (gives the floor thickness so GJK has volume)
        new DbVector3(-100f, -1.0f, -100f),
        new DbVector3(-100f, -1.0f,  100f),
        new DbVector3( 100f, -1.0f, -100f),
        new DbVector3( 100f, -1.0f,  100f),
    };

    public static readonly ConvexHullCollider FloorConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = FloorConvexHull0Vertices,
        TriangleIndicesLocal = new List<int> { 0, 5, 1, 0, 1, 2, 0, 2, 4, 5, 0, 4, 2, 1, 3, 1, 5, 3, 5, 4, 6, 4, 2, 6, 2, 3, 6, 3, 5, 7, 5, 6, 7, 6, 3, 7 },
        Margin = 0f
    };

    public static readonly List<ConvexHullCollider> PlaneConvexHulls = new List<ConvexHullCollider>
    {
        FloorConvexHull0,
    };

    public static readonly ComplexCollider FloorCollider = new ComplexCollider
    {
        ConvexHulls = PlaneConvexHulls,
        CenterPoint = new DbVector3(0, -0.5f, 0)
    };
}

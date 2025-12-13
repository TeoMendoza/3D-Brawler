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

using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> FloorConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(-100f, -0.05f, -100f),
        new DbVector3(-100f, -0.05f,  100f),
        new DbVector3(-100f, 0f, -100f),
        new DbVector3(-100f, 0f,  100f),
        new DbVector3( 100f, -0.05f, -100f),
        new DbVector3( 100f, -0.05f,  100f),
        new DbVector3( 100f, 0f, -100f),
        new DbVector3( 100f, 0f,  100f),
    };

    public static readonly ConvexHullCollider FloorConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = FloorConvexHull0Vertices
    };

    public static readonly List<ConvexHullCollider> PlaneConvexHulls = new List<ConvexHullCollider>
    {
        FloorConvexHull0,
    };

    public static readonly ComplexCollider FloorCollider = new ComplexCollider
    {
        ConvexHulls = PlaneConvexHulls
    };
}

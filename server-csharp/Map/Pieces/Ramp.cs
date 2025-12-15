using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> RampConvexHull0Vertices = new List<DbVector3>
    {
        // Back edge (low), bottom at y = 0, world-positioned at z = 10 with scale applied
        new DbVector3(-1.875f, 0.0f,  7.375f),
        new DbVector3( 1.875f, 0.0f,  7.375f),

        // Front bottom edge (low)
        new DbVector3(-1.875f, 0.0f, 12.625f),
        new DbVector3( 1.875f, 0.0f, 12.625f),

        // Front top edge (high)
        new DbVector3(-1.875f, 2.4f, 12.625f),
        new DbVector3( 1.875f, 2.4f, 12.625f),
    };

    public static readonly ConvexHullCollider RampConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = RampConvexHull0Vertices,
        Margin = 0f
    };

    public static readonly List<ConvexHullCollider> RampConvexHulls = new List<ConvexHullCollider>
    {
        RampConvexHull0,
    };

    public static readonly ComplexCollider RampCollider = new ComplexCollider
    {
        ConvexHulls = RampConvexHulls,
        CenterPoint = new DbVector3(0f, 0.6f, 10f)
    };
}

using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> RampConvexHull0Vertices = new List<DbVector3>
    {
        // Low back edge (was Z=-0.35, Y=-0.2)
        new DbVector3(-7.625f, 0.0f, 11.875f), // Left
        new DbVector3(-7.625f, 0.0f,  8.125f), // Right

        // Low front edge (was Z=+0.35, Y=-0.2)
        new DbVector3(-2.375f, 0.0f, 11.875f), // Left
        new DbVector3(-2.375f, 0.0f,  8.125f), // Right

        // High front ridge (was Z=+0.35, Y=+0.2)
        new DbVector3(-2.375f, 2.4f, 11.875f), // Left
        new DbVector3(-2.375f, 2.4f,  8.125f), // Right
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
        CenterPoint = new DbVector3(-5.0f, 0.6f, 10.0f)
    };
}

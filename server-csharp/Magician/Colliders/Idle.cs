using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{


   public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
    {
        // Bottom pole
        new DbVector3(0.000000f, 0.000000f, 0.000000f),

        // Bottom band (single band, no stacked rings)
        new DbVector3(0.209108f, 0.240000f, 0.000000f),
        new DbVector3(-0.209108f, 0.240000f, 0.000000f),
        new DbVector3(0.000000f, 0.240000f, 0.209108f),
        new DbVector3(0.000000f, 0.240000f, -0.209108f),

        // Top band (single band, no stacked rings)
        new DbVector3(0.209108f, 1.718794f, 0.000000f),
        new DbVector3(-0.209108f, 1.718794f, 0.000000f),
        new DbVector3(0.000000f, 1.718794f, 0.209108f),
        new DbVector3(0.000000f, 1.718794f, -0.209108f),

        // Top pole
        new DbVector3(0.000000f, 1.958794f, 0.000000f),
    };

    public static readonly ConvexHullCollider IdleConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull0Vertices,
        Margin = 0f
    };

    public static readonly List<ConvexHullCollider> MagicianIdleConvexHulls = new List<ConvexHullCollider>
    {
        IdleConvexHull0
    };

    public static readonly ComplexCollider MagicianIdleCollider = new ComplexCollider
    {
        ConvexHulls = MagicianIdleConvexHulls,
        CenterPoint = new DbVector3(0f, 0.979397f, 0f)
    };
}

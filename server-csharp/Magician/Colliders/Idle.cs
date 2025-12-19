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
        TriangleIndicesLocal = new List<int> { 2, 3, 6, 4, 2, 6, 1, 3, 0, 3, 2, 0, 2, 4, 0, 4, 1, 0, 3, 1, 5, 1, 4, 5, 9, 6, 7, 6, 3, 7, 3, 5, 7, 5, 9, 7, 4, 6, 8, 6, 9, 8, 9, 5, 8, 5, 4, 8 },
        Margin = 0f
    };

    public static readonly List<ConvexHullCollider> MagicianIdleConvexHulls = new List<ConvexHullCollider>
    {
        IdleConvexHull0,
    };

    public static readonly ComplexCollider MagicianIdleCollider = new ComplexCollider
    {
        ConvexHulls = MagicianIdleConvexHulls,
        CenterPoint = new DbVector3(0f, 0.979397f, 0f)
    };
}

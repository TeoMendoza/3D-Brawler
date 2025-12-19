using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> Ramp2ConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3( 7.625f, 0.0f,  8.125f), // Left
        new DbVector3( 7.625f, 0.0f, 11.875f), // Right

        // Low front edge (was Z=+0.35, Y=-0.2)
        new DbVector3( 2.375f, 0.0f,  8.125f), // Left
        new DbVector3( 2.375f, 0.0f, 11.875f), // Right

        // High front ridge (was Z=+0.35, Y=+0.2)
        new DbVector3( 2.375f, 2.4f,  8.125f), // Left
        new DbVector3( 2.375f, 2.4f, 11.875f), // Right
    };

    public static readonly ConvexHullCollider Ramp2ConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = Ramp2ConvexHull0Vertices,
        TriangleIndicesLocal = new List<int> { 2, 0, 1, 0, 5, 1, 2, 5, 4, 5, 0, 4, 0, 2, 4, 2, 1, 3, 1, 5, 3, 5, 2, 3 },
        Margin = 0f
    };

    public static readonly List<ConvexHullCollider> Ramp2ConvexHulls = new List<ConvexHullCollider>
    {
        Ramp2ConvexHull0,
    };

    public static readonly ComplexCollider Ramp2Collider = new ComplexCollider
    {
        ConvexHulls = Ramp2ConvexHulls,
        CenterPoint = new DbVector3(5.0f, 0.6f, 10.0f)
    };
}

using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> FloorConvexHull0Vertices = new List<DbVector3>
    {
        // --- CENTER ---
        new DbVector3(0, 0, 0), 
        new DbVector3(0, -1, 0),

        // --- INNER RING (25m) ---
        new DbVector3( 25, 0,  25), new DbVector3( 25, 0, -25),
        new DbVector3(-25, 0,  25), new DbVector3(-25, 0, -25),
        
        // --- MID RING (50m) ---
        new DbVector3( 50, 0,  50), new DbVector3( 50, 0, -50),
        new DbVector3(-50, 0,  50), new DbVector3(-50, 0, -50),

        // --- OUTER RING (75m) ---
        new DbVector3( 75, 0,  75), new DbVector3( 75, 0, -75),
        new DbVector3(-75, 0,  75), new DbVector3(-75, 0, -75),

        // --- EDGE (100m) ---
        new DbVector3(-100f, 0.0f, -100f),
        new DbVector3(-100f, 0.0f,  100f),
        new DbVector3( 100f, 0.0f, -100f),
        new DbVector3( 100f, 0.0f,  100f),
        
        // Bottom corners (optional, but good for completeness)
        new DbVector3(-100f, -1f, -100f),
        new DbVector3(-100f, -1f,  100f),
        new DbVector3( 100f, -1f, -100f),
        new DbVector3( 100f, -1f,  100f),
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
        CenterPoint = new DbVector3(0f, -0.227273f, 0f)
    };
}

using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{


   public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
    {
        // --- BOTTOM POLE (The Anchor) ---
        // At height 0.05. This is the single point that will stand on the floor.
        // Minimizes jitter completely.
        new DbVector3( 0.000000f,  0.050000f,  0.000000f),

        // --- BOTTOM "TIP" RING (Rounding the feet) ---
        // Height: ~0.12  Radius: ~0.17
        new DbVector3( 0.176777f,  0.123223f,  0.000000f),
        new DbVector3( 0.125000f,  0.123223f,  0.125000f),
        new DbVector3( 0.000000f,  0.123223f,  0.176777f),
        new DbVector3(-0.125000f,  0.123223f,  0.125000f),
        new DbVector3(-0.176777f,  0.123223f,  0.000000f),
        new DbVector3(-0.125000f,  0.123223f, -0.125000f),
        new DbVector3( 0.000000f,  0.123223f, -0.176777f),
        new DbVector3( 0.125000f,  0.123223f, -0.125000f),

        // --- BOTTOM EQUATOR (Full Width) ---
        // Height: 0.30 (Center of bottom sphere) Radius: 0.25
        new DbVector3( 0.250000f,  0.300000f,  0.000000f),
        new DbVector3( 0.176777f,  0.300000f,  0.176777f),
        new DbVector3( 0.000000f,  0.300000f,  0.250000f),
        new DbVector3(-0.176777f,  0.300000f,  0.176777f),
        new DbVector3(-0.250000f,  0.300000f,  0.000000f),
        new DbVector3(-0.176777f,  0.300000f, -0.176777f),
        new DbVector3( 0.000000f,  0.300000f, -0.250000f),
        new DbVector3( 0.176777f,  0.300000f, -0.176777f),

        // --- TOP EQUATOR ---
        // Height: 1.40 (Center of top sphere)
        new DbVector3( 0.250000f,  1.400000f,  0.000000f),
        new DbVector3( 0.176777f,  1.400000f,  0.176777f),
        new DbVector3( 0.000000f,  1.400000f,  0.250000f),
        new DbVector3(-0.176777f,  1.400000f,  0.176777f),
        new DbVector3(-0.250000f,  1.400000f,  0.000000f),
        new DbVector3(-0.176777f,  1.400000f, -0.176777f),
        new DbVector3( 0.000000f,  1.400000f, -0.250000f),
        new DbVector3( 0.176777f,  1.400000f, -0.176777f),

        // --- TOP "TIP" RING ---
        new DbVector3( 0.176777f,  1.576777f,  0.000000f),
        new DbVector3( 0.125000f,  1.576777f,  0.125000f),
        new DbVector3( 0.000000f,  1.576777f,  0.176777f),
        new DbVector3(-0.125000f,  1.576777f,  0.125000f),
        new DbVector3(-0.176777f,  1.576777f,  0.000000f),
        new DbVector3(-0.125000f,  1.576777f, -0.125000f),
        new DbVector3( 0.000000f,  1.576777f, -0.176777f),
        new DbVector3( 0.125000f,  1.576777f, -0.125000f),

        // --- TOP POLE ---
        new DbVector3( 0.000000f,  1.650000f,  0.000000f),
    };



    public static readonly ConvexHullCollider IdleConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull0Vertices,
        Margin = 0f
    };

    public static readonly List<ConvexHullCollider> MagicianIdleConvexHulls = new List<ConvexHullCollider>
    {
        IdleConvexHull0,
    };

    public static readonly ComplexCollider MagicianIdleCollider = new ComplexCollider
    {
        ConvexHulls = MagicianIdleConvexHulls,
        CenterPoint = new DbVector3(0f, 0.85f, 0f)
    };
}

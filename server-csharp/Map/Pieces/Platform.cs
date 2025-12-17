using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> PlatformConvexHull0Vertices = new List<DbVector3>
    {
        // Bottom face (Y = 2.15 - 0.25 = 1.9)
        new DbVector3(-2.375f, 1.9f, 11.875f), // Bottom-Far-Left
        new DbVector3( 2.375f, 1.9f, 11.875f), // Bottom-Far-Right
        new DbVector3( 2.375f, 1.9f,  8.125f), // Bottom-Near-Right
        new DbVector3(-2.375f, 1.9f,  8.125f), // Bottom-Near-Left

        // Top face (Y = 2.15 + 0.25 = 2.4)
        new DbVector3(-2.375f, 2.4f, 11.875f), // Top-Far-Left
        new DbVector3( 2.375f, 2.4f, 11.875f), // Top-Far-Right
        new DbVector3( 2.375f, 2.4f,  8.125f), // Top-Near-Right
        new DbVector3(-2.375f, 2.4f,  8.125f), // Top-Near-Left
    };

    public static readonly ConvexHullCollider PlatformConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = PlatformConvexHull0Vertices
    };

    public static readonly List<ConvexHullCollider> PlatformConvexHulls = new List<ConvexHullCollider>
    {
        PlatformConvexHull0,
    };

    public static readonly ComplexCollider PlatformCollider = new ComplexCollider
    {
        ConvexHulls = PlatformConvexHulls,
        CenterPoint = new DbVector3(0.0f, 2.15f, 10.0f)
    };

}


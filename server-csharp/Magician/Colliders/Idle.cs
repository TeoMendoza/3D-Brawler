using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{


    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
{
    // Bottom Ring (Y = 0.05, Radius = 0.25)
    new DbVector3( 0.250000f,  0.050000f,  0.000000f),
    new DbVector3( 0.230970f,  0.050000f,  0.095671f),
    new DbVector3( 0.176777f,  0.050000f,  0.176777f),
    new DbVector3( 0.095671f,  0.050000f,  0.230970f),
    new DbVector3( 0.000000f,  0.050000f,  0.250000f),
    new DbVector3(-0.095671f,  0.050000f,  0.230970f),
    new DbVector3(-0.176777f,  0.050000f,  0.176777f),
    new DbVector3(-0.230970f,  0.050000f,  0.095671f),
    new DbVector3(-0.250000f,  0.050000f,  0.000000f),
    new DbVector3(-0.230970f,  0.050000f, -0.095671f),
    new DbVector3(-0.176777f,  0.050000f, -0.176777f),
    new DbVector3(-0.095671f,  0.050000f, -0.230970f),
    new DbVector3(-0.000000f,  0.050000f, -0.250000f),
    new DbVector3( 0.095671f,  0.050000f, -0.230970f),
    new DbVector3( 0.176777f,  0.050000f, -0.176777f),
    new DbVector3( 0.230970f,  0.050000f, -0.095671f),

    // Top Ring (Y = 1.65, Radius = 0.25)
    new DbVector3( 0.250000f,  1.650000f,  0.000000f),
    new DbVector3( 0.230970f,  1.650000f,  0.095671f),
    new DbVector3( 0.176777f,  1.650000f,  0.176777f),
    new DbVector3( 0.095671f,  1.650000f,  0.230970f),
    new DbVector3( 0.000000f,  1.650000f,  0.250000f),
    new DbVector3(-0.095671f,  1.650000f,  0.230970f),
    new DbVector3(-0.176777f,  1.650000f,  0.176777f),
    new DbVector3(-0.230970f,  1.650000f,  0.095671f),
    new DbVector3(-0.250000f,  1.650000f,  0.000000f),
    new DbVector3(-0.230970f,  1.650000f, -0.095671f),
    new DbVector3(-0.176777f,  1.650000f, -0.176777f),
    new DbVector3(-0.095671f,  1.650000f, -0.230970f),
    new DbVector3(-0.000000f,  1.650000f, -0.250000f),
    new DbVector3( 0.095671f,  1.650000f, -0.230970f),
    new DbVector3( 0.176777f,  1.650000f, -0.176777f),
    new DbVector3( 0.230970f,  1.650000f, -0.095671f),
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
        ConvexHulls = MagicianIdleConvexHulls
    };
}

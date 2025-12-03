using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
{
    new DbVector3( 0.350000f,  0.0f,  0.000000f),
    new DbVector3( 0.323358f,  0.0f,  0.133939f),
    new DbVector3( 0.247487f,  0.0f,  0.247487f),
    new DbVector3( 0.133939f,  0.0f,  0.323358f),
    new DbVector3( 0.000000f,  0.0f,  0.350000f),
    new DbVector3(-0.133939f,  0.0f,  0.323358f),
    new DbVector3(-0.247487f,  0.0f,  0.247487f),
    new DbVector3(-0.323358f,  0.0f,  0.133939f),
    new DbVector3(-0.350000f,  0.0f,  0.000000f),
    new DbVector3(-0.323358f,  0.0f, -0.133939f),
    new DbVector3(-0.247487f,  0.0f, -0.247487f),
    new DbVector3(-0.133939f,  0.0f, -0.323358f),
    new DbVector3(-0.000000f,  0.0f, -0.350000f),
    new DbVector3( 0.133939f,  0.0f, -0.323358f),
    new DbVector3( 0.247487f,  0.0f, -0.247487f),
    new DbVector3( 0.323358f,  0.0f, -0.133939f),

    new DbVector3( 0.350000f,  1.7f,  0.000000f),
    new DbVector3( 0.323358f,  1.7f,  0.133939f),
    new DbVector3( 0.247487f,  1.7f,  0.247487f),
    new DbVector3( 0.133939f,  1.7f,  0.323358f),
    new DbVector3( 0.000000f,  1.7f,  0.350000f),
    new DbVector3(-0.133939f,  1.7f,  0.323358f),
    new DbVector3(-0.247487f,  1.7f,  0.247487f),
    new DbVector3(-0.323358f,  1.7f,  0.133939f),
    new DbVector3(-0.350000f,  1.7f,  0.000000f),
    new DbVector3(-0.323358f,  1.7f, -0.133939f),
    new DbVector3(-0.247487f,  1.7f, -0.247487f),
    new DbVector3(-0.133939f,  1.7f, -0.323358f),
    new DbVector3(-0.000000f,  1.7f, -0.350000f),
    new DbVector3( 0.133939f,  1.7f, -0.323358f),
    new DbVector3( 0.247487f,  1.7f, -0.247487f),
    new DbVector3( 0.323358f,  1.7f, -0.133939f),
};


    public static readonly ConvexHullCollider IdleConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull0Vertices,
        Margin = 0.005f
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

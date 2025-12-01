using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3( 0.35f,    0f,      0f),
        new DbVector3( 0.2475f,  0f,  0.2475f),
        new DbVector3( 0f,       0f,   0.35f),
        new DbVector3(-0.2475f,  0f,  0.2475f),
        new DbVector3(-0.35f,    0f,      0f),
        new DbVector3(-0.2475f,  0f, -0.2475f),
        new DbVector3( 0f,       0f,  -0.35f),
        new DbVector3( 0.2475f,  0f, -0.2475f),

        new DbVector3( 0.35f,    1.7f,      0f),
        new DbVector3( 0.2475f,  1.7f,  0.2475f),
        new DbVector3( 0f,       1.7f,   0.35f),
        new DbVector3(-0.2475f,  1.7f,  0.2475f),
        new DbVector3(-0.35f,    1.7f,      0f),
        new DbVector3(-0.2475f,  1.7f, -0.2475f),
        new DbVector3( 0f,       1.7f,  -0.35f),
        new DbVector3( 0.2475f,  1.7f, -0.2475f),
    };

    public static readonly ConvexHullCollider IdleConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull0Vertices,
        Margin = 0.025f
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

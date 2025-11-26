using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> CrouchConvexHull0Vertices = new List<DbVector3>
{
    new DbVector3( 0.35f, 0f,     0f),
    new DbVector3(-0.35f, 0f,     0f),
    new DbVector3( 0f,    0f,     0.35f),
    new DbVector3( 0f,    0f,    -0.35f),

    new DbVector3( 0.35f, 1.1f,   0f),
    new DbVector3(-0.35f, 1.1f,   0f),
    new DbVector3( 0f,    1.1f,   0.35f),
    new DbVector3( 0f,    1.1f,  -0.35f),
};

    public static readonly ConvexHullCollider CrouchConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull0Vertices
    };

    public static readonly List<ConvexHullCollider> MagicianCrouchConvexHulls = new List<ConvexHullCollider>
    {
        CrouchConvexHull0,
    };

    public static readonly ComplexCollider MagicianCrouchingCollider = new ComplexCollider
    {
        ConvexHulls = MagicianCrouchConvexHulls
    };
}

using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(0.3f, 1.548461f, 0f),
        new DbVector3(0.3f, 0.188317f, 0f),
        new DbVector3(-0.3f, 1.548461f, 0f),
        new DbVector3(-0.3f, 0.188317f, 0f),
        new DbVector3(0f, 1.848461f, 0f),
        new DbVector3(0f, 0.488317f, 0f),
        new DbVector3(0f, 1.248461f, 0f),
        new DbVector3(0f, -0.111683f, 0f),
        new DbVector3(0f, 1.548461f, 0.3f),
        new DbVector3(0f, 0.188317f, 0.3f),
        new DbVector3(0f, 1.548461f, -0.3f),
        new DbVector3(0f, 0.188317f, -0.3f),
    };

    public static readonly ConvexHullCollider IdleConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull0Vertices
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

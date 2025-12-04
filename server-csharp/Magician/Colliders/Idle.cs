using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{


    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3( 0.300000f,  0.000000f,  0.000000f),
        new DbVector3( 0.277164f,  0.000000f,  0.114805f),
        new DbVector3( 0.212132f,  0.000000f,  0.212132f),
        new DbVector3( 0.114805f,  0.000000f,  0.277164f),
        new DbVector3( 0.000000f,  0.000000f,  0.300000f),
        new DbVector3(-0.114805f,  0.000000f,  0.277164f),
        new DbVector3(-0.212132f,  0.000000f,  0.212132f),
        new DbVector3(-0.277164f,  0.000000f,  0.114805f),
        new DbVector3(-0.300000f,  0.000000f,  0.000000f),
        new DbVector3(-0.277164f,  0.000000f, -0.114805f),
        new DbVector3(-0.212132f,  0.000000f, -0.212132f),
        new DbVector3(-0.114805f,  0.000000f, -0.277164f),
        new DbVector3(-0.000000f,  0.000000f, -0.300000f),
        new DbVector3( 0.114805f,  0.000000f, -0.277164f),
        new DbVector3( 0.212132f,  0.000000f, -0.212132f),
        new DbVector3( 0.277164f,  0.000000f, -0.114805f),

        new DbVector3( 0.350000f,  0.850000f,  0.000000f),
        new DbVector3( 0.323358f,  0.850000f,  0.133939f),
        new DbVector3( 0.247487f,  0.850000f,  0.247487f),
        new DbVector3( 0.133939f,  0.850000f,  0.323358f),
        new DbVector3( 0.000000f,  0.850000f,  0.350000f),
        new DbVector3(-0.133939f,  0.850000f,  0.323358f),
        new DbVector3(-0.247487f,  0.850000f,  0.247487f),
        new DbVector3(-0.323358f,  0.850000f,  0.133939f),
        new DbVector3(-0.350000f,  0.850000f,  0.000000f),
        new DbVector3(-0.323358f,  0.850000f, -0.133939f),
        new DbVector3(-0.247487f,  0.850000f, -0.247487f),
        new DbVector3(-0.133939f,  0.850000f, -0.323358f),
        new DbVector3(-0.000000f,  0.850000f, -0.350000f),
        new DbVector3( 0.133939f,  0.850000f, -0.323358f),
        new DbVector3( 0.247487f,  0.850000f, -0.247487f),
        new DbVector3( 0.323358f,  0.850000f, -0.133939f),

        new DbVector3( 0.300000f,  1.700000f,  0.000000f),
        new DbVector3( 0.277164f,  1.700000f,  0.114805f),
        new DbVector3( 0.212132f,  1.700000f,  0.212132f),
        new DbVector3( 0.114805f,  1.700000f,  0.277164f),
        new DbVector3( 0.000000f,  1.700000f,  0.300000f),
        new DbVector3(-0.114805f,  1.700000f,  0.277164f),
        new DbVector3(-0.212132f,  1.700000f,  0.212132f),
        new DbVector3(-0.277164f,  1.700000f,  0.114805f),
        new DbVector3(-0.300000f,  1.700000f,  0.000000f),
        new DbVector3(-0.277164f,  1.700000f, -0.114805f),
        new DbVector3(-0.212132f,  1.700000f, -0.212132f),
        new DbVector3(-0.114805f,  1.700000f, -0.277164f),
        new DbVector3(-0.000000f,  1.700000f, -0.300000f),
        new DbVector3( 0.114805f,  1.700000f, -0.277164f),
        new DbVector3( 0.212132f,  1.700000f, -0.212132f),
        new DbVector3( 0.277164f,  1.700000f, -0.114805f),
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

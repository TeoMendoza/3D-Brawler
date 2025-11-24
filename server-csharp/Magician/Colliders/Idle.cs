using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(-0.095079f, 0.469046f, 0.119213f),
        new DbVector3(-0.08794f, 0.484163f, 0.024925f),
        new DbVector3(-0.096901f, 0.414177f, 0.123102f),
        new DbVector3(-0.038791f, 0.486453f, 0.134452f),
        new DbVector3(0.233661f, 0.058315f, 0.212295f),
        new DbVector3(0.116896f, 0.058315f, 0.212295f),
        new DbVector3(0.233661f, -0.019528f, 0.212295f),
        new DbVector3(0.194739f, 0.486453f, 0.09553f),
        new DbVector3(0.233661f, -0.019528f, 0.017687f),
        new DbVector3(0.155817f, 0.486453f, -0.099078f),
        new DbVector3(0.039053f, 0.486453f, -0.099078f),
        new DbVector3(0.077974f, 0.019394f, -0.137999f),
        new DbVector3(0.000131f, 0.486453f, -0.060156f),
        new DbVector3(0.077974f, -0.019528f, 0.134452f),
        new DbVector3(0.039053f, -0.019528f, -0.099078f),
        new DbVector3(0.155817f, -0.019528f, -0.099078f),
    };

    public static readonly ConvexHullCollider IdleConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull0Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull1Vertices = new List<DbVector3>
    {
        new DbVector3(-0.311242f, 0.719983f, -0.060156f),
        new DbVector3(-0.038666f, 0.917621f, -0.132307f),
        new DbVector3(-0.194477f, 0.486453f, -0.021234f),
        new DbVector3(-0.311242f, 0.914591f, -0.060156f),
        new DbVector3(-0.038791f, 0.486453f, 0.017687f),
        new DbVector3(-0.038791f, 0.875669f, 0.173374f),
        new DbVector3(-0.038791f, 0.486453f, 0.173374f),
        new DbVector3(-0.035979f, 0.916849f, 0.131981f),
        new DbVector3(-0.194477f, 0.914591f, 0.134452f),
        new DbVector3(-0.311242f, 0.914591f, 0.056609f),
        new DbVector3(-0.155556f, 0.875669f, 0.173374f),
        new DbVector3(-0.194477f, 0.603218f, 0.173374f),
        new DbVector3(-0.233399f, 0.486453f, 0.134452f),
        new DbVector3(-0.099717f, 0.478827f, 0.007778f),
        new DbVector3(-0.233399f, 0.486453f, 0.017687f),
        new DbVector3(-0.311242f, 0.719983f, 0.056609f),
    };

    public static readonly ConvexHullCollider IdleConvexHull1 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull1Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull2Vertices = new List<DbVector3>
    {
        new DbVector3(-0.038791f, 1.070277f, -0.176921f),
        new DbVector3(-0.038791f, 0.914591f, -0.176921f),
        new DbVector3(-0.041421f, 1.382587f, -0.130825f),
        new DbVector3(0.194739f, 1.38165f, -0.137999f),
        new DbVector3(0.272582f, 1.38165f, 0.056609f),
        new DbVector3(0.155817f, 1.303807f, 0.173374f),
        new DbVector3(0.311504f, 1.109199f, 0.056609f),
        new DbVector3(0.077974f, 0.914591f, 0.173374f),
        new DbVector3(0.171482f, 0.908021f, 0.104755f),
        new DbVector3(-0.038791f, 0.914591f, 0.173374f),
        new DbVector3(0.281025f, 0.907964f, 0.000145f),
        new DbVector3(0.311504f, 1.225964f, -0.060156f),
        new DbVector3(0.116896f, 0.914591f, -0.176921f),
        new DbVector3(0.272582f, 1.38165f, -0.099078f),
        new DbVector3(0.077974f, 1.38165f, 0.173374f),
        new DbVector3(-0.038791f, 1.38165f, 0.173374f),
    };

    public static readonly ConvexHullCollider IdleConvexHull2 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull2Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull3Vertices = new List<DbVector3>
    {
        new DbVector3(-0.27232f, 1.38165f, 0.056609f),
        new DbVector3(-0.137978f, 1.394051f, -0.11793f),
        new DbVector3(-0.27232f, 1.38165f, -0.099078f),
        new DbVector3(-0.038791f, 1.38165f, 0.173374f),
        new DbVector3(-0.041421f, 1.382587f, -0.130825f),
        new DbVector3(-0.116634f, 1.031356f, -0.176921f),
        new DbVector3(-0.038791f, 0.914591f, -0.176921f),
        new DbVector3(-0.077712f, 0.914591f, -0.176921f),
        new DbVector3(-0.311242f, 0.914591f, -0.060156f),
        new DbVector3(-0.311242f, 1.187042f, -0.021234f),
        new DbVector3(-0.311242f, 0.914591f, 0.056609f),
        new DbVector3(-0.194477f, 0.914591f, 0.134452f),
        new DbVector3(-0.038791f, 0.914591f, 0.173374f),
        new DbVector3(-0.155556f, 1.264885f, 0.173374f),
        new DbVector3(-0.155556f, 1.303807f, 0.173374f),
        new DbVector3(-0.077712f, 1.38165f, 0.173374f),
    };

    public static readonly ConvexHullCollider IdleConvexHull3 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull3Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull4Vertices = new List<DbVector3>
    {
        new DbVector3(-0.038791f, 1.809788f, 0.09553f),
        new DbVector3(0.077974f, 1.770866f, -0.099078f),
        new DbVector3(-0.038791f, 1.809788f, -0.060156f),
        new DbVector3(-0.038791f, 1.770866f, -0.099078f),
        new DbVector3(0.255932f, 1.364282f, -0.030026f),
        new DbVector3(-0.037928f, 1.379588f, 0.127751f),
        new DbVector3(-0.041421f, 1.382587f, -0.130825f),
        new DbVector3(0.272582f, 1.420572f, 0.017687f),
        new DbVector3(0.155817f, 1.420572f, 0.134452f),
        new DbVector3(0.194739f, 1.38165f, -0.137999f),
        new DbVector3(0.039053f, 1.693023f, -0.137999f),
        new DbVector3(0.116896f, 1.731944f, 0.134452f),
        new DbVector3(-0.038791f, 1.731944f, 0.173374f),
        new DbVector3(0.039053f, 1.537336f, 0.173374f),
        new DbVector3(-0.038791f, 1.537336f, 0.173374f),
        new DbVector3(-0.038791f, 1.693023f, -0.137999f),
    };

    public static readonly ConvexHullCollider IdleConvexHull4 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull4Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull5Vertices = new List<DbVector3>
    {
        new DbVector3(-0.077712f, 1.770866f, -0.060156f),
        new DbVector3(-0.077712f, 1.731944f, -0.099078f),
        new DbVector3(-0.233399f, 1.498415f, -0.099078f),
        new DbVector3(-0.023171f, 1.751333f, -0.043354f),
        new DbVector3(-0.077712f, 1.770866f, 0.134452f),
        new DbVector3(-0.27232f, 1.420572f, 0.017687f),
        new DbVector3(-0.233399f, 1.498415f, 0.056609f),
        new DbVector3(-0.155556f, 1.459493f, 0.134452f),
        new DbVector3(-0.037928f, 1.379588f, 0.127751f),
        new DbVector3(-0.27232f, 1.38165f, -0.099078f),
        new DbVector3(-0.27232f, 1.38165f, 0.017687f),
        new DbVector3(-0.194477f, 1.498415f, -0.137999f),
        new DbVector3(-0.041421f, 1.382587f, -0.130825f),
        new DbVector3(-0.02453f, 1.748201f, 0.108325f),
        new DbVector3(-0.022385f, 1.718545f, -0.072878f),
        new DbVector3(-0.137963f, 1.374419f, 0.106112f),
    };

    public static readonly ConvexHullCollider IdleConvexHull5 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull5Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull6Vertices = new List<DbVector3>
    {
        new DbVector3(-0.311242f, 0.058315f, 0.290139f),
        new DbVector3(-0.311242f, 0.058315f, 0.134452f),
        new DbVector3(-0.311242f, -0.019528f, 0.251217f),
        new DbVector3(-0.089574f, 0.517253f, 0.158538f),
        new DbVector3(-0.233399f, 0.486453f, 0.134452f),
        new DbVector3(-0.194477f, 0.486453f, -0.021234f),
        new DbVector3(-0.233399f, 0.486453f, 0.017687f),
        new DbVector3(-0.27232f, -0.019528f, 0.290139f),
        new DbVector3(-0.233399f, -0.019528f, -0.021234f),
        new DbVector3(-0.116634f, -0.019528f, -0.021234f),
        new DbVector3(-0.116634f, -0.019528f, 0.09553f),
        new DbVector3(-0.077712f, 0.097237f, 0.09553f),
        new DbVector3(-0.194477f, -0.019528f, 0.290139f),
        new DbVector3(-0.077712f, 0.369688f, 0.173374f),
        new DbVector3(-0.194477f, 0.058315f, 0.290139f),
        new DbVector3(-0.099717f, 0.478827f, 0.007778f),
    };

    public static readonly ConvexHullCollider IdleConvexHull6 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull6Vertices
    };

    public static readonly List<DbVector3> IdleConvexHull7Vertices = new List<DbVector3>
    {
        new DbVector3(-0.038791f, 0.836748f, -0.137999f),
        new DbVector3(-0.038666f, 0.917621f, -0.132307f),
        new DbVector3(0.116896f, 0.875669f, -0.137999f),
        new DbVector3(0.039053f, 0.486453f, -0.099078f),
        new DbVector3(-0.061049f, 0.733069f, -0.034603f),
        new DbVector3(0.155817f, 0.525375f, 0.134452f),
        new DbVector3(0.311504f, 0.719983f, 0.056609f),
        new DbVector3(0.272582f, 0.914591f, 0.09553f),
        new DbVector3(-0.035979f, 0.916849f, 0.131981f),
        new DbVector3(-0.055774f, 0.761489f, 0.125417f),
        new DbVector3(0.092131f, 0.928607f, -0.121101f),
        new DbVector3(0.000131f, 0.525375f, 0.09553f),
        new DbVector3(0.039053f, 0.836748f, -0.137999f),
        new DbVector3(0.272582f, 0.914591f, -0.060156f),
        new DbVector3(0.311504f, 0.681061f, -0.060156f),
        new DbVector3(0.194739f, 0.486453f, -0.060156f),
    };

    public static readonly ConvexHullCollider IdleConvexHull7 = new ConvexHullCollider
    {
        VerticesLocal = IdleConvexHull7Vertices
    };

    public static readonly List<ConvexHullCollider> IdleHullsConvexHulls = new List<ConvexHullCollider>
    {
        IdleConvexHull0,
        IdleConvexHull1,
        IdleConvexHull2,
        IdleConvexHull3,
        IdleConvexHull4,
        IdleConvexHull5,
        IdleConvexHull6,
        IdleConvexHull7,
    };

    public static readonly ComplexCollider MagicianIdleCollider = new ComplexCollider
    {
        ConvexHulls = IdleHullsConvexHulls
    };
}

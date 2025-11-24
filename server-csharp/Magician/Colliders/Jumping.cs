using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> JumpingConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(-0.231718f, 0.383279f, 0.153465f),
        new DbVector3(-0.192214f, 0.659808f, 0.074457f),
        new DbVector3(0.005307f, 0.264767f, -0.083559f),
        new DbVector3(0.005307f, 0.383279f, -0.083559f),
        new DbVector3(0.123819f, 0.422783f, -0.083559f),
        new DbVector3(0.202827f, 0.185759f, -0.004551f),
        new DbVector3(0.084315f, 0.146255f, 0.074457f),
        new DbVector3(0.202827f, 0.146255f, 0.153465f),
        new DbVector3(-0.231718f, 0.343775f, 0.311482f),
        new DbVector3(-0.231718f, 0.422783f, 0.311482f),
        new DbVector3(-0.192214f, 0.422783f, 0.350986f),
        new DbVector3(-0.176636f, 0.66603f, 0.22486f),
        new DbVector3(0.202827f, 0.659808f, 0.232473f),
        new DbVector3(-0.15271f, 0.343775f, 0.350986f),
        new DbVector3(0.123819f, 0.146255f, 0.192969f),
        new DbVector3(-0.192214f, 0.343775f, 0.350986f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull0Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull1Vertices = new List<DbVector3>
    {
        new DbVector3(0.047051f, 1.686913f, 0.153465f),
        new DbVector3(-0.035907f, 1.923937f, 0.153465f),
        new DbVector3(-0.118866f, 1.647408f, 0.074457f),
        new DbVector3(-0.121792f, 1.578712f, -0.148288f),
        new DbVector3(-0.077387f, 1.568401f, -0.202071f),
        new DbVector3(-0.108556f, 1.563357f, 0.057466f),
        new DbVector3(-0.118866f, 1.647408f, -0.202071f),
        new DbVector3(0.005572f, 1.647408f, -0.202071f),
        new DbVector3(-0.118866f, 1.726417f, -0.162567f),
        new DbVector3(0.08853f, 2.002945f, -0.083559f),
        new DbVector3(0.295927f, 1.568401f, -0.123063f),
        new DbVector3(0.412097f, 1.554168f, 0.118745f),
        new DbVector3(-0.062429f, 1.561226f, 0.080556f),
        new DbVector3(-0.035907f, 2.002945f, -0.004551f),
        new DbVector3(0.13001f, 2.002945f, 0.113961f),
        new DbVector3(0.13001f, 1.963441f, 0.153465f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull1 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull1Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull2Vertices = new List<DbVector3>
    {
        new DbVector3(-0.192214f, 1.726417f, -0.083559f),
        new DbVector3(-0.192214f, 1.726417f, -0.123063f),
        new DbVector3(-0.54775f, 1.489393f, 0.153465f),
        new DbVector3(-0.502264f, 1.432154f, 0.179636f),
        new DbVector3(-0.529699f, 1.430329f, 0.118957f),
        new DbVector3(-0.508246f, 1.489393f, 0.192969f),
        new DbVector3(-0.10577f, 1.526981f, 0.065772f),
        new DbVector3(-0.15271f, 1.489393f, -0.123063f),
        new DbVector3(-0.113206f, 1.647408f, 0.074457f),
        new DbVector3(-0.15271f, 1.726417f, -0.044055f),
        new DbVector3(-0.113206f, 1.726417f, -0.162567f),
        new DbVector3(-0.113206f, 1.528897f, -0.162567f),
        new DbVector3(-0.231718f, 1.647408f, -0.162567f),
        new DbVector3(-0.35023f, 1.489393f, -0.123063f),
        new DbVector3(-0.35023f, 1.449888f, -0.083559f),
        new DbVector3(-0.468742f, 1.528897f, -0.044055f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull2 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull2Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull3Vertices = new List<DbVector3>
    {
        new DbVector3(0.163323f, 1.489393f, 0.58801f),
        new DbVector3(0.242331f, 1.449888f, 0.627514f),
        new DbVector3(0.202827f, 1.528897f, 0.58801f),
        new DbVector3(0.360843f, 1.568401f, 0.271977f),
        new DbVector3(0.202827f, 1.528897f, 0.429994f),
        new DbVector3(0.321339f, 1.489393f, 0.627514f),
        new DbVector3(0.242331f, 1.37088f, 0.58801f),
        new DbVector3(0.479355f, 1.410384f, 0.232473f),
        new DbVector3(0.479355f, 1.528897f, 0.271977f),
        new DbVector3(0.439851f, 1.568401f, 0.232473f),
        new DbVector3(0.360843f, 1.528897f, 0.548506f),
        new DbVector3(0.163323f, 1.489393f, 0.509002f),
        new DbVector3(0.163323f, 1.449888f, 0.58801f),
        new DbVector3(0.242331f, 1.410384f, 0.627514f),
        new DbVector3(0.321339f, 1.528897f, 0.58801f),
        new DbVector3(0.321339f, 1.410384f, 0.232473f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull3 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull3Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull4Vertices = new List<DbVector3>
    {
        new DbVector3(-0.666262f, 1.212864f, 0.192969f),
        new DbVector3(-0.666262f, 1.212864f, 0.232473f),
        new DbVector3(-0.666262f, 1.331376f, 0.271977f),
        new DbVector3(-0.626758f, 1.410384f, 0.153465f),
        new DbVector3(-0.626758f, 1.410384f, 0.271977f),
        new DbVector3(-0.626758f, 1.37088f, 0.311482f),
        new DbVector3(-0.468742f, 1.410384f, 0.271977f),
        new DbVector3(-0.54775f, 1.449888f, 0.192969f),
        new DbVector3(-0.54775f, 1.212864f, 0.311482f),
        new DbVector3(-0.468742f, 1.331376f, 0.271977f),
        new DbVector3(-0.626758f, 1.212864f, 0.311482f),
        new DbVector3(-0.429238f, 1.37088f, 0.074457f),
        new DbVector3(-0.468742f, 1.410384f, -0.004551f),
        new DbVector3(-0.429169f, 1.450136f, -0.004455f),
        new DbVector3(-0.447056f, 1.454433f, 0.009432f),
        new DbVector3(-0.54775f, 1.449888f, 0.074457f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull4 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull4Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull5Vertices = new List<DbVector3>
    {
        new DbVector3(-0.141507f, 1.568401f, 0.113961f),
        new DbVector3(-0.146469f, 1.083076f, 0.216825f),
        new DbVector3(-0.141507f, 1.133856f, 0.232473f),
        new DbVector3(-0.092127f, 1.094352f, 0.232473f),
        new DbVector3(0.197598f, 1.086917f, 0.139729f),
        new DbVector3(0.204153f, 1.568401f, 0.153465f),
        new DbVector3(-0.14499f, 1.578712f, -0.148288f),
        new DbVector3(0.105393f, 1.331376f, -0.202071f),
        new DbVector3(0.204153f, 1.094352f, -0.162567f),
        new DbVector3(-0.141507f, 1.094352f, -0.123063f),
        new DbVector3(-0.092127f, 1.094352f, -0.202071f),
        new DbVector3(-0.141507f, 1.133856f, -0.162567f),
        new DbVector3(0.006633f, 1.331376f, -0.202071f),
        new DbVector3(-0.092127f, 1.252368f, -0.202071f),
        new DbVector3(0.154773f, 1.133856f, -0.202071f),
        new DbVector3(0.105393f, 1.094352f, -0.202071f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull5 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull5Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull6Vertices = new List<DbVector3>
    {
        new DbVector3(0.113508f, 1.109226f, -0.142694f),
        new DbVector3(0.123819f, 1.054848f, -0.162567f),
        new DbVector3(-0.048226f, 1.111685f, -0.145475f),
        new DbVector3(-0.073702f, 1.054848f, -0.162567f),
        new DbVector3(0.202827f, 0.659808f, 0.034953f),
        new DbVector3(0.005307f, 0.659808f, 0.074457f),
        new DbVector3(-0.095444f, 1.025246f, 0.17837f),
        new DbVector3(-0.089226f, 1.091853f, 0.181949f),
        new DbVector3(0.044811f, 0.857328f, 0.271978f),
        new DbVector3(0.163323f, 1.094352f, 0.192969f),
        new DbVector3(0.202827f, 0.659808f, 0.232473f),
        new DbVector3(0.242331f, 1.094352f, 0.113961f),
        new DbVector3(0.005307f, 0.659808f, 0.153465f),
        new DbVector3(0.044811f, 0.659808f, 0.232473f),
        new DbVector3(0.044811f, 0.738816f, 0.271978f),
        new DbVector3(0.242331f, 1.015344f, -0.004551f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull6 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull6Vertices
    };

    public static readonly List<DbVector3> JumpingConvexHull7Vertices = new List<DbVector3>
    {
        new DbVector3(-0.231718f, 0.659808f, 0.153465f),
        new DbVector3(-0.197259f, 1.101151f, 0.047289f),
        new DbVector3(-0.050548f, 1.104521f, -0.097265f),
        new DbVector3(-0.073702f, 0.97584f, 0.232473f),
        new DbVector3(-0.113206f, 1.094352f, 0.271977f),
        new DbVector3(-0.15271f, 0.817824f, 0.39049f),
        new DbVector3(-0.113206f, 0.659808f, 0.271978f),
        new DbVector3(-0.271222f, 0.896832f, 0.39049f),
        new DbVector3(-0.231718f, 1.015344f, 0.39049f),
        new DbVector3(-0.271222f, 1.054848f, 0.350986f),
        new DbVector3(-0.310726f, 0.857328f, 0.350986f),
        new DbVector3(-0.310726f, 0.97584f, 0.232473f),
        new DbVector3(-0.15271f, 1.054848f, 0.350986f),
        new DbVector3(-0.231718f, 1.094352f, 0.232473f),
        new DbVector3(-0.271222f, 0.77832f, 0.350986f),
        new DbVector3(-0.093515f, 0.639485f, 0.133578f),
    };

    public static readonly ConvexHullCollider JumpingConvexHull7 = new ConvexHullCollider
    {
        VerticesLocal = JumpingConvexHull7Vertices
    };

    public static readonly List<ConvexHullCollider> JumpHullsConvexHulls = new List<ConvexHullCollider>
    {
        JumpingConvexHull0,
        JumpingConvexHull1,
        JumpingConvexHull2,
        JumpingConvexHull3,
        JumpingConvexHull4,
        JumpingConvexHull5,
        JumpingConvexHull6,
        JumpingConvexHull7,
    };

    public static readonly ComplexCollider MagicianJumpingCollider = new ComplexCollider
    {
        ConvexHulls = JumpHullsConvexHulls
    };
}

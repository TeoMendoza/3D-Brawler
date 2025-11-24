using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> CrouchConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(0.249701f, 0.56804f, -0.165017f),
        new DbVector3(0.104816f, 0.683949f, -0.280925f),
        new DbVector3(0.191747f, 0.683949f, -0.020131f),
        new DbVector3(-0.082808f, 0.56731f, 0.083054f),
        new DbVector3(-0.04007f, 0.56804f, 0.095778f),
        new DbVector3(-0.077826f, 0.633981f, 0.03224f),
        new DbVector3(-0.069048f, 0.683949f, -0.251948f),
        new DbVector3(0.133793f, 0.654971f, 0.037824f),
        new DbVector3(0.104816f, 0.683949f, -0.020131f),
        new DbVector3(0.220724f, 0.220314f, 0.037824f),
        new DbVector3(0.263017f, 0.231972f, 0.034506f),
        new DbVector3(0.220724f, 0.278268f, 0.153732f),
        new DbVector3(-0.081099f, 0.510873f, 0.082188f),
        new DbVector3(-0.069048f, 0.481108f, -0.222971f),
        new DbVector3(0.249701f, 0.56804f, 0.066801f),
        new DbVector3(-0.04007f, 0.597017f, -0.280925f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull0Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull1Vertices = new List<DbVector3>
    {
        new DbVector3(-0.532682f, 0.712926f, -0.049108f),
        new DbVector3(-0.300865f, 1.002697f, -0.049108f),
        new DbVector3(-0.387797f, 0.828834f, -0.165017f),
        new DbVector3(-0.11553f, 0.675679f, -0.158915f),
        new DbVector3(-0.524492f, 0.668338f, 0.007418f),
        new DbVector3(-0.271888f, 1.002697f, 0.124755f),
        new DbVector3(-0.069149f, 1.034515f, 0.126578f),
        new DbVector3(-0.184956f, 0.97372f, 0.153732f),
        new DbVector3(-0.155979f, 0.828834f, 0.153732f),
        new DbVector3(-0.069048f, 0.741903f, 0.124755f),
        new DbVector3(-0.058979f, 0.73481f, -0.18723f),
        new DbVector3(-0.098025f, 0.799857f, -0.193994f),
        new DbVector3(-0.069048f, 1.031675f, -0.107062f),
        new DbVector3(-0.098025f, 0.886789f, -0.165017f),
        new DbVector3(-0.213934f, 1.031675f, -0.078085f),
        new DbVector3(-0.503808f, 0.673219f, 0.031031f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull1 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull1Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull2Vertices = new List<DbVector3>
    {
        new DbVector3(-0.184956f, 1.292469f, 0.269641f),
        new DbVector3(-0.271888f, 1.060652f, -0.020131f),
        new DbVector3(-0.271888f, 1.060652f, 0.095778f),
        new DbVector3(-0.155979f, 1.060652f, -0.078085f),
        new DbVector3(-0.127002f, 1.321446f, 0.066801f),
        new DbVector3(-0.069048f, 1.060652f, -0.078085f),
        new DbVector3(-0.259293f, 1.017399f, 0.079124f),
        new DbVector3(-0.155979f, 1.321446f, 0.095778f),
        new DbVector3(-0.155979f, 1.089629f, 0.269641f),
        new DbVector3(-0.069048f, 1.031675f, 0.240664f),
        new DbVector3(-0.127002f, 1.292469f, 0.298618f),
        new DbVector3(-0.069048f, 1.292469f, 0.298618f),
        new DbVector3(-0.069048f, 1.350423f, 0.095778f),
        new DbVector3(-0.068987f, 1.023129f, -0.065289f),
        new DbVector3(-0.155979f, 1.321446f, 0.269641f),
        new DbVector3(-0.069048f, 1.350423f, 0.240664f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull2 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull2Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull3Vertices = new List<DbVector3>
    {
        new DbVector3(0.133793f, 0.683949f, 0.037824f),
        new DbVector3(0.019505f, 0.67481f, -0.245886f),
        new DbVector3(0.133793f, 0.712926f, -0.251948f),
        new DbVector3(-0.069048f, 0.683949f, -0.020131f),
        new DbVector3(-0.069048f, 0.77088f, -0.222971f),
        new DbVector3(-0.069048f, 0.741903f, 0.124755f),
        new DbVector3(0.017884f, 0.712926f, -0.251948f),
        new DbVector3(0.263432f, 0.972641f, -0.096932f),
        new DbVector3(0.104816f, 1.031675f, -0.107062f),
        new DbVector3(0.249701f, 0.944743f, 0.095778f),
        new DbVector3(-0.04007f, 1.031675f, -0.107062f),
        new DbVector3(-0.075662f, 0.995924f, -0.081007f),
        new DbVector3(-0.069149f, 1.034515f, 0.126578f),
        new DbVector3(0.046861f, 0.828834f, 0.153732f),
        new DbVector3(0.16277f, 1.002697f, 0.153732f),
        new DbVector3(0.249701f, 1.031675f, 0.095778f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull3 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull3Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull4Vertices = new List<DbVector3>
    {
        new DbVector3(0.017884f, 1.321446f, 0.240664f),
        new DbVector3(-0.069048f, 1.292469f, 0.298618f),
        new DbVector3(-0.04007f, 1.176561f, 0.298618f),
        new DbVector3(0.249701f, 1.031675f, 0.095778f),
        new DbVector3(0.249701f, 1.089629f, -0.020131f),
        new DbVector3(-0.069048f, 1.350423f, 0.095778f),
        new DbVector3(0.104816f, 1.089629f, -0.078085f),
        new DbVector3(-0.069048f, 1.292469f, 0.037824f),
        new DbVector3(-0.069048f, 1.350423f, 0.240664f),
        new DbVector3(-0.04007f, 1.060652f, 0.240664f),
        new DbVector3(-0.080635f, 1.068364f, 0.227754f),
        new DbVector3(-0.068987f, 1.023129f, -0.065289f),
        new DbVector3(-0.069048f, 1.176561f, 0.298618f),
        new DbVector3(0.08768f, 1.028461f, -0.065182f),
        new DbVector3(-0.069048f, 1.060652f, -0.078085f),
        new DbVector3(-0.011093f, 1.089629f, -0.078085f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull4 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull4Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull5Vertices = new List<DbVector3>
    {
        new DbVector3(-0.56166f, 0.423154f, 0.066801f),
        new DbVector3(-0.391625f, 0.692828f, -0.105337f),
        new DbVector3(-0.532682f, 0.510086f, 0.211687f),
        new DbVector3(-0.648591f, 0.452131f, 0.037824f),
        new DbVector3(-0.474728f, 0.654971f, -0.107062f),
        new DbVector3(-0.677568f, 0.423154f, 0.18271f),
        new DbVector3(-0.677568f, 0.510086f, 0.095778f),
        new DbVector3(-0.619614f, 0.597017f, 0.008846f),
        new DbVector3(-0.56166f, 0.683949f, 0.037824f),
        new DbVector3(-0.590637f, 0.56804f, 0.211687f),
        new DbVector3(-0.532682f, 0.683949f, 0.066801f),
        new DbVector3(-0.677568f, 0.510086f, 0.211687f),
        new DbVector3(-0.474728f, 0.683949f, 0.066801f),
        new DbVector3(-0.619614f, 0.423154f, 0.18271f),
        new DbVector3(-0.677568f, 0.481108f, 0.211687f),
        new DbVector3(-0.619614f, 0.510086f, 0.008846f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull5 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull5Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull6Vertices = new List<DbVector3>
    {
        new DbVector3(0.278679f, 0.191337f, -0.280925f),
        new DbVector3(0.36561f, 0.220314f, -0.251948f),
        new DbVector3(0.394587f, 0.104405f, -0.33888f),
        new DbVector3(0.278679f, -0.011504f, -0.251948f),
        new DbVector3(0.249701f, 0.191337f, -0.251948f),
        new DbVector3(0.249223f, 0.373369f, -0.073198f),
        new DbVector3(0.481519f, 0.075428f, -0.193994f),
        new DbVector3(0.250064f, 0.360705f, -0.051627f),
        new DbVector3(0.273228f, 0.365595f, -0.046007f),
        new DbVector3(0.249701f, 0.191337f, -0.049108f),
        new DbVector3(0.249701f, 0.133382f, -0.193994f),
        new DbVector3(0.36561f, 0.307245f, -0.078085f),
        new DbVector3(0.35682f, 0.29294f, -0.038313f),
        new DbVector3(0.481519f, -0.011504f, -0.049108f),
        new DbVector3(0.36561f, -0.011504f, -0.049108f),
        new DbVector3(0.394587f, -0.011504f, -0.309902f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull6 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull6Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull7Vertices = new List<DbVector3>
    {
        new DbVector3(0.307656f, 0.3652f, -0.049108f),
        new DbVector3(0.250064f, 0.360705f, -0.051627f),
        new DbVector3(0.278679f, 0.3652f, 0.211687f),
        new DbVector3(0.452542f, 0.075428f, -0.049108f),
        new DbVector3(0.394587f, 0.307245f, -0.049108f),
        new DbVector3(0.394587f, 0.3652f, 0.037824f),
        new DbVector3(0.394587f, 0.220314f, 0.153732f),
        new DbVector3(0.452542f, -0.011504f, -0.020131f),
        new DbVector3(0.25916f, 0.350491f, 0.166468f),
        new DbVector3(0.249701f, 0.336223f, 0.211687f),
        new DbVector3(0.307656f, 0.278268f, 0.211687f),
        new DbVector3(0.36561f, 0.3652f, 0.18271f),
        new DbVector3(0.394587f, -0.011504f, -0.020131f),
        new DbVector3(0.249701f, 0.278268f, 0.211687f),
        new DbVector3(0.249701f, 0.191337f, -0.049108f),
        new DbVector3(0.36561f, 0.336223f, -0.049108f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull7 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull7Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull8Vertices = new List<DbVector3>
    {
        new DbVector3(0.278679f, 1.060652f, -0.049108f),
        new DbVector3(0.278679f, 1.060652f, 0.066801f),
        new DbVector3(0.36561f, 1.031675f, -0.13604f),
        new DbVector3(0.249701f, 1.002697f, -0.13604f),
        new DbVector3(0.235787f, 1.046329f, 0.056894f),
        new DbVector3(0.307656f, 0.944743f, -0.165017f),
        new DbVector3(0.452542f, 0.915766f, -0.165017f),
        new DbVector3(0.597427f, 0.77088f, -0.020131f),
        new DbVector3(0.597427f, 0.712926f, 0.037824f),
        new DbVector3(0.626405f, 0.683949f, 0.008846f),
        new DbVector3(0.278679f, 0.97372f, 0.066801f),
        new DbVector3(0.249701f, 0.915766f, -0.13604f),
        new DbVector3(0.486435f, 0.668107f, 0.027075f),
        new DbVector3(0.234766f, 0.978071f, 0.056823f),
        new DbVector3(0.481519f, 0.683949f, -0.078085f),
        new DbVector3(0.597427f, 0.683949f, -0.107062f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull8 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull8Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull9Vertices = new List<DbVector3>
    {
        new DbVector3(-0.155979f, 0.683949f, -0.020131f),
        new DbVector3(-0.242911f, 0.597017f, -0.020131f),
        new DbVector3(-0.197158f, 0.607683f, 0.038676f),
        new DbVector3(-0.22026f, 0.580969f, 0.039274f),
        new DbVector3(-0.127002f, 0.683949f, -0.222971f),
        new DbVector3(-0.060345f, 0.708828f, 0.009516f),
        new DbVector3(-0.069048f, 0.423154f, 0.037824f),
        new DbVector3(-0.069048f, 0.683949f, -0.251948f),
        new DbVector3(-0.227207f, 0.445579f, 0.041979f),
        new DbVector3(-0.242911f, 0.423154f, 0.008846f),
        new DbVector3(-0.242911f, 0.452131f, -0.020131f),
        new DbVector3(-0.213934f, 0.452131f, -0.078085f),
        new DbVector3(-0.127002f, 0.423154f, -0.107062f),
        new DbVector3(-0.069048f, 0.539063f, -0.251948f),
        new DbVector3(-0.098025f, 0.539063f, -0.251948f),
        new DbVector3(-0.127002f, 0.56804f, -0.222971f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull9 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull9Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull10Vertices = new List<DbVector3>
    {
        new DbVector3(-0.358819f, 0.3652f, 0.095778f),
        new DbVector3(-0.271888f, 0.3652f, 0.298618f),
        new DbVector3(-0.387797f, 0.3652f, 0.269641f),
        new DbVector3(-0.271888f, 0.56804f, 0.037824f),
        new DbVector3(-0.184956f, 0.654971f, 0.095778f),
        new DbVector3(-0.077826f, 0.633981f, 0.03224f),
        new DbVector3(-0.358819f, 0.539063f, 0.124755f),
        new DbVector3(-0.387797f, 0.510086f, 0.298618f),
        new DbVector3(-0.300865f, 0.597017f, 0.298618f),
        new DbVector3(-0.387797f, 0.539063f, 0.269641f),
        new DbVector3(-0.329842f, 0.597017f, 0.18271f),
        new DbVector3(-0.069048f, 0.423154f, 0.037824f),
        new DbVector3(-0.069048f, 0.481108f, 0.124755f),
        new DbVector3(-0.069048f, 0.654971f, 0.095778f),
        new DbVector3(-0.213934f, 0.56804f, 0.298618f),
        new DbVector3(-0.069048f, 0.625994f, 0.124755f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull10 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull10Vertices
    };

    public static readonly List<DbVector3> CrouchConvexHull11Vertices = new List<DbVector3>
    {
        new DbVector3(-0.213934f, 0.3652f, 0.153732f),
        new DbVector3(-0.358819f, -0.011504f, 0.356573f),
        new DbVector3(-0.271888f, -0.011504f, 0.124755f),
        new DbVector3(-0.474728f, 0.046451f, 0.211687f),
        new DbVector3(-0.387797f, -0.011504f, 0.037824f),
        new DbVector3(-0.474728f, -0.011504f, 0.298618f),
        new DbVector3(-0.387797f, 0.16236f, 0.037824f),
        new DbVector3(-0.300865f, 0.075428f, 0.008846f),
        new DbVector3(-0.358819f, 0.3652f, 0.095778f),
        new DbVector3(-0.271888f, -0.011504f, 0.037824f),
        new DbVector3(-0.474728f, 0.046451f, 0.298618f),
        new DbVector3(-0.416774f, 0.046451f, 0.356573f),
        new DbVector3(-0.358819f, 0.3652f, 0.269641f),
        new DbVector3(-0.358819f, 0.046451f, 0.356573f),
        new DbVector3(-0.242911f, 0.3652f, 0.095778f),
        new DbVector3(-0.416774f, -0.011504f, 0.356573f),
    };

    public static readonly ConvexHullCollider CrouchConvexHull11 = new ConvexHullCollider
    {
        VerticesLocal = CrouchConvexHull11Vertices
    };

    public static readonly List<ConvexHullCollider> CrouchHullsConvexHulls = new List<ConvexHullCollider>
    {
        CrouchConvexHull0,
        CrouchConvexHull1,
        CrouchConvexHull2,
        CrouchConvexHull3,
        CrouchConvexHull4,
        CrouchConvexHull5,
        CrouchConvexHull6,
        CrouchConvexHull7,
        CrouchConvexHull8,
        CrouchConvexHull9,
        CrouchConvexHull10,
        CrouchConvexHull11,
    };

    public static readonly ComplexCollider MagicianCrouchingCollider = new ComplexCollider
    {
        ConvexHulls = CrouchHullsConvexHulls
    };
}

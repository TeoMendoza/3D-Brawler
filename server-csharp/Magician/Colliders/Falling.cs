using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static readonly List<DbVector3> FallingConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(-0.172694f, 0.340379f, -0.171863f),
        new DbVector3(-0.206231f, 0.10562f, -0.104789f),
        new DbVector3(-0.206231f, 0.340379f, -0.104789f),
        new DbVector3(-0.072083f, 0.072083f, -0.071252f),
        new DbVector3(0.196212f, 0.306842f, -0.004178f),
        new DbVector3(-0.072083f, 0.038547f, 0.029359f),
        new DbVector3(-0.206231f, 0.474527f, -0.004178f),
        new DbVector3(-0.172694f, 0.474527f, -0.037715f),
        new DbVector3(-0.172694f, 0.038547f, 0.029359f),
        new DbVector3(-0.206231f, 0.44099f, 0.163507f),
        new DbVector3(-0.206231f, 0.072083f, -0.004178f),
        new DbVector3(0.129138f, 0.273305f, 0.062896f),
        new DbVector3(0.196212f, 0.474527f, -0.004178f),
        new DbVector3(-0.10562f, 0.239768f, -0.171863f),
        new DbVector3(-0.072083f, 0.340379f, -0.171863f),
        new DbVector3(0.077162f, 0.49418f, -0.093816f),
    };

    public static readonly ConvexHullCollider FallingConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull0Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull1Vertices = new List<DbVector3>
    {
        new DbVector3(0.114352f, 0.856079f, 0.252639f),
        new DbVector3(0.229749f, 0.843433f, 0.062896f),
        new DbVector3(0.069735f, 0.845722f, 0.057743f),
        new DbVector3(0.263286f, 0.843433f, 0.297655f),
        new DbVector3(0.162675f, 0.843433f, 0.297655f),
        new DbVector3(0.083073f, 0.578092f, 0.045912f),
        new DbVector3(0.062064f, 0.575138f, 0.096433f),
        new DbVector3(0.105324f, 0.553048f, 0.04459f),
        new DbVector3(0.196212f, 0.608675f, 0.062896f),
        new DbVector3(0.162675f, 0.541601f, 0.096433f),
        new DbVector3(0.263286f, 0.742823f, 0.264118f),
        new DbVector3(0.129138f, 0.742823f, 0.297655f),
        new DbVector3(0.062064f, 0.843433f, 0.197044f),
        new DbVector3(0.129138f, 0.809897f, 0.297655f),
        new DbVector3(0.263286f, 0.843433f, 0.12997f),
        new DbVector3(0.229749f, 0.742823f, 0.297655f),
    };

    public static readonly ConvexHullCollider FallingConvexHull1 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull1Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull2Vertices = new List<DbVector3>
    {
        new DbVector3(-0.239768f, 0.742823f, -0.004178f),
        new DbVector3(-0.191598f, 0.479198f, 0.065943f),
        new DbVector3(-0.239768f, 0.843433f, 0.062896f),
        new DbVector3(-0.208161f, 0.748864f, 0.070073f),
        new DbVector3(-0.038546f, 0.742823f, 0.062896f),
        new DbVector3(-0.065451f, 0.839436f, 0.050227f),
        new DbVector3(-0.206231f, 0.742823f, -0.138326f),
        new DbVector3(-0.154246f, 0.469607f, 0.01181f),
        new DbVector3(-0.112043f, 0.474659f, 0.062843f),
        new DbVector3(-0.10562f, 0.809897f, -0.238937f),
        new DbVector3(-0.206231f, 0.809897f, -0.171863f),
        new DbVector3(-0.206231f, 0.843433f, -0.171863f),
        new DbVector3(-0.038184f, 0.846183f, -0.236951f),
        new DbVector3(-0.09491f, 0.857234f, -0.228526f),
        new DbVector3(-0.239768f, 0.843433f, -0.071252f),
        new DbVector3(-0.035412f, 0.835843f, -0.223009f),
    };

    public static readonly ConvexHullCollider FallingConvexHull2 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull2Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull3Vertices = new List<DbVector3>
    {
        new DbVector3(-0.239768f, 0.508064f, 0.062896f),
        new DbVector3(-0.065451f, 0.839436f, 0.050227f),
        new DbVector3(-0.072083f, 0.508064f, 0.163507f),
        new DbVector3(-0.197231f, 0.84002f, 0.057604f),
        new DbVector3(-0.239768f, 0.809897f, 0.062896f),
        new DbVector3(-0.206231f, 0.843433f, 0.12997f),
        new DbVector3(-0.239768f, 0.709286f, 0.230581f),
        new DbVector3(-0.273305f, 0.575138f, 0.197044f),
        new DbVector3(-0.239768f, 0.541601f, 0.264118f),
        new DbVector3(-0.172694f, 0.709286f, 0.297655f),
        new DbVector3(-0.038546f, 0.843433f, 0.096433f),
        new DbVector3(-0.172694f, 0.742823f, 0.264118f),
        new DbVector3(-0.10562f, 0.709286f, 0.297655f),
        new DbVector3(-0.10562f, 0.642212f, 0.297655f),
        new DbVector3(-0.273305f, 0.575138f, 0.12997f),
        new DbVector3(-0.206231f, 0.474527f, 0.197044f),
    };

    public static readonly ConvexHullCollider FallingConvexHull3 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull3Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull4Vertices = new List<DbVector3>
    {
        new DbVector3(0.042036f, 1.535356f, 0.172834f),
        new DbVector3(0.056419f, 1.217935f, -0.133976f),
        new DbVector3(0.061082f, 1.214907f, 0.123755f),
        new DbVector3(0.062064f, 1.312951f, -0.138326f),
        new DbVector3(0.062064f, 1.447099f, -0.037715f),
        new DbVector3(0.052281f, 1.48185f, 0.226878f),
        new DbVector3(0.055475f, 1.544817f, 0.230196f),
        new DbVector3(0.33036f, 1.21234f, 0.163507f),
        new DbVector3(0.095601f, 1.480636f, 0.230581f),
        new DbVector3(0.40485f, 1.21414f, 0.149745f),
        new DbVector3(0.095601f, 1.21234f, -0.138326f),
        new DbVector3(0.397434f, 1.279414f, -0.004178f),
        new DbVector3(0.095601f, 1.312951f, -0.138326f),
        new DbVector3(0.263286f, 1.380025f, -0.037715f),
        new DbVector3(0.397434f, 1.312951f, 0.12997f),
        new DbVector3(0.129138f, 1.380025f, -0.104789f),
    };

    public static readonly ConvexHullCollider FallingConvexHull4 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull4Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull5Vertices = new List<DbVector3>
    {
        new DbVector3(-0.303284f, 1.24938f, 0.021832f),
        new DbVector3(-0.239768f, 0.843433f, 0.062896f),
        new DbVector3(-0.206231f, 1.21234f, 0.12997f),
        new DbVector3(-0.239768f, 0.843433f, -0.071252f),
        new DbVector3(-0.318205f, 1.218667f, -0.064784f),
        new DbVector3(-0.139157f, 0.843433f, -0.272474f),
        new DbVector3(-0.10562f, 1.011118f, -0.306011f),
        new DbVector3(0.062058f, 1.245566f, 0.126187f),
        new DbVector3(-0.072083f, 0.843433f, 0.12997f),
        new DbVector3(0.069735f, 0.845722f, 0.057743f),
        new DbVector3(0.062064f, 1.044655f, -0.306011f),
        new DbVector3(0.049245f, 1.235941f, -0.147559f),
        new DbVector3(-0.310632f, 1.235572f, 0.006556f),
        new DbVector3(-0.10562f, 1.245877f, -0.171863f),
        new DbVector3(-0.159665f, 0.833353f, 0.113564f),
        new DbVector3(0.062064f, 0.843433f, -0.272474f),
    };

    public static readonly ConvexHullCollider FallingConvexHull5 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull5Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull6Vertices = new List<DbVector3>
    {
        new DbVector3(-0.508064f, 0.910507f, 0.264118f),
        new DbVector3(-0.642212f, 1.011118f, 0.264118f),
        new DbVector3(-0.642212f, 0.944044f, 0.264118f),
        new DbVector3(-0.303284f, 1.24938f, 0.021832f),
        new DbVector3(-0.44099f, 1.245877f, -0.138326f),
        new DbVector3(-0.340379f, 1.245877f, -0.138326f),
        new DbVector3(-0.407453f, 1.078192f, -0.104789f),
        new DbVector3(-0.508064f, 1.111729f, -0.104789f),
        new DbVector3(-0.44099f, 1.245877f, 0.029359f),
        new DbVector3(-0.508064f, 1.21234f, 0.029359f),
        new DbVector3(-0.474527f, 1.245877f, -0.104789f),
        new DbVector3(-0.642212f, 1.011118f, 0.096433f),
        new DbVector3(-0.508064f, 1.178803f, -0.104789f),
        new DbVector3(-0.642212f, 0.87697f, 0.163507f),
        new DbVector3(-0.508064f, 0.910507f, 0.197044f),
        new DbVector3(-0.306842f, 1.178803f, -0.104789f),
    };

    public static readonly ConvexHullCollider FallingConvexHull6 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull6Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull7Vertices = new List<DbVector3>
    {
        new DbVector3(-0.005009f, 1.279414f, 0.230581f),
        new DbVector3(-0.139157f, 1.380025f, 0.331192f),
        new DbVector3(-0.071321f, 1.311748f, 0.119732f),
        new DbVector3(0.046314f, 1.395927f, 0.109977f),
        new DbVector3(-0.10562f, 1.614783f, 0.297655f),
        new DbVector3(-0.139157f, 1.547709f, 0.364729f),
        new DbVector3(-0.072083f, 1.614783f, 0.331192f),
        new DbVector3(0.062064f, 1.581246f, 0.12997f),
        new DbVector3(-0.10562f, 1.581246f, 0.12997f),
        new DbVector3(-0.005009f, 1.581246f, 0.364729f),
        new DbVector3(-0.09544f, 1.37742f, 0.112219f),
        new DbVector3(-0.139157f, 1.514173f, 0.163507f),
        new DbVector3(-0.072083f, 1.346488f, 0.331192f),
        new DbVector3(0.028528f, 1.413562f, 0.364729f),
        new DbVector3(-0.139157f, 1.413562f, 0.364729f),
        new DbVector3(0.062064f, 1.380025f, 0.264118f),
    };

    public static readonly ConvexHullCollider FallingConvexHull7 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull7Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull8Vertices = new List<DbVector3>
    {
        new DbVector3(0.410958f, 1.177796f, 0.042291f),
        new DbVector3(0.078386f, 1.191907f, 0.010861f),
        new DbVector3(0.056254f, 0.841312f, 0.03233f),
        new DbVector3(0.0315f, 0.846905f, -0.135969f),
        new DbVector3(0.047521f, 1.129187f, -0.21419f),
        new DbVector3(0.046803f, 0.918697f, -0.282108f),
        new DbVector3(0.056419f, 1.217935f, -0.133976f),
        new DbVector3(0.056092f, 0.998608f, -0.275589f),
        new DbVector3(0.095601f, 1.011118f, -0.306011f),
        new DbVector3(0.393281f, 1.224961f, 0.021948f),
        new DbVector3(0.095601f, 1.145266f, -0.238937f),
        new DbVector3(0.397434f, 1.178803f, -0.004178f),
        new DbVector3(0.196212f, 0.843433f, -0.171863f),
        new DbVector3(0.095601f, 0.910507f, -0.306011f),
        new DbVector3(0.129138f, 0.843433f, -0.2054f),
        new DbVector3(0.229749f, 0.843433f, 0.029359f),
    };

    public static readonly ConvexHullCollider FallingConvexHull8 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull8Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull9Vertices = new List<DbVector3>
    {
        new DbVector3(0.598656f, 0.910507f, 0.264118f),
        new DbVector3(0.397434f, 1.145266f, 0.163507f),
        new DbVector3(0.598656f, 0.87697f, 0.197044f),
        new DbVector3(0.699267f, 0.977581f, 0.331192f),
        new DbVector3(0.413153f, 1.166489f, 0.046273f),
        new DbVector3(0.389646f, 1.289379f, 0.046896f),
        new DbVector3(0.430971f, 1.279414f, -0.004178f),
        new DbVector3(0.38873f, 1.2833f, 0.085656f),
        new DbVector3(0.430971f, 1.312951f, 0.096433f),
        new DbVector3(0.430971f, 1.312951f, 0.029359f),
        new DbVector3(0.799878f, 1.011118f, 0.163507f),
        new DbVector3(0.766341f, 0.87697f, 0.297655f),
        new DbVector3(0.799878f, 0.910507f, 0.163507f),
        new DbVector3(0.404711f, 1.224595f, 0.141706f),
        new DbVector3(0.498045f, 1.245877f, 0.163507f),
        new DbVector3(0.732804f, 1.078192f, 0.230581f),
    };

    public static readonly ConvexHullCollider FallingConvexHull9 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull9Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull10Vertices = new List<DbVector3>
    {
        new DbVector3(-0.206231f, 1.413562f, 0.12997f),
        new DbVector3(-0.206231f, 1.245877f, -0.071252f),
        new DbVector3(-0.18485f, 1.256911f, 0.108816f),
        new DbVector3(0.062058f, 1.245566f, 0.126187f),
        new DbVector3(-0.072083f, 1.547709f, 0.096433f),
        new DbVector3(-0.066928f, 1.538292f, 0.143136f),
        new DbVector3(0.019773f, 1.537808f, 0.146736f),
        new DbVector3(0.062064f, 1.447099f, -0.037715f),
        new DbVector3(-0.210204f, 1.314177f, -0.060755f),
        new DbVector3(-0.206231f, 1.413562f, -0.071252f),
        new DbVector3(-0.10562f, 1.279414f, -0.171863f),
        new DbVector3(0.062064f, 1.279414f, -0.171863f),
        new DbVector3(-0.10562f, 1.245877f, -0.171863f),
        new DbVector3(0.049245f, 1.235941f, -0.147559f),
        new DbVector3(-0.206231f, 1.447099f, -0.037715f),
        new DbVector3(0.062064f, 1.380025f, -0.104789f),
    };

    public static readonly ConvexHullCollider FallingConvexHull10 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull10Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull11Vertices = new List<DbVector3>
    {
        new DbVector3(-0.273305f, 1.413562f, 0.096433f),
        new DbVector3(-0.44099f, 1.279414f, -0.004178f),
        new DbVector3(-0.239768f, 1.312951f, 0.12997f),
        new DbVector3(-0.340379f, 1.380025f, -0.004178f),
        new DbVector3(-0.44099f, 1.279414f, -0.104789f),
        new DbVector3(-0.239768f, 1.447099f, 0.062896f),
        new DbVector3(-0.273305f, 1.245877f, -0.104789f),
        new DbVector3(-0.206231f, 1.346488f, -0.104789f),
        new DbVector3(-0.206231f, 1.245877f, -0.071252f),
        new DbVector3(-0.19649f, 1.419917f, 0.002015f),
        new DbVector3(-0.188046f, 1.25305f, 0.062652f),
        new DbVector3(-0.197331f, 1.397198f, 0.080005f),
        new DbVector3(-0.206231f, 1.447099f, 0.062896f),
        new DbVector3(-0.239768f, 1.279414f, 0.12997f),
        new DbVector3(-0.306842f, 1.245877f, 0.062896f),
        new DbVector3(-0.423899f, 1.228622f, -0.018582f),
    };

    public static readonly ConvexHullCollider FallingConvexHull11 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull11Vertices
    };

    public static readonly List<DbVector3> FallingConvexHull12Vertices = new List<DbVector3>
    {
        new DbVector3(0.028528f, 0.843433f, 0.062896f),
        new DbVector3(0.043641f, 0.860215f, -0.235063f),
        new DbVector3(-0.05037f, 0.84133f, -0.002947f),
        new DbVector3(-0.038184f, 0.846183f, -0.236951f),
        new DbVector3(-0.05674f, 0.780924f, -0.035116f),
        new DbVector3(0.194269f, 0.84407f, 0.06311f),
        new DbVector3(0.062064f, 0.809897f, -0.238937f),
        new DbVector3(-0.046353f, 0.810904f, -0.002896f),
        new DbVector3(0.062064f, 0.508064f, 0.062896f),
        new DbVector3(0.028528f, 0.809897f, 0.062896f),
        new DbVector3(0.028528f, 0.474527f, -0.104789f),
        new DbVector3(0.196212f, 0.508064f, 0.029359f),
        new DbVector3(0.062064f, 0.474527f, -0.138326f),
        new DbVector3(0.162675f, 0.474527f, -0.138326f),
        new DbVector3(0.196212f, 0.809897f, -0.138326f),
        new DbVector3(-0.038546f, 0.77636f, -0.171863f),
    };

    public static readonly ConvexHullCollider FallingConvexHull12 = new ConvexHullCollider
    {
        VerticesLocal = FallingConvexHull12Vertices
    };

    public static readonly List<ConvexHullCollider> FallingHullsConvexHulls = new List<ConvexHullCollider>
    {
        FallingConvexHull0,
        FallingConvexHull1,
        FallingConvexHull2,
        FallingConvexHull3,
        FallingConvexHull4,
        FallingConvexHull5,
        FallingConvexHull6,
        FallingConvexHull7,
        FallingConvexHull8,
        FallingConvexHull9,
        FallingConvexHull10,
        FallingConvexHull11,
        FallingConvexHull12,
    };

    public static readonly ComplexCollider MagicianFallingCollider = new ComplexCollider
    {
        ConvexHulls = FallingHullsConvexHulls
    };
}

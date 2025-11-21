using System.Collections.Generic;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> ConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(0.1904541f, 0.4866093f, 0.07226442f),
        new DbVector3(0.043196f, 0.511324f, 0.055237f),
        new DbVector3(0.06348569f, 0.0633812f, 0.2415556f),
        new DbVector3(0.02116289f, 0.0633812f, 0.1992328f),
        new DbVector3(0.02116289f, -0.0212644f, 0.1992328f),
        new DbVector3(0.1904541f, -0.0212644f, 0.1992328f),
        new DbVector3(0.06348569f, 0.4866093f, -0.1393496f),
        new DbVector3(0.1481313f, 0.4866093f, -0.1393496f),
        new DbVector3(0.02116289f, -0.0212644f, -0.0970268f),
        new DbVector3(0.1904541f, 0.4866093f, -0.0970268f),
        new DbVector3(0.1904541f, -0.0212644f, 0.02994162f),
        new DbVector3(0.1481313f, -0.0212644f, -0.0970268f),
    };

    public static readonly ConvexHullCollider ConvexHull0 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull0Vertices
    };

    public static readonly List<DbVector3> ConvexHull1Vertices = new List<DbVector3>
    {
        new DbVector3(-0.1904511f, 0.0633812f, 0.1992328f),
        new DbVector3(-0.1904511f, -0.0212644f, 0.1992328f),
        new DbVector3(-0.1058055f, 0.0633812f, 0.2415556f),
        new DbVector3(-0.1904511f, 0.4866093f, 0.07226442f),
        new DbVector3(-0.02115991f, -0.0212644f, 0.1992328f),
        new DbVector3(-0.02115991f, -0.0212644f, -0.0970268f),
        new DbVector3(-0.043196f, 0.511323f, 0.055232f),
        new DbVector3(-0.1481283f, -0.0212644f, -0.0970268f),
        new DbVector3(-0.06348271f, 0.4866093f, -0.1393496f),
        new DbVector3(-0.06348271f, 0.0633812f, 0.2415556f),
        new DbVector3(-0.1904511f, 0.4866093f, -0.0970268f),
        new DbVector3(-0.04723886f, 0.4778728f, -0.06923476f),
    };

    public static readonly ConvexHullCollider ConvexHull1 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull1Vertices
    };

    public static readonly List<DbVector3> ConvexHull2Vertices = new List<DbVector3>
    {
        new DbVector3(-0.2327739f, 0.9098372f, 0.07226442f),
        new DbVector3(-0.1058055f, 0.9521601f, 0.15691f),
        new DbVector3(-0.2327739f, 0.9521601f, -0.01238119f),
        new DbVector3(-0.1481283f, 0.7828688f, 0.15691f),
        new DbVector3(-0.1904511f, 0.4866093f, 0.07226442f),
        new DbVector3(-0.02115991f, 0.528932f, 0.1145872f),
        new DbVector3(-0.1058055f, 0.9521601f, -0.1816724f),
        new DbVector3(-0.02115991f, 0.9098372f, -0.1816724f),
        new DbVector3(-0.1904511f, 0.4866093f, -0.0970268f),
        new DbVector3(-0.02020947f, 0.9593424f, -0.1494396f),
        new DbVector3(-0.04723886f, 0.4778728f, -0.06923476f),
        new DbVector3(-0.02009048f, 0.9560862f, 0.1347297f),
    };

    public static readonly ConvexHullCollider ConvexHull2 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull2Vertices
    };

    public static readonly List<DbVector3> ConvexHull3Vertices = new List<DbVector3>
    {
        new DbVector3(-0.8252932f, 1.502357f, -0.01238119f),
        new DbVector3(-0.8252932f, 1.460034f, 0.02994162f),
        new DbVector3(-0.5290335f, 1.502357f, 0.02994162f),
        new DbVector3(-0.942666f, 1.428804f, -0.11667f),
        new DbVector3(-0.8252932f, 1.375388f, -0.1393496f),
        new DbVector3(-0.9099388f, 1.333065f, 0.02994162f),
        new DbVector3(-0.964833f, 1.432866f, -0.029508f),
        new DbVector3(-0.5290335f, 1.502357f, -0.1393496f),
        new DbVector3(-0.867616f, 1.502357f, -0.0970268f),
        new DbVector3(-0.5290335f, 1.375388f, -0.1393496f),
        new DbVector3(-0.7829703f, 1.333065f, 0.02994162f),
        new DbVector3(-0.9647409f, 1.439359f, -0.030908f),
    };

    public static readonly ConvexHullCollider ConvexHull3 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull3Vertices
    };

    public static readonly List<DbVector3> ConvexHull4Vertices = new List<DbVector3>
    {
        new DbVector3(0.4410601f, 1.422778f, -0.1154961f),
        new DbVector3(-0.02445541f, 1.416616f, -0.1239446f),
        new DbVector3(0.1481313f, 1.417711f, 0.15691f),
        new DbVector3(0.4443909f, 1.375388f, -0.1393496f),
        new DbVector3(0.1058085f, 0.9521601f, 0.15691f),
        new DbVector3(-0.02495406f, 1.398794f, 0.1258374f),
        new DbVector3(0.1058085f, 1.079129f, -0.1816724f),
        new DbVector3(0.1058085f, 0.9521601f, -0.1816724f),
        new DbVector3(-0.02020947f, 0.9593424f, -0.1494396f),
        new DbVector3(0.1904541f, 0.9521601f, -0.0970268f),
        new DbVector3(0.170323f, 0.945434f, 0.06539f),
        new DbVector3(-0.02009048f, 0.9560862f, 0.1347297f),
    };

    public static readonly ConvexHullCollider ConvexHull4 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull4Vertices
    };

    public static readonly List<DbVector3> ConvexHull5Vertices = new List<DbVector3>
    {
        new DbVector3(0.02494623f, 1.3988f, 0.125821f),
        new DbVector3(-0.02134069f, 1.41651f, 0.1131405f),
        new DbVector3(-0.02445541f, 1.416616f, -0.1239446f),
        new DbVector3(-0.02115991f, 1.544679f, 0.15691f),
        new DbVector3(0.4410601f, 1.422778f, -0.1154961f),
        new DbVector3(0.06348569f, 1.756293f, -0.1393496f),
        new DbVector3(-0.02115991f, 1.840939f, -0.05470399f),
        new DbVector3(-0.02115991f, 1.840939f, 0.07226442f),
        new DbVector3(-0.02115991f, 1.756293f, 0.15691f),
        new DbVector3(0.4443909f, 1.417711f, 0.02994162f),
        new DbVector3(-0.01648364f, 1.736767f, -0.102171f),
        new DbVector3(0.1058085f, 1.417711f, 0.15691f),
    };

    public static readonly ConvexHullCollider ConvexHull5 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull5Vertices
    };

    public static readonly List<DbVector3> ConvexHull6Vertices = new List<DbVector3>
    {
        new DbVector3(-0.1058055f, 0.9521601f, 0.15691f),
        new DbVector3(-0.5281047f, 1.4209f, -0.01496139f),
        new DbVector3(-0.170323f, 0.945434f, 0.06539f),
        new DbVector3(-0.5290335f, 1.375388f, -0.1393496f),
        new DbVector3(-0.5278988f, 1.421705f, -0.1220221f),
        new DbVector3(-0.02445541f, 1.416616f, -0.1239446f),
        new DbVector3(-0.1481283f, 1.24842f, 0.15691f),
        new DbVector3(-0.1481283f, 1.417711f, 0.15691f),
        new DbVector3(-0.02495406f, 1.398794f, 0.1258374f),
        new DbVector3(-0.02020947f, 0.9593424f, -0.1494396f),
        new DbVector3(-0.02009048f, 0.9560862f, 0.1347297f),
        new DbVector3(-0.1904511f, 0.9521601f, -0.0970268f),
    };

    public static readonly ConvexHullCollider ConvexHull6 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull6Vertices
    };

    public static readonly List<DbVector3> ConvexHull7Vertices = new List<DbVector3>
    {
        new DbVector3(-0.5290335f, 1.502357f, -0.1393496f),
        new DbVector3(-0.06348271f, 1.840939f, -0.05470399f),
        new DbVector3(-0.06348271f, 1.756293f, -0.1393496f),
        new DbVector3(-0.1058055f, 1.417711f, 0.15691f),
        new DbVector3(-0.5234149f, 1.425014f, -0.008271503f),
        new DbVector3(-0.02495406f, 1.398794f, 0.1258374f),
        new DbVector3(-0.02445541f, 1.416616f, -0.1239446f),
        new DbVector3(-0.5290335f, 1.502357f, 0.02994162f),
        new DbVector3(-0.06348271f, 1.756293f, 0.15691f),
        new DbVector3(-0.02115991f, 1.840939f, 0.07226442f),
        new DbVector3(-0.02115991f, 1.756293f, 0.15691f),
        new DbVector3(-0.02115991f, 1.840939f, -0.05470399f),
    };

    public static readonly ConvexHullCollider ConvexHull7 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull7Vertices
    };

    public static readonly List<DbVector3> ConvexHull8Vertices = new List<DbVector3>
    {
        new DbVector3(-0.02009048f, 0.9560862f, 0.1347297f),
        new DbVector3(-0.03958584f, 0.7428718f, 0.06486394f),
        new DbVector3(-0.02115991f, 0.8675145f, 0.15691f),
        new DbVector3(-0.02020947f, 0.9593424f, -0.1494396f),
        new DbVector3(0.1058085f, 0.9521601f, 0.15691f),
        new DbVector3(0.02116289f, 0.528932f, 0.1145872f),
        new DbVector3(0.04723987f, 0.4778706f, -0.06923182f),
        new DbVector3(0.1058085f, 0.9521601f, -0.1816724f),
        new DbVector3(0.1904541f, 0.4866093f, 0.07226442f),
        new DbVector3(0.2327769f, 0.9521601f, -0.01238119f),
        new DbVector3(0.1481313f, 0.7828688f, 0.15691f),
        new DbVector3(0.1904541f, 0.4866093f, -0.0970268f),
    };

    public static readonly ConvexHullCollider ConvexHull8 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull8Vertices
    };

    public static readonly List<DbVector3> ConvexHull9Vertices = new List<DbVector3>
    {
        new DbVector3(0.4418963f, 1.456158f, -0.1195171f),
        new DbVector3(0.6136822f, 1.502357f, -0.1393496f),
        new DbVector3(0.4443909f, 1.375388f, -0.1393496f),
        new DbVector3(0.867619f, 1.502357f, -0.01238119f),
        new DbVector3(0.4443909f, 1.502357f, 0.02994162f),
        new DbVector3(0.9099417f, 1.333065f, 0.02994162f),
        new DbVector3(0.9414092f, 1.430237f, -0.1175186f),
        new DbVector3(0.7829733f, 1.333065f, 0.02994162f),
        new DbVector3(0.4443909f, 1.417711f, 0.02994162f),
        new DbVector3(0.8252962f, 1.375388f, -0.1393496f),
        new DbVector3(0.4424168f, 1.402891f, -0.03330591f),
        new DbVector3(0.4437452f, 1.471876f, -0.1149074f),
    };

    public static readonly ConvexHullCollider ConvexHull9 = new ConvexHullCollider
    {
        VerticesLocal = ConvexHull9Vertices
    };

    public static readonly List<ConvexHullCollider> MagicianConvexHulls = new List<ConvexHullCollider>
    {
        ConvexHull0,
        ConvexHull1,
        ConvexHull2,
        ConvexHull3,
        ConvexHull4,
        ConvexHull5,
        ConvexHull6,
        ConvexHull7,
        ConvexHull8,
        ConvexHull9,
    };

    public static readonly ComplexCollider MagicianCollider = new ComplexCollider
    {
        ConvexHulls = MagicianConvexHulls
    };
}

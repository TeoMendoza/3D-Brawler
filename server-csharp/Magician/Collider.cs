using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static readonly List<DbVector3> ConvexHull0Vertices = new List<DbVector3>
    {
        new DbVector3(0.01997155f, 0.06249816f, 0.1991009f),
        new DbVector3(0.06228565f, 0.06249816f, 0.241415f),
        new DbVector3(0.01997155f, 0.2740687f, 0.02984449f),
        new DbVector3(0.189228f, 0.1048123f, 0.07215859f),
        new DbVector3(0.1469139f, 0.2740687f, -0.09709784f),
        new DbVector3(0.1469139f, 0.2740687f, 0.02984449f),
        new DbVector3(0.189228f, 0.06249816f, 0.1991009f),
        new DbVector3(0.189228f, 0.02018405f, -0.01246962f),
        new DbVector3(0.189228f, -0.02213005f, 0.02984449f),
        new DbVector3(0.1469139f, -0.02213005f, -0.09709784f),
        new DbVector3(0.1045998f, 0.06249816f, 0.241415f),
        new DbVector3(0.06228565f, -0.02213005f, 0.241415f),
        new DbVector3(0.189228f, -0.02213005f, 0.1991009f),
        new DbVector3(0.01997155f, 0.2740687f, -0.09709784f),
        new DbVector3(0.01997155f, -0.02213005f, 0.1991009f),
        new DbVector3(0.01997155f, -0.02213005f, -0.09709784f),
    };

    public static readonly ConvexHullCollider ConvexHull0 = new() { VerticesLocal = ConvexHull0Vertices };

    public static readonly List<DbVector3> ConvexHull1Vertices = new List<DbVector3>
    {
        new DbVector3(-0.02213457f, 0.9563854f, 0.1333362f),
        new DbVector3(-0.0404154f, 0.7436866f, 0.06605743f),
        new DbVector3(-0.02234256f, 0.8664662f, 0.1567868f),
        new DbVector3(-0.02317904f, 0.9607698f, -0.1491869f),
        new DbVector3(0.1045998f, 0.9510944f, 0.1567868f),
        new DbVector3(-0.03583865f, 0.7443234f, -0.05248984f),
        new DbVector3(0.06228565f, 0.7395239f, -0.1394119f),
        new DbVector3(-0.02234256f, 0.9087803f, -0.1817261f),
        new DbVector3(0.1045998f, 0.9510944f, -0.1817261f),
        new DbVector3(0.189228f, 0.7395239f, -0.09709784f),
        new DbVector3(0.2315421f, 0.7818379f, 0.07215859f),
        new DbVector3(0.1469139f, 0.7818379f, 0.1567868f),
        new DbVector3(0.2315421f, 0.9510944f, -0.01246962f),
        new DbVector3(0.06228565f, 0.7818379f, 0.1567868f),
        new DbVector3(0.2315421f, 0.9087803f, 0.07215859f),
        new DbVector3(0.1045998f, 0.9087803f, -0.1817261f),
    };

    public static readonly ConvexHullCollider ConvexHull1 = new() { VerticesLocal = ConvexHull1Vertices };

    public static readonly List<DbVector3> ConvexHull2Vertices = new List<DbVector3>
    {
        new DbVector3(0.6969972f, 1.374236f, -0.1394119f),
        new DbVector3(0.7078682f, 1.411397f, -0.0255249f),
        new DbVector3(0.6969972f, 1.501178f, -0.01246962f),
        new DbVector3(0.7393114f, 1.374236f, 0.02984449f),
        new DbVector3(0.7393114f, 1.458864f, 0.02984449f),
        new DbVector3(0.7816255f, 1.331921f, 0.02984449f),
        new DbVector3(0.9085678f, 1.331921f, -0.01246962f),
        new DbVector3(0.9085678f, 1.41655f, 0.02984449f),
        new DbVector3(0.6946866f, 1.466137f, -0.08315822f),
        new DbVector3(0.7393114f, 1.501178f, -0.1394119f),
        new DbVector3(0.7393114f, 1.374236f, -0.1394119f),
        new DbVector3(0.943706f, 1.4281f, -0.116486f),
        new DbVector3(0.8239396f, 1.501178f, -0.1394119f),
        new DbVector3(0.963624f, 1.445238f, -0.083295f),
        new DbVector3(0.8662537f, 1.501178f, -0.01246962f),
        new DbVector3(0.8239396f, 1.458864f, 0.02984449f),
    };

    public static readonly ConvexHullCollider ConvexHull2 = new() { VerticesLocal = ConvexHull2Vertices };

    public static readonly List<DbVector3> ConvexHull3Vertices = new List<DbVector3>
    {
        new DbVector3(0.4454699f, 1.472876f, -0.1138439f),
        new DbVector3(0.4431126f, 1.501178f, 0.02984449f),
        new DbVector3(0.6123691f, 1.501178f, -0.1394119f),
        new DbVector3(0.4420763f, 1.402311f, -0.03574882f),
        new DbVector3(0.4431126f, 1.374236f, -0.1394119f),
        new DbVector3(0.4431126f, 1.41655f, 0.02984449f),
        new DbVector3(0.6969972f, 1.374236f, -0.1394119f),
        new DbVector3(0.4438515f, 1.454839f, -0.1194564f),
        new DbVector3(0.5700549f, 1.41655f, 0.02984449f),
        new DbVector3(0.6969972f, 1.501178f, -0.01246962f),
        new DbVector3(0.5700549f, 1.501178f, 0.02984449f),
        new DbVector3(0.7018626f, 1.451058f, -0.09923233f),
        new DbVector3(0.7078682f, 1.411397f, -0.0255249f),
        new DbVector3(0.6946866f, 1.466137f, -0.08315822f),
    };

    public static readonly ConvexHullCollider ConvexHull3 = new() { VerticesLocal = ConvexHull3Vertices };

    public static readonly List<DbVector3> ConvexHull4Vertices = new List<DbVector3>
    {
        new DbVector3(-0.191599f, 0.06249816f, 0.1991009f),
        new DbVector3(-0.1069708f, 0.06249816f, 0.241415f),
        new DbVector3(-0.1492849f, 0.2740687f, 0.02984449f),
        new DbVector3(-0.06465667f, 0.02018405f, -0.1394119f),
        new DbVector3(-0.02234256f, -0.02213005f, -0.09709784f),
        new DbVector3(-0.1492849f, -0.02213005f, -0.09709784f),
        new DbVector3(-0.02234256f, 0.2740687f, -0.09709784f),
        new DbVector3(-0.1492849f, 0.2740687f, -0.09709784f),
        new DbVector3(-0.191599f, 0.02018405f, -0.01246962f),
        new DbVector3(-0.06465667f, 0.06249816f, 0.241415f),
        new DbVector3(-0.1069708f, -0.02213005f, 0.241415f),
        new DbVector3(-0.02234256f, -0.02213005f, 0.1991009f),
        new DbVector3(-0.02234256f, 0.06249816f, 0.1991009f),
        new DbVector3(-0.02234256f, 0.2740687f, 0.02984449f),
        new DbVector3(-0.191599f, -0.02213005f, 0.02984449f),
        new DbVector3(-0.191599f, -0.02213005f, 0.1991009f),
    };

    public static readonly ConvexHullCollider ConvexHull4 = new() { VerticesLocal = ConvexHull4Vertices };

    public static readonly List<DbVector3> ConvexHull5Vertices = new List<DbVector3>
    {
        new DbVector3(-0.1492849f, 0.4856392f, -0.1394119f),
        new DbVector3(-0.191599f, 0.4856392f, -0.09709784f),
        new DbVector3(-0.046714f, 0.507397f, 0.05931f),
        new DbVector3(-0.06465667f, 0.4856392f, -0.1394119f),
        new DbVector3(-0.02234256f, 0.2740687f, -0.09709784f),
        new DbVector3(-0.02234256f, 0.2740687f, 0.02984449f),
        new DbVector3(-0.1492849f, 0.2740687f, 0.02984449f),
        new DbVector3(-0.1069708f, 0.3163828f, -0.1394119f),
        new DbVector3(-0.191599f, 0.401011f, 0.02984449f),
        new DbVector3(-0.191599f, 0.3586969f, -0.01246962f),
        new DbVector3(-0.1390614f, 0.4865068f, 0.05114164f),
        new DbVector3(-0.191599f, 0.3586969f, -0.09709784f),
        new DbVector3(-0.1546119f, 0.4877831f, 0.0151271f),
        new DbVector3(-0.1492849f, 0.3586969f, -0.1394119f),
        new DbVector3(-0.06465667f, 0.3163828f, -0.1394119f),
        new DbVector3(-0.04991893f, 0.4785996f, -0.06996854f),
    };

    public static readonly ConvexHullCollider ConvexHull5 = new() { VerticesLocal = ConvexHull5Vertices };

    public static readonly List<DbVector3> ConvexHull6Vertices = new List<DbVector3>
    {
        new DbVector3(-0.191599f, 0.6125815f, -0.09709784f),
        new DbVector3(-0.191599f, 0.4856392f, 0.07215859f),
        new DbVector3(-0.1636141f, 0.7554761f, 0.05345864f),
        new DbVector3(-0.1492849f, 0.7395239f, -0.1394119f),
        new DbVector3(-0.1492849f, 0.5279533f, 0.1144727f),
        new DbVector3(-0.02234256f, 0.5279533f, 0.1144727f),
        new DbVector3(-0.1277338f, 0.7497274f, 0.097329f),
        new DbVector3(-0.04991893f, 0.4785996f, -0.06996854f),
        new DbVector3(-0.04321477f, 0.742562f, -0.07227197f),
        new DbVector3(-0.0476671f, 0.7468186f, 0.08641599f),
        new DbVector3(-0.1153467f, 0.7492779f, -0.1009622f),
        new DbVector3(-0.191599f, 0.5279533f, -0.09709784f),
        new DbVector3(-0.046714f, 0.507397f, 0.05931f),
        new DbVector3(-0.1069708f, 0.6972098f, -0.1394119f),
        new DbVector3(-0.1290129f, 0.4782374f, -0.07411392f),
        new DbVector3(-0.1492849f, 0.6972098f, -0.1394119f),
    };

    public static readonly ConvexHullCollider ConvexHull6 = new() { VerticesLocal = ConvexHull6Vertices };

    public static readonly List<DbVector3> ConvexHull7Vertices = new List<DbVector3>
    {
        new DbVector3(-0.2339131f, 0.8241521f, -0.01246962f),
        new DbVector3(-0.2339131f, 0.7818379f, 0.07215859f),
        new DbVector3(-0.2339131f, 0.9087803f, 0.07215859f),
        new DbVector3(-0.191599f, 0.9510944f, -0.09709784f),
        new DbVector3(-0.1069708f, 0.9510944f, -0.1817261f),
        new DbVector3(-0.02234256f, 0.9087803f, -0.1817261f),
        new DbVector3(-0.1492849f, 0.7395239f, -0.1394119f),
        new DbVector3(-0.02317904f, 0.9607698f, -0.1491869f),
        new DbVector3(-0.06465667f, 0.7395239f, -0.1394119f),
        new DbVector3(-0.0476671f, 0.7468186f, 0.08641599f),
        new DbVector3(-0.191599f, 0.7395239f, 0.1144727f),
        new DbVector3(-0.1492849f, 0.7818379f, 0.1567868f),
        new DbVector3(-0.191599f, 0.9510944f, 0.1144727f),
        new DbVector3(-0.1069708f, 0.9510944f, 0.1567868f),
        new DbVector3(-0.02213457f, 0.9563854f, 0.1333362f),
        new DbVector3(-0.191599f, 0.7395239f, -0.09709784f),
    };

    public static readonly ConvexHullCollider ConvexHull7 = new() { VerticesLocal = ConvexHull7Vertices };

    public static readonly List<DbVector3> ConvexHull8Vertices = new List<DbVector3>
    {
        new DbVector3(-0.7839965f, 1.501178f, -0.01246962f),
        new DbVector3(-0.796371f, 1.385541f, -0.005750706f),
        new DbVector3(-0.7839965f, 1.458864f, 0.02984449f),
        new DbVector3(-0.7918186f, 1.450333f, -0.1054724f),
        new DbVector3(-0.7416824f, 1.374236f, 0.02984449f),
        new DbVector3(-0.7416824f, 1.374236f, -0.1394119f),
        new DbVector3(-0.5301118f, 1.374236f, -0.1394119f),
        new DbVector3(-0.5221913f, 1.40525f, -0.03483758f),
        new DbVector3(-0.5301118f, 1.501178f, -0.1394119f),
        new DbVector3(-0.5222124f, 1.423824f, -0.009091392f),
        new DbVector3(-0.6147401f, 1.501178f, -0.1394119f),
        new DbVector3(-0.5301118f, 1.501178f, 0.02984449f),
        new DbVector3(-0.7868684f, 1.427852f, -0.1091047f),
        new DbVector3(-0.572426f, 1.501178f, 0.02984449f),
        new DbVector3(-0.7830344f, 1.400825f, -0.07551508f),
    };

    public static readonly ConvexHullCollider ConvexHull8 = new() { VerticesLocal = ConvexHull8Vertices };

    public static readonly List<DbVector3> ConvexHull9Vertices = new List<DbVector3>
    {
        new DbVector3(0.047388f, 0.5067019f, 0.060436f),
        new DbVector3(0.1469139f, 0.4856392f, -0.1394119f),
        new DbVector3(0.06228565f, 0.4856392f, -0.1394119f),
        new DbVector3(0.01997155f, 0.3586969f, 0.07215859f),
        new DbVector3(0.01997155f, 0.2740687f, -0.09709784f),
        new DbVector3(0.01997155f, 0.2740687f, 0.02984449f),
        new DbVector3(0.189228f, 0.4856392f, 0.07215859f),
        new DbVector3(0.04636767f, 0.4753826f, -0.06834235f),
        new DbVector3(0.06228565f, 0.3163828f, -0.1394119f),
        new DbVector3(0.1045998f, 0.3163828f, -0.1394119f),
        new DbVector3(0.1469139f, 0.3163828f, 0.07215859f),
        new DbVector3(0.189228f, 0.4856392f, -0.09709784f),
        new DbVector3(0.1469139f, 0.3586969f, -0.1394119f),
        new DbVector3(0.189228f, 0.3163828f, -0.05478373f),
        new DbVector3(0.1469139f, 0.2740687f, 0.02984449f),
        new DbVector3(0.189228f, 0.4433251f, 0.07215859f),
    };

    public static readonly ConvexHullCollider ConvexHull9 = new() { VerticesLocal = ConvexHull9Vertices };

    public static readonly List<DbVector3> ConvexHull10Vertices = new List<DbVector3>
    {
        new DbVector3(-0.963932f, 1.438456f, -0.026502f),
        new DbVector3(-0.8263106f, 1.458864f, 0.02984449f),
        new DbVector3(-0.8686247f, 1.501178f, -0.01246962f),
        new DbVector3(-0.9631861f, 1.444634f, -0.08405211f),
        new DbVector3(-0.963904f, 1.428483f, -0.031615f),
        new DbVector3(-0.8686247f, 1.501178f, -0.09709784f),
        new DbVector3(-0.9109388f, 1.331921f, 0.02984449f),
        new DbVector3(-0.8686247f, 1.374236f, -0.09709784f),
        new DbVector3(-0.7839965f, 1.331921f, -0.01246962f),
        new DbVector3(-0.8263106f, 1.501178f, -0.1394119f),
        new DbVector3(-0.7839965f, 1.501178f, -0.1394119f),
        new DbVector3(-0.7839965f, 1.331921f, 0.02984449f),
        new DbVector3(-0.7868684f, 1.427852f, -0.1091047f),
        new DbVector3(-0.7830344f, 1.400825f, -0.07551508f),
        new DbVector3(-0.7839965f, 1.501178f, -0.01246962f),
        new DbVector3(-0.7839965f, 1.458864f, 0.02984449f),
    };

    public static readonly ConvexHullCollider ConvexHull10 = new() { VerticesLocal = ConvexHull10Vertices };

    public static readonly List<DbVector3> ConvexHull11Vertices = new List<DbVector3>
    {
        new DbVector3(0.1769524f, 1.419047f, 0.02719171f),
        new DbVector3(0.1926468f, 1.413451f, -0.1190666f),
        new DbVector3(0.1632372f, 1.345259f, 0.0305505f),
        new DbVector3(0.2738562f, 1.331921f, -0.09709784f),
        new DbVector3(0.4431126f, 1.374236f, -0.1394119f),
        new DbVector3(0.3161703f, 1.41655f, 0.02984449f),
        new DbVector3(0.4431157f, 1.419943f, -0.02150733f),
        new DbVector3(0.4384066f, 1.421241f, -0.1155171f),
        new DbVector3(0.19376f, 1.363916f, -0.072048f),
        new DbVector3(0.2315421f, 1.331921f, 0.02984449f),
        new DbVector3(0.1947935f, 1.397503f, -0.1100842f),
        new DbVector3(0.4420763f, 1.402311f, -0.03574882f),
        new DbVector3(0.2738562f, 1.331921f, -0.01246962f),
        new DbVector3(0.3161703f, 1.374236f, 0.02984449f),
    };

    public static readonly ConvexHullCollider ConvexHull11 = new() { VerticesLocal = ConvexHull11Vertices };

    public static readonly List<DbVector3> ConvexHull12Vertices = new List<DbVector3>
    {
        new DbVector3(0.4431126f, 1.501178f, 0.02984449f),
        new DbVector3(0.4431126f, 1.41655f, 0.02984449f),
        new DbVector3(0.4454699f, 1.472876f, -0.1138439f),
        new DbVector3(0.1790485f, 1.516149f, -0.03122984f),
        new DbVector3(0.1746438f, 1.45305f, 0.05926635f),
        new DbVector3(0.189228f, 1.501178f, 0.07215859f),
        new DbVector3(0.2315421f, 1.458864f, 0.07215859f),
        new DbVector3(0.4384066f, 1.421241f, -0.1155171f),
        new DbVector3(0.1926468f, 1.413451f, -0.1190666f),
        new DbVector3(0.1769524f, 1.419047f, 0.02719171f),
        new DbVector3(0.2315421f, 1.543492f, -0.1394119f),
        new DbVector3(0.189228f, 1.543492f, -0.1394119f),
        new DbVector3(0.2315421f, 1.543492f, -0.01246962f),
        new DbVector3(0.2315421f, 1.501178f, 0.07215859f),
    };

    public static readonly ConvexHullCollider ConvexHull12 = new() { VerticesLocal = ConvexHull12Vertices };

    public static readonly List<DbVector3> ConvexHull13Vertices = new List<DbVector3>
    {
        new DbVector3(-0.304008f, 1.406558f, -0.02188436f),
        new DbVector3(-0.5290076f, 1.420874f, -0.01558809f),
        new DbVector3(-0.5221913f, 1.40525f, -0.03483758f),
        new DbVector3(-0.5301118f, 1.374236f, -0.1394119f),
        new DbVector3(-0.5276632f, 1.420461f, -0.1215435f),
        new DbVector3(-0.3185413f, 1.374236f, -0.1394119f),
        new DbVector3(-0.317162f, 1.418693f, -0.02072533f),
        new DbVector3(-0.3105743f, 1.416145f, -0.112294f),
    };

    public static readonly ConvexHullCollider ConvexHull13 = new() { VerticesLocal = ConvexHull13Vertices };

    public static readonly List<DbVector3> ConvexHull14Vertices = new List<DbVector3>
    {
        new DbVector3(-0.533593f, 1.452247f, -0.118413f),
        new DbVector3(-0.5222124f, 1.423824f, -0.009091392f),
        new DbVector3(-0.5301118f, 1.501178f, 0.02984449f),
        new DbVector3(-0.5301118f, 1.501178f, -0.1394119f),
        new DbVector3(-0.4031695f, 1.41655f, 0.02984449f),
        new DbVector3(-0.4031695f, 1.501178f, 0.02984449f),
        new DbVector3(-0.317162f, 1.418693f, -0.02072533f),
        new DbVector3(-0.5276632f, 1.420461f, -0.1215435f),
        new DbVector3(-0.3185413f, 1.501178f, -0.1394119f),
        new DbVector3(-0.3104898f, 1.472346f, -0.03297945f),
        new DbVector3(-0.3105743f, 1.416145f, -0.112294f),
    };

    public static readonly ConvexHullCollider ConvexHull14 = new() { VerticesLocal = ConvexHull14Vertices };

    public static readonly List<DbVector3> ConvexHull15Vertices = new List<DbVector3>
    {
        new DbVector3(-0.01936436f, 1.17709f, 0.1365522f),
        new DbVector3(0.1045998f, 1.204979f, 0.1567868f),
        new DbVector3(-0.01825252f, 1.229204f, -0.1078547f),
        new DbVector3(-0.01538465f, 1.056499f, -0.1496923f),
        new DbVector3(0.1045998f, 1.078037f, -0.1817261f),
        new DbVector3(-0.02317904f, 0.9607698f, -0.1491869f),
        new DbVector3(0.189228f, 0.9510944f, -0.09709784f),
        new DbVector3(0.1469139f, 1.204979f, -0.09709784f),
        new DbVector3(0.143878f, 0.937466f, 0.09171f),
        new DbVector3(-0.02213457f, 0.9563854f, 0.1333362f),
        new DbVector3(0.1688264f, 0.9463532f, 0.06628199f),
        new DbVector3(0.1045998f, 0.9510944f, 0.1567868f),
        new DbVector3(0.01766967f, 1.229979f, -0.1078097f),
        new DbVector3(0.189228f, 1.120351f, 0.02984449f),
        new DbVector3(0.189228f, 1.078037f, 0.07215859f),
        new DbVector3(0.06228565f, 1.204979f, -0.1394119f),
    };

    public static readonly ConvexHullCollider ConvexHull15 = new() { VerticesLocal = ConvexHull15Vertices };

    public static readonly List<DbVector3> ConvexHull16Vertices = new List<DbVector3>
    {
        new DbVector3(0.1469139f, 1.41655f, 0.1567868f),
        new DbVector3(0.1926468f, 1.413451f, -0.1190666f),
        new DbVector3(-0.02904825f, 1.415806f, -0.1254543f),
        new DbVector3(0.1045998f, 1.204979f, -0.1394119f),
        new DbVector3(-0.02454388f, 1.399567f, 0.1249986f),
        new DbVector3(-0.01936436f, 1.17709f, 0.1365522f),
        new DbVector3(0.1469139f, 1.247293f, 0.1567868f),
        new DbVector3(-0.01825252f, 1.229204f, -0.1078547f),
        new DbVector3(0.05147345f, 1.17946f, 0.1281876f),
        new DbVector3(0.1469139f, 1.204979f, 0.1144727f),
        new DbVector3(-0.02760206f, 1.379664f, -0.1240691f),
        new DbVector3(0.1469139f, 1.204979f, -0.09709784f),
        new DbVector3(0.189228f, 1.289607f, 0.1144727f),
        new DbVector3(0.1947935f, 1.397503f, -0.1100842f),
        new DbVector3(0.189228f, 1.289607f, -0.05478373f),
        new DbVector3(0.1594943f, 1.411072f, 0.08980279f),
    };

    public static readonly ConvexHullCollider ConvexHull16 = new() { VerticesLocal = ConvexHull16Vertices };

    public static readonly List<DbVector3> ConvexHull17Vertices = new List<DbVector3>
    {
        new DbVector3(0.1045998f, 1.670434f, 0.1144727f),
        new DbVector3(0.06228565f, 1.670434f, 0.1567868f),
        new DbVector3(0.189228f, 1.458864f, 0.1144727f),
        new DbVector3(0.189228f, 1.543492f, -0.1394119f),
        new DbVector3(0.07439744f, 1.674162f, -0.08103697f),
        new DbVector3(0.1926468f, 1.413451f, -0.1190666f),
        new DbVector3(0.1045998f, 1.41655f, 0.1567868f),
        new DbVector3(0.0388422f, 1.674139f, -0.1059283f),
        new DbVector3(-0.02104927f, 1.672756f, -0.1146445f),
        new DbVector3(-0.02234256f, 1.543492f, 0.1567868f),
        new DbVector3(-0.01764694f, 1.669211f, 0.1267787f),
        new DbVector3(-0.02904825f, 1.415806f, -0.1254543f),
        new DbVector3(-0.0223748f, 1.416464f, 0.1143866f),
        new DbVector3(0.1045998f, 1.458864f, 0.1567868f),
        new DbVector3(0.189228f, 1.543492f, 0.02984449f),
        new DbVector3(0.0621684f, 1.416654f, -0.1321404f),
    };

    public static readonly ConvexHullCollider ConvexHull17 = new() { VerticesLocal = ConvexHull17Vertices };

    public static readonly List<DbVector3> ConvexHull18Vertices = new List<DbVector3>
    {
        new DbVector3(-0.02234256f, 1.755062f, 0.1567868f),
        new DbVector3(-0.02104927f, 1.672756f, -0.1146445f),
        new DbVector3(-0.01764694f, 1.669211f, 0.1267787f),
        new DbVector3(0.06228565f, 1.670434f, 0.1567868f),
        new DbVector3(-0.02094338f, 1.733772f, -0.1035151f),
        new DbVector3(-0.02234256f, 1.839691f, -0.05478373f),
        new DbVector3(0.06228565f, 1.755062f, -0.1394119f),
        new DbVector3(0.1045998f, 1.755062f, -0.09709784f),
        new DbVector3(0.1045998f, 1.670434f, 0.1144727f),
        new DbVector3(0.07439744f, 1.674162f, -0.08103697f),
        new DbVector3(0.06228565f, 1.839691f, 0.07215859f),
        new DbVector3(0.0388422f, 1.674139f, -0.1059283f),
        new DbVector3(-0.02234256f, 1.839691f, 0.07215859f),
        new DbVector3(0.06228565f, 1.755062f, 0.1567868f),
        new DbVector3(0.1045998f, 1.797377f, 0.07215859f),
        new DbVector3(-0.02246002f, 1.802814f, 0.02777428f),
    };

    public static readonly ConvexHullCollider ConvexHull18 = new() { VerticesLocal = ConvexHull18Vertices };

    public static readonly List<DbVector3> ConvexHull19Vertices = new List<DbVector3>
    {
        new DbVector3(-0.191599f, 1.078037f, 0.07215859f),
        new DbVector3(-0.191599f, 0.9510944f, -0.09709784f),
        new DbVector3(-0.1692768f, 0.9442855f, 0.06593363f),
        new DbVector3(-0.1069708f, 0.9510944f, 0.1567868f),
        new DbVector3(-0.1492849f, 1.204979f, -0.09709784f),
        new DbVector3(-0.01825252f, 1.229204f, -0.1078547f),
        new DbVector3(-0.1069708f, 1.078037f, -0.1817261f),
        new DbVector3(-0.191599f, 1.078037f, -0.09709784f),
        new DbVector3(-0.191599f, 1.120351f, -0.05478373f),
        new DbVector3(-0.01936436f, 1.17709f, 0.1365522f),
        new DbVector3(-0.1069708f, 1.204979f, 0.1567868f),
        new DbVector3(-0.1492849f, 1.204979f, 0.1144727f),
        new DbVector3(-0.1069708f, 0.9510944f, -0.1817261f),
        new DbVector3(-0.02317904f, 0.9607698f, -0.1491869f),
        new DbVector3(-0.02213457f, 0.9563854f, 0.1333362f),
        new DbVector3(-0.01538465f, 1.056499f, -0.1496923f),
    };

    public static readonly ConvexHullCollider ConvexHull19 = new() { VerticesLocal = ConvexHull19Vertices };

    public static readonly List<DbVector3> ConvexHull20Vertices = new List<DbVector3>
    {
        new DbVector3(-0.1492849f, 1.247293f, 0.1567868f),
        new DbVector3(-0.01936436f, 1.17709f, 0.1365522f),
        new DbVector3(-0.1492849f, 1.41655f, 0.1567868f),
        new DbVector3(-0.02454388f, 1.399567f, 0.1249986f),
        new DbVector3(-0.02904825f, 1.415806f, -0.1254543f),
        new DbVector3(-0.3185413f, 1.374236f, -0.1394119f),
        new DbVector3(-0.3185413f, 1.374236f, 0.02984449f),
        new DbVector3(-0.3185413f, 1.41655f, 0.02984449f),
        new DbVector3(-0.3105743f, 1.416145f, -0.112294f),
        new DbVector3(-0.1069708f, 1.204979f, -0.1394119f),
        new DbVector3(-0.01825252f, 1.229204f, -0.1078547f),
        new DbVector3(-0.1492849f, 1.204979f, 0.1144727f),
        new DbVector3(-0.05552931f, 1.177248f, 0.1290796f),
        new DbVector3(-0.1242127f, 1.20216f, -0.03407644f),
    };

    public static readonly ConvexHullCollider ConvexHull20 = new() { VerticesLocal = ConvexHull20Vertices };

    public static readonly List<DbVector3> ConvexHull21Vertices = new List<DbVector3>
    {
        new DbVector3(0.04825766f, 0.7481061f, 0.08484219f),
        new DbVector3(0.01997155f, 0.6548957f, -0.09709784f),
        new DbVector3(0.01997155f, 0.5279533f, 0.1144727f),
        new DbVector3(0.04119311f, 0.7462534f, -0.07044668f),
        new DbVector3(0.1292262f, 0.750118f, 0.09948169f),
        new DbVector3(0.04636767f, 0.4753826f, -0.06834235f),
        new DbVector3(0.1045998f, 0.6972098f, -0.1394119f),
        new DbVector3(0.1469139f, 0.7395239f, -0.1394119f),
        new DbVector3(0.1469139f, 0.5279533f, 0.1144727f),
        new DbVector3(0.189228f, 0.4856392f, 0.07215859f),
        new DbVector3(0.1625478f, 0.754699f, 0.0542422f),
        new DbVector3(0.189228f, 0.7395239f, -0.09709784f),
        new DbVector3(0.047388f, 0.5067019f, 0.060436f),
        new DbVector3(0.189228f, 0.4856392f, -0.09709784f),
        new DbVector3(0.1132295f, 0.7492109f, -0.1005624f),
        new DbVector3(0.1469139f, 0.6972098f, -0.1394119f),
    };

    public static readonly ConvexHullCollider ConvexHull21 = new() { VerticesLocal = ConvexHull21Vertices };

    public static readonly List<DbVector3> ConvexHull22Vertices = new List<DbVector3>
    {
        new DbVector3(-0.3185413f, 1.501178f, -0.1394119f),
        new DbVector3(-0.3185413f, 1.41655f, 0.02984449f),
        new DbVector3(-0.3185413f, 1.501178f, 0.02984449f),
        new DbVector3(-0.07701233f, 1.677141f, 0.05618795f),
        new DbVector3(-0.06465667f, 1.670434f, 0.1567868f),
        new DbVector3(-0.191599f, 1.458864f, 0.1144727f),
        new DbVector3(-0.01764694f, 1.669211f, 0.1267787f),
        new DbVector3(-0.02454388f, 1.399567f, 0.1249986f),
        new DbVector3(-0.02904825f, 1.415806f, -0.1254543f),
        new DbVector3(-0.06465667f, 1.670434f, -0.1394119f),
        new DbVector3(-0.07477675f, 1.670552f, -0.0835532f),
        new DbVector3(-0.1069708f, 1.41655f, 0.1567868f),
        new DbVector3(-0.02104927f, 1.672756f, -0.1146445f),
        new DbVector3(-0.3105743f, 1.416145f, -0.112294f),
        new DbVector3(-0.1069708f, 1.458864f, 0.1567868f),
    };

    public static readonly ConvexHullCollider ConvexHull22 = new() { VerticesLocal = ConvexHull22Vertices };

    public static readonly List<DbVector3> ConvexHull23Vertices = new List<DbVector3>
    {
        new DbVector3(-0.06465667f, 1.670434f, -0.1394119f),
        new DbVector3(-0.1069708f, 1.755062f, -0.09709784f),
        new DbVector3(-0.06465667f, 1.755062f, -0.1394119f),
        new DbVector3(-0.02234256f, 1.755062f, 0.1567868f),
        new DbVector3(-0.01764694f, 1.669211f, 0.1267787f),
        new DbVector3(-0.02234256f, 1.839691f, 0.07215859f),
        new DbVector3(-0.02234256f, 1.839691f, -0.05478373f),
        new DbVector3(-0.02104927f, 1.672756f, -0.1146445f),
        new DbVector3(-0.06465667f, 1.755062f, 0.1567868f),
        new DbVector3(-0.06465667f, 1.670434f, 0.1567868f),
        new DbVector3(-0.07477675f, 1.670552f, -0.0835532f),
        new DbVector3(-0.1069708f, 1.670434f, 0.1144727f),
        new DbVector3(-0.06465667f, 1.839691f, -0.05478373f),
        new DbVector3(-0.1069708f, 1.797377f, 0.07215859f),
        new DbVector3(-0.06465667f, 1.839691f, 0.07215859f),
        new DbVector3(-0.1069708f, 1.755062f, 0.1144727f),
    };

    public static readonly ConvexHullCollider ConvexHull23 = new() { VerticesLocal = ConvexHull23Vertices };

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
        ConvexHull10,
        ConvexHull11,
        ConvexHull12,
        ConvexHull13,
        ConvexHull14,
        ConvexHull15,
        ConvexHull16,
        ConvexHull17,
        ConvexHull18,
        ConvexHull19,
        ConvexHull20,
        ConvexHull21,
        ConvexHull22,
        ConvexHull23,
    };
    public static readonly ComplexCollider MagicianCollider = new ComplexCollider {ConvexHulls = MagicianConvexHulls};

}
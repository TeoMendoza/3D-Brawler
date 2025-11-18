using System.Numerics;
using System.Collections.Generic;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public partial struct ConvexHullCollider(List<DbVector3> verticesLocal, float margin)
    {
        public List<DbVector3> VerticesLocal = verticesLocal;
        public float Margin = margin;
    }

    public struct GjkVertex(DbVector3 SupportPointA, DbVector3 SupportPointB)
    {
        public DbVector3 SupportPointA = SupportPointA;
        public DbVector3 SupportPointB = SupportPointB;
        public DbVector3 MinkowskiPoint = Sub(SupportPointA, SupportPointB);
    }

    public struct GjkResult(bool Intersects, List<GjkVertex> Simplex, DbVector3 LastDirection)
    {
        public bool Intersects = Intersects;
        public List<GjkVertex> Simplex = Simplex;
        public DbVector3 LastDirection = LastDirection;
    }

    public static bool SolveGjk(ConvexHullCollider ColliderA, DbVector3 PositionA, float YawRadiansA, ConvexHullCollider ColliderB, DbVector3 PositionB, float YawRadiansB, out GjkResult Result, int MaxIterations = 32)
    {
        List<GjkVertex> Simplex = new List<GjkVertex>(4);
        DbVector3 SearchDirection = new(0f, 0f, 1f);

        GjkVertex InitialVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
        float InitialDot = Dot(InitialVertex.MinkowskiPoint, SearchDirection);

        if (InitialDot <= 0f)
        {
            Result = new GjkResult
            {
                Intersects = false,
                Simplex = Simplex,
                LastDirection = SearchDirection
            };
            return false;
        }

        Simplex.Add(InitialVertex);
        SearchDirection = Negate(InitialVertex.MinkowskiPoint);

        for (int IterationIndex = 0; IterationIndex < MaxIterations; IterationIndex++)
        {
            GjkVertex SupportVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
            float SupportDot = Dot(SupportVertex.MinkowskiPoint, SearchDirection);

            if (SupportDot <= 0f)
            {
                Result = new GjkResult
                {
                    Intersects = false,
                    Simplex = Simplex,
                    LastDirection = SearchDirection
                };
                return false;
            }

            Simplex.Add(SupportVertex);

            if (UpdateSimplex(ref Simplex, ref SearchDirection))
            {
                Result = new GjkResult
                {
                    Intersects = true,
                    Simplex = Simplex,
                    LastDirection = SearchDirection
                };
                return true;
            }
        }

        Result = new GjkResult
        {
            Intersects = false,
            Simplex = Simplex,
            LastDirection = SearchDirection
        };
        return false;
    }


    static GjkVertex SupportPairWorld(ConvexHullCollider ColliderA, DbVector3 PositionA, float YawRadiansA, ConvexHullCollider ColliderB, DbVector3 PositionB, float YawRadiansB, DbVector3 DirectionWorld)
    {
        DbVector3 SupportPointAWorld = SupportWorld(ColliderA, PositionA, YawRadiansA, DirectionWorld);
        DbVector3 SupportPointBWorld = SupportWorld(ColliderB, PositionB, YawRadiansB, Negate(DirectionWorld));
        return new GjkVertex(SupportPointAWorld, SupportPointBWorld);
    }

    static DbVector3 SupportWorld(ConvexHullCollider Collider, DbVector3 WorldPosition, float YawRadians, DbVector3 DirectionWorld)
    {
        DbVector3 DirectionLocal = RotateAroundYAxis(DirectionWorld, -YawRadians);
        DbVector3 SupportLocalPoint = SupportLocal(Collider, DirectionLocal);
        DbVector3 SupportWorldRotated = RotateAroundYAxis(SupportLocalPoint, YawRadians);
        return Add(SupportWorldRotated, WorldPosition);
    }

    static DbVector3 SupportLocal(ConvexHullCollider Collider, DbVector3 Direction)
    {
        List<DbVector3> Vertices = Collider.VerticesLocal;

        int BestVertexIndex = 0;
        float BestDotProduct = Dot(Vertices[0], Direction);

        for (int VertexIndex = 1; VertexIndex < Vertices.Count; VertexIndex++)
        {
            float DotProduct = Dot(Vertices[VertexIndex], Direction);
            if (DotProduct > BestDotProduct)
            {
                BestDotProduct = DotProduct;
                BestVertexIndex = VertexIndex;
            }
        }

        DbVector3 BestVertex = Vertices[BestVertexIndex];

        if (Collider.Margin > 0f)
        {
            DbVector3 NormalizedDirection = Normalize(Direction);
            return new DbVector3(
                BestVertex.x + NormalizedDirection.x * Collider.Margin,
                BestVertex.y + NormalizedDirection.y * Collider.Margin,
                BestVertex.z + NormalizedDirection.z * Collider.Margin
            );
        }

        return BestVertex;
    }

    static DbVector3 RotateAroundYAxis(DbVector3 Vector, float YawRadians)
    {
        float CosYaw = MathF.Cos(YawRadians);
        float SinYaw = MathF.Sin(YawRadians);

        float RotatedX = Vector.x * CosYaw + Vector.z * SinYaw;
        float RotatedZ = -Vector.x * SinYaw + Vector.z * CosYaw;

        return new DbVector3(RotatedX, Vector.y, RotatedZ);
    }

    static bool UpdateSimplex(ref List<GjkVertex> Simplex, ref DbVector3 SearchDirection)
    {
        int SimplexCount = Simplex.Count;

        if (SimplexCount == 2)
        {
            DbVector3 PointA = Simplex[1].MinkowskiPoint;
            DbVector3 PointB = Simplex[0].MinkowskiPoint;

            DbVector3 SegmentBA = Sub(PointB, PointA);
            DbVector3 VectorToOriginFromA = Negate(PointA);

            float DotSegmentWithAO = Dot(SegmentBA, VectorToOriginFromA);

            if (DotSegmentWithAO <= 0f)
            {
                Simplex.RemoveAt(0);
                SearchDirection = VectorToOriginFromA;
                return false;
            }

            DbVector3 NewDirection = TripleCross(SegmentBA, VectorToOriginFromA, SegmentBA);

            if (NearZero(NewDirection))
            {
                NewDirection = Perp(SegmentBA);
            }

            SearchDirection = NewDirection;
            return false;
        }

        if (SimplexCount == 3)
        {
            DbVector3 PointA = Simplex[2].MinkowskiPoint;
            DbVector3 PointB = Simplex[1].MinkowskiPoint;
            DbVector3 PointC = Simplex[0].MinkowskiPoint;

            DbVector3 EdgeAB = Sub(PointB, PointA);
            DbVector3 EdgeAC = Sub(PointC, PointA);
            DbVector3 VectorToOriginFromA = Negate(PointA);
            DbVector3 TriangleNormalABC = Cross(EdgeAB, EdgeAC);

            float DotAB_AO = Dot(EdgeAB, VectorToOriginFromA);
            float DotAC_AO = Dot(EdgeAC, VectorToOriginFromA);

            if (DotAB_AO <= 0f && DotAC_AO <= 0f)
            {
                Simplex.Clear();
                Simplex.Add(Simplex[2]);
                SearchDirection = VectorToOriginFromA;
                return false;
            }

            if (DotAB_AO > 0f)
            {
                DbVector3 PerpendicularTowardAB = TripleCross(EdgeAC, EdgeAB, EdgeAB);
                float DotABRegion = Dot(PerpendicularTowardAB, VectorToOriginFromA);

                if (DotABRegion > 0f)
                {
                    Simplex.RemoveAt(0);
                    DbVector3 NewDirection = PerpendicularTowardAB;

                    if (NearZero(NewDirection))
                    {
                        NewDirection = Perp(EdgeAB);
                    }

                    SearchDirection = NewDirection;
                    return false;
                }
            }

            if (DotAC_AO > 0f)
            {
                DbVector3 PerpendicularTowardAC = TripleCross(EdgeAB, EdgeAC, EdgeAC);
                float DotACRegion = Dot(PerpendicularTowardAC, VectorToOriginFromA);

                if (DotACRegion > 0f)
                {
                    Simplex.RemoveAt(1);
                    DbVector3 NewDirection = PerpendicularTowardAC;

                    if (NearZero(NewDirection))
                    {
                        NewDirection = Perp(EdgeAC);
                    }

                    SearchDirection = NewDirection;
                    return false;
                }
            }

            DbVector3 OrientedNormal = TriangleNormalABC;

            if (Dot(OrientedNormal, VectorToOriginFromA) < 0f)
            {
                OrientedNormal = Negate(OrientedNormal);
            }

            SearchDirection = OrientedNormal;
            return false;
        }

        if (SimplexCount == 4)
        {
            DbVector3 PointA = Simplex[3].MinkowskiPoint;
            DbVector3 PointB = Simplex[2].MinkowskiPoint;
            DbVector3 PointC = Simplex[1].MinkowskiPoint;
            DbVector3 PointD = Simplex[0].MinkowskiPoint;

            DbVector3 VectorToOriginFromA = Negate(PointA);
            DbVector3 EdgeAB = Sub(PointB, PointA);
            DbVector3 EdgeAC = Sub(PointC, PointA);
            DbVector3 EdgeAD = Sub(PointD, PointA);

            DbVector3 FaceNormalABC = Cross(EdgeAB, EdgeAC);
            DbVector3 FaceNormalACD = Cross(EdgeAC, EdgeAD);
            DbVector3 FaceNormalADB = Cross(EdgeAD, EdgeAB);

            float DotABC = Dot(FaceNormalABC, VectorToOriginFromA);
            float DotACD = Dot(FaceNormalACD, VectorToOriginFromA);
            float DotADB = Dot(FaceNormalADB, VectorToOriginFromA);

            if (DotABC > 0f)
            {
                Simplex.RemoveAt(0);
                SearchDirection = FaceNormalABC;
                return false;
            }

            if (DotACD > 0f)
            {
                Simplex.RemoveAt(2);
                SearchDirection = FaceNormalACD;
                return false;
            }

            if (DotADB > 0f)
            {
                Simplex.RemoveAt(1);
                SearchDirection = FaceNormalADB;
                return false;
            }

            return true;
        }

        DbVector3 PointSingle = Simplex[0].MinkowskiPoint;
        SearchDirection = Negate(PointSingle);
        return false;
    }

    static DbVector3 Negate(DbVector3 Vector)
    {
        return new DbVector3(-Vector.x, -Vector.y, -Vector.z);
    }

    static DbVector3 TripleCross(DbVector3 VectorA, DbVector3 VectorB, DbVector3 VectorC)
    {
        return Cross(Cross(VectorA, VectorB), VectorC);
    }

    static bool NearZero(DbVector3 Vector)
    {
        float MagnitudeSquared = Dot(Vector, Vector);
        return MagnitudeSquared <= 1e-12f;
    }

    static DbVector3 Perp(DbVector3 Vector)
    {
        if (MathF.Abs(Vector.x) > MathF.Abs(Vector.z))
        {
            return new DbVector3(-Vector.y, Vector.x, 0f);
        }

        return new DbVector3(0f, -Vector.z, Vector.y);
    }

    public static readonly List<DbVector3> PlayerConvexHullVerticesLocal = new List<DbVector3>
    {
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(-0.08126362f, 0.010119f, 0.2013561f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(-0.08126362f, 0.010119f, 0.2013561f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(-0.08126362f, 0.010119f, 0.2013561f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
    };

    public static readonly ConvexHullCollider MagicianColliderTemplate = new() { VerticesLocal = PlayerConvexHullVerticesLocal, Margin = 0f };



   public struct ContactEPA(DbVector3 Normal)
{
    public DbVector3 Normal = Normal; // Object B -> A
}

public struct EpaFace
{
    public int IndexA;
    public int IndexB;
    public int IndexC;
    public DbVector3 Normal;
    public float Distance;
    public bool Obsolete;
}

public struct EpaEdge
{
    public int IndexA;
    public int IndexB;
    public bool Obsolete;
}

public static bool EpaSolve(GjkResult Gjk, ConvexHullCollider ColliderA, DbVector3 PositionA, float YawRadiansA, ConvexHullCollider ColliderB, DbVector3 PositionB, float YawRadiansB, out ContactEPA Contact)
{
    const int MaxIterations = 32;
    const float Epsilon = 1e-4f;

    Contact = new ContactEPA(new DbVector3(0f, 0f, 0f));

    List<GjkVertex> PolytopeVertices = Gjk.Simplex;
    if (PolytopeVertices == null || PolytopeVertices.Count < 4)
    {
        return false;
    }

    List<EpaFace> Faces = new List<EpaFace>();
    AddFace(PolytopeVertices, Faces, 0, 1, 2);
    AddFace(PolytopeVertices, Faces, 0, 3, 1);
    AddFace(PolytopeVertices, Faces, 0, 2, 3);
    AddFace(PolytopeVertices, Faces, 1, 3, 2);

    for (int Iteration = 0; Iteration < MaxIterations; Iteration++)
    {
        int ClosestFaceIndex = -1;
        float ClosestDistance = float.MaxValue;

        for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
        {
            EpaFace Face = Faces[FaceIndex];
            if (Face.Obsolete)
            {
                continue;
            }

            if (Face.Distance < ClosestDistance)
            {
                ClosestDistance = Face.Distance;
                ClosestFaceIndex = FaceIndex;
            }
        }

        if (ClosestFaceIndex < 0)
        {
            return false;
        }

        EpaFace ClosestFace = Faces[ClosestFaceIndex];
        DbVector3 SearchDirection = ClosestFace.Normal;

        GjkVertex NewVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);

        float Projection = Dot(NewVertex.MinkowskiPoint, SearchDirection);
        float Improvement = Projection - ClosestFace.Distance;

        if (Improvement < Epsilon)
        {
            DbVector3 Normal = ClosestFace.Normal;
            float LengthSquared = Dot(Normal, Normal);

            if (LengthSquared > 1e-12f)
            {
                float InverseLength = 1f / Sqrt(LengthSquared);
                Normal = Mul(Normal, InverseLength);
            }
            else
            {
                Normal = NormalizeSmallVector(Gjk.LastDirection, new DbVector3(0f, 1f, 0f));
            }

            Normal.y = 0f;
            float HorizontalLengthSquared = Dot(Normal, Normal);
            if (HorizontalLengthSquared > 1e-12f)
            {
                float InverseHorizontalLength = 1f / Sqrt(HorizontalLengthSquared);
                Normal = Mul(Normal, InverseHorizontalLength);
            }

            DbVector3 RelativeBToA = Sub(PositionA, PositionB);
            if (Dot(Normal, RelativeBToA) < 0f)
            {
                Normal = Negate(Normal);
            }

            Contact = new ContactEPA(Normal);
            return true;
        }

        int NewVertexIndex = PolytopeVertices.Count;
        PolytopeVertices.Add(NewVertex);

        List<EpaEdge> Edges = new List<EpaEdge>();

        for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
        {
            EpaFace Face = Faces[FaceIndex];
            if (Face.Obsolete)
            {
                continue;
            }

            DbVector3 FacePoint = PolytopeVertices[Face.IndexA].MinkowskiPoint;
            DbVector3 ToNewPoint = Sub(NewVertex.MinkowskiPoint, FacePoint);
            float DotValue = Dot(Face.Normal, ToNewPoint);

            if (DotValue > 0f)
            {
                Face.Obsolete = true;
                Faces[FaceIndex] = Face;

                AddEdge(Edges, Face.IndexA, Face.IndexB);
                AddEdge(Edges, Face.IndexB, Face.IndexC);
                AddEdge(Edges, Face.IndexC, Face.IndexA);
            }
        }

        for (int FaceIndex = Faces.Count - 1; FaceIndex >= 0; FaceIndex--)
        {
            if (Faces[FaceIndex].Obsolete)
            {
                Faces.RemoveAt(FaceIndex);
            }
        }

        for (int EdgeIndex = 0; EdgeIndex < Edges.Count; EdgeIndex++)
        {
            EpaEdge Edge = Edges[EdgeIndex];
            if (Edge.Obsolete)
            {
                continue;
            }

            AddFace(PolytopeVertices, Faces, Edge.IndexA, Edge.IndexB, NewVertexIndex);
        }
    }

    int FinalClosestFaceIndex = -1;
    float FinalClosestDistance = float.MaxValue;

    for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
    {
        EpaFace Face = Faces[FaceIndex];
        if (Face.Obsolete)
        {
            continue;
        }

        if (Face.Distance < FinalClosestDistance)
        {
            FinalClosestDistance = Face.Distance;
            FinalClosestFaceIndex = FaceIndex;
        }
    }

    if (FinalClosestFaceIndex >= 0)
    {
        EpaFace Face = Faces[FinalClosestFaceIndex];

        DbVector3 Normal = Face.Normal;
        float LengthSquared = Dot(Normal, Normal);

        if (LengthSquared > 1e-12f)
        {
            float InverseLength = 1f / Sqrt(LengthSquared);
            Normal = Mul(Normal, InverseLength);
        }
        else
        {
            Normal = NormalizeSmallVector(Gjk.LastDirection, new DbVector3(0f, 1f, 0f));
        }

        Normal.y = 0f;
        float HorizontalLengthSquared = Dot(Normal, Normal);
        if (HorizontalLengthSquared > 1e-12f)
        {
            float InverseHorizontalLength = 1f / Sqrt(HorizontalLengthSquared);
            Normal = Mul(Normal, InverseHorizontalLength);
        }

        DbVector3 RelativeBToA = Sub(PositionA, PositionB);
        if (Dot(Normal, RelativeBToA) < 0f)
        {
            Normal = Negate(Normal);
        }

        Contact = new ContactEPA(Normal);
        return false;
    }

    return false;
}


static void AddFace(List<GjkVertex> Vertices, List<EpaFace> Faces, int IndexA, int IndexB, int IndexC)
{
    DbVector3 PointA = Vertices[IndexA].MinkowskiPoint;
    DbVector3 PointB = Vertices[IndexB].MinkowskiPoint;
    DbVector3 PointC = Vertices[IndexC].MinkowskiPoint;

    DbVector3 EdgeAB = Sub(PointB, PointA);
    DbVector3 EdgeAC = Sub(PointC, PointA);
    DbVector3 Normal = Cross(EdgeAB, EdgeAC);

    float LengthSquared = Dot(Normal, Normal);
    if (LengthSquared > 1e-12f)
    {
        float InverseLength = 1f / Sqrt(LengthSquared);
        Normal = Mul(Normal, InverseLength);
    }
    else
    {
        Normal = NormalizeSmallVector(Sub(PointB, PointC), new DbVector3(0f, 1f, 0f));
    }

    float Distance = Dot(Normal, PointA);
    if (Distance < 0f)
    {
        Normal = Negate(Normal);
        Distance = -Distance;
        (IndexC, IndexB) = (IndexB, IndexC);
    }

    EpaFace Face;
    Face.IndexA = IndexA;
    Face.IndexB = IndexB;
    Face.IndexC = IndexC;
    Face.Normal = Normal;
    Face.Distance = Distance;
    Face.Obsolete = false;

    Faces.Add(Face);
}

static void AddEdge(List<EpaEdge> Edges, int IndexA, int IndexB)
{
    for (int EdgeIndex = 0; EdgeIndex < Edges.Count; EdgeIndex++)
    {
        EpaEdge ExistingEdge = Edges[EdgeIndex];
        if (!ExistingEdge.Obsolete && ExistingEdge.IndexA == IndexB && ExistingEdge.IndexB == IndexA)
        {
            ExistingEdge.Obsolete = true;
            Edges[EdgeIndex] = ExistingEdge;
            return;
        }
    }

    EpaEdge Edge;
    Edge.IndexA = IndexA;
    Edge.IndexB = IndexB;
    Edge.Obsolete = false;
    Edges.Add(Edge);
}


static DbVector3 ComputeContactNormal(DbVector3 RawNormal, DbVector3 PositionA, DbVector3 PositionB)
{
    DbVector3 Normal = RawNormal;
    Normal.y = 0f;
    Normal = Normalize(Normal);

    DbVector3 RelativeBToA = Sub(PositionA, PositionB);
    if (Dot(Normal, RelativeBToA) < 0f) Normal = Negate(Normal);

    return Normal;
}

}

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

    public struct GjkVertex
    {
        public DbVector3 SupportPointA;
        public DbVector3 SupportPointB;
        public DbVector3 MinkowskiPoint;

        public GjkVertex(DbVector3 SupportPointA, DbVector3 SupportPointB)
        {
            this.SupportPointA = SupportPointA;
            this.SupportPointB = SupportPointB;
            MinkowskiPoint = Sub(SupportPointA, SupportPointB);
        }
    }

    public struct GjkResult
    {
        public bool Intersects;
        public List<GjkVertex> Simplex;
        public DbVector3 LastDirection;
    }

    public static bool SolveGjk(
        ConvexHullCollider ColliderA,
        DbVector3 PositionA,
        float YawRadiansA,
        ConvexHullCollider ColliderB,
        DbVector3 PositionB,
        float YawRadiansB,
        out GjkResult Result,
        int MaxIterations = 32
    )
    {
        List<GjkVertex> Simplex = new List<GjkVertex>(4);
        DbVector3 SearchDirection = new DbVector3(1f, 0f, 0f);

        GjkVertex InitialVertex = SupportPairWorld(
            ColliderA,
            PositionA,
            YawRadiansA,
            ColliderB,
            PositionB,
            YawRadiansB,
            SearchDirection
        );

        if (Dot(InitialVertex.MinkowskiPoint, SearchDirection) <= 0f)
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
            GjkVertex SupportVertex = SupportPairWorld(
                ColliderA,
                PositionA,
                YawRadiansA,
                ColliderB,
                PositionB,
                YawRadiansB,
                SearchDirection
            );

            if (Dot(SupportVertex.MinkowskiPoint, SearchDirection) <= 0f)
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

    static GjkVertex SupportPairWorld(
        ConvexHullCollider ColliderA,
        DbVector3 PositionA,
        float YawRadiansA,
        ConvexHullCollider ColliderB,
        DbVector3 PositionB,
        float YawRadiansB,
        DbVector3 DirectionWorld
    )
    {
        DbVector3 SupportPointAWorld = SupportWorld(ColliderA, PositionA, YawRadiansA, DirectionWorld);
        DbVector3 SupportPointBWorld = SupportWorld(ColliderB, PositionB, YawRadiansB, Negate(DirectionWorld));
        return new GjkVertex(SupportPointAWorld, SupportPointBWorld);
    }

    static DbVector3 SupportWorld(
        ConvexHullCollider Collider,
        DbVector3 WorldPosition,
        float YawRadians,
        DbVector3 DirectionWorld
    )
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

            DbVector3 PerpendicularTowardAB = Cross(TriangleNormalABC, EdgeAB);
            if (Dot(PerpendicularTowardAB, VectorToOriginFromA) > 0f)
            {
                Simplex.RemoveAt(0);
                DbVector3 NewDirection = TripleCross(EdgeAB, VectorToOriginFromA, EdgeAB);
                if (NearZero(NewDirection))
                {
                    NewDirection = Perp(EdgeAB);
                }
                SearchDirection = NewDirection;
                return false;
            }

            DbVector3 PerpendicularTowardAC = Cross(EdgeAC, TriangleNormalABC);
            if (Dot(PerpendicularTowardAC, VectorToOriginFromA) > 0f)
            {
                Simplex.RemoveAt(1);
                DbVector3 NewDirection = TripleCross(EdgeAC, VectorToOriginFromA, EdgeAC);
                if (NearZero(NewDirection))
                {
                    NewDirection = Perp(EdgeAC);
                }
                SearchDirection = NewDirection;
                return false;
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

            if (Dot(FaceNormalABC, VectorToOriginFromA) > 0f)
            {
                Simplex.RemoveAt(0);
                SearchDirection = FaceNormalABC;
                return false;
            }

            if (Dot(FaceNormalACD, VectorToOriginFromA) > 0f)
            {
                Simplex.RemoveAt(2);
                SearchDirection = FaceNormalACD;
                return false;
            }

            if (Dot(FaceNormalADB, VectorToOriginFromA) > 0f)
            {
                Simplex.RemoveAt(1);
                SearchDirection = FaceNormalADB;
                return false;
            }

            return true;
        }

        SearchDirection = Negate(Simplex[0].MinkowskiPoint);
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

    public static readonly ConvexHullCollider MagicianColliderTemplate =
        new ConvexHullCollider
        {
            VerticesLocal = PlayerConvexHullVerticesLocal,
            Margin = 0f
        };
}

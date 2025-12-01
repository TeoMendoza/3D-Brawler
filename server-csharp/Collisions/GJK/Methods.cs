using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static bool SolveGjk(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, out GjkResult Result, int MaxIterations = 32)
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


    static GjkVertex SupportPairWorld(List<ConvexHullCollider> ComplexColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ComplexColliderB, DbVector3 PositionB, float YawRadiansB, DbVector3 DirectionWorld)
    {
        DbVector3 SupportPointAWorld = SupportWorldComplex(ComplexColliderA, PositionA, YawRadiansA, DirectionWorld);
        DbVector3 SupportPointBWorld = SupportWorldComplex(ComplexColliderB, PositionB, YawRadiansB, Negate(DirectionWorld));
        return new GjkVertex(SupportPointAWorld, SupportPointBWorld);
    }

    static DbVector3 SupportWorldComplex(List<ConvexHullCollider> ComplexCollider, DbVector3 WorldPosition, float YawRadians, DbVector3 DirectionWorld)
    {
        DbVector3 DirectionLocal = RotateAroundYAxis(DirectionWorld, -YawRadians);
        DbVector3 SupportLocalPoint = SupportLocalComplex(ComplexCollider, DirectionLocal);
        DbVector3 SupportWorldRotated = RotateAroundYAxis(SupportLocalPoint, YawRadians);
        return Add(SupportWorldRotated, WorldPosition);
    }

    static DbVector3 SupportLocalComplex(List<ConvexHullCollider> ComplexCollider, DbVector3 DirectionLocal)
    {
        float BestDot = float.NegativeInfinity;
        DbVector3 BestPoint = new(0f, 0f, 0f);

        for (int Index = 0; Index < ComplexCollider.Count; Index++)
        {
            DbVector3 HullSupportPoint = SupportLocal(ComplexCollider[Index], DirectionLocal);
            float DotValue = Dot(HullSupportPoint, DirectionLocal);

            if (DotValue > BestDot)
            {
                BestDot = DotValue;
                BestPoint = HullSupportPoint;
            }
        }

        return BestPoint;
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

        // Margin inflation (logical skin)
        float Margin = Collider.Margin;
        if (Margin > 0f)
        {
            float DirLenSq = Dot(Direction, Direction);
            if (DirLenSq > 1e-8f)
            {
                float InvLen = 1.0f / MathF.Sqrt(DirLenSq);
                DbVector3 DirNorm = new(Direction.x * InvLen, Direction.y * InvLen, Direction.z * InvLen);

                BestVertex = new DbVector3(
                    BestVertex.x + DirNorm.x * Margin,
                    BestVertex.y + DirNorm.y * Margin,
                    BestVertex.z + DirNorm.z * Margin
                );
            }
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

    static float ComputePenetrationDepthApprox(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, DbVector3 Normal)
    {
        DbVector3 SupportA = SupportWorldComplex(ColliderA, PositionA, YawRadiansA, Negate(Normal));
        DbVector3 SupportB = SupportWorldComplex(ColliderB, PositionB, YawRadiansB, Normal);

        float DistanceA = Dot(SupportA, Normal);
        float DistanceB = Dot(SupportB, Normal);

        float Gap = DistanceB - DistanceA;
        if (Gap >= 0f) return 0f;

        return -Gap;
    }
}
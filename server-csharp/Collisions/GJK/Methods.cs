using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static bool SolveGjk(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, out GjkResult Result, int MaxIterations = 32)
    {
        List<GjkVertex> Simplex = new List<GjkVertex>(4);
        DbVector3 SearchDirection = new DbVector3(0f, 0f, 1f);

        GjkVertex InitialVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
        float InitialDot = Dot(InitialVertex.MinkowskiPoint, SearchDirection);

        if (InitialDot <= 0f)
        {
            DbVector3 ClosestPointA;
            DbVector3 ClosestPointB;

            ComputeWitnessPointsFromSimplex(Simplex, out ClosestPointA, out ClosestPointB);

            Result = new GjkResult
            {
                Intersects = false,
                Simplex = Simplex,
                LastDirection = Negate(SearchDirection),
                ClosestPointA = ClosestPointA,
                ClosestPointB = ClosestPointB
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
                DbVector3 ClosestPointA;
                DbVector3 ClosestPointB;

                ComputeWitnessPointsFromSimplex(Simplex, out ClosestPointA, out ClosestPointB);

                Result = new GjkResult
                {
                    Intersects = false,
                    Simplex = Simplex,
                    LastDirection = Negate(SearchDirection),
                    ClosestPointA = ClosestPointA,
                    ClosestPointB = ClosestPointB
                };
                return false;
            }

            Simplex.Add(SupportVertex);

            if (UpdateSimplex(ref Simplex, ref SearchDirection))
            {
                DbVector3 ClosestPointA;
                DbVector3 ClosestPointB;

                ComputeWitnessPointsFromSimplex(Simplex, out ClosestPointA, out ClosestPointB);

                Result = new GjkResult
                {
                    Intersects = true,
                    Simplex = Simplex,
                    LastDirection = Negate(SearchDirection),
                    ClosestPointA = ClosestPointA,
                    ClosestPointB = ClosestPointB
                };
                return true;
            }
        }

        {
            DbVector3 ClosestPointA;
            DbVector3 ClosestPointB;

            ComputeWitnessPointsFromSimplex(Simplex, out ClosestPointA, out ClosestPointB);

            Result = new GjkResult
            {
                Intersects = false,
                Simplex = Simplex,
                LastDirection = Negate(SearchDirection),
                ClosestPointA = ClosestPointA,
                ClosestPointB = ClosestPointB
            };
            return false;
        }
    }

    static void ComputeWitnessPointsFromSimplex(List<GjkVertex> Simplex, out DbVector3 ClosestPointA, out DbVector3 ClosestPointB)
    {
        int Count = Simplex.Count;

        if (Count == 0)
        {
            ClosestPointA = new DbVector3(0f, 0f, 0f);
            ClosestPointB = new DbVector3(0f, 0f, 0f);
            return;
        }

        if (Count == 1)
        {
            GjkVertex V = Simplex[0];
            ClosestPointA = V.SupportPointA;
            ClosestPointB = V.SupportPointB;
            return;
        }

        int BestIndexI = 0;
        int BestIndexJ = 1;
        float BestDistanceSquared = float.PositiveInfinity;

        for (int I = 0; I < Count; I++)
        {
            DbVector3 Wi = Simplex[I].MinkowskiPoint;
            float DistanceSquaredI = Dot(Wi, Wi);

            if (DistanceSquaredI < BestDistanceSquared)
            {
                BestDistanceSquared = DistanceSquaredI;
                BestIndexI = I;
                BestIndexJ = I;
            }

            for (int J = I + 1; J < Count; J++)
            {
                DbVector3 W0 = Simplex[I].MinkowskiPoint;
                DbVector3 W1 = Simplex[J].MinkowskiPoint;

                DbVector3 Edge = Sub(W1, W0);
                float EdgeLengthSquared = Dot(Edge, Edge);

                if (EdgeLengthSquared <= 1e-12f)
                {
                    continue;
                }

                DbVector3 NegativeW0 = Negate(W0);
                float T = Dot(NegativeW0, Edge) / EdgeLengthSquared;

                if (T < 0f)
                {
                    T = 0f;
                }
                else if (T > 1f)
                {
                    T = 1f;
                }

                DbVector3 ClosestMinkowski = new DbVector3(
                    W0.x + Edge.x * T,
                    W0.y + Edge.y * T,
                    W0.z + Edge.z * T
                );

                float DistanceSquared = Dot(ClosestMinkowski, ClosestMinkowski);

                if (DistanceSquared < BestDistanceSquared)
                {
                    BestDistanceSquared = DistanceSquared;
                    BestIndexI = I;
                    BestIndexJ = J;
                }
            }
        }

        if (BestIndexI == BestIndexJ)
        {
            GjkVertex V = Simplex[BestIndexI];
            ClosestPointA = V.SupportPointA;
            ClosestPointB = V.SupportPointB;
        }
        else
        {
            GjkVertex V0 = Simplex[BestIndexI];
            GjkVertex V1 = Simplex[BestIndexJ];

            DbVector3 W0 = V0.MinkowskiPoint;
            DbVector3 W1 = V1.MinkowskiPoint;
            DbVector3 Edge = Sub(W1, W0);

            float EdgeLengthSquared = Dot(Edge, Edge);

            if (EdgeLengthSquared <= 1e-12f)
            {
                ClosestPointA = V0.SupportPointA;
                ClosestPointB = V0.SupportPointB;
                return;
            }

            DbVector3 NegativeW0 = Negate(W0);
            float T = Dot(NegativeW0, Edge) / EdgeLengthSquared;

            if (T < 0f)
            {
                T = 0f;
            }
            else if (T > 1f)
            {
                T = 1f;
            }

            DbVector3 SupportA0 = V0.SupportPointA;
            DbVector3 SupportA1 = V1.SupportPointA;
            DbVector3 SupportB0 = V0.SupportPointB;
            DbVector3 SupportB1 = V1.SupportPointB;

            ClosestPointA = new DbVector3(
                SupportA0.x + (SupportA1.x - SupportA0.x) * T,
                SupportA0.y + (SupportA1.y - SupportA0.y) * T,
                SupportA0.z + (SupportA1.z - SupportA0.z) * T
            );

            ClosestPointB = new DbVector3(
                SupportB0.x + (SupportB1.x - SupportB0.x) * T,
                SupportB0.y + (SupportB1.y - SupportB0.y) * T,
                SupportB0.z + (SupportB1.z - SupportB0.z) * T
            );
        }
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
                GjkVertex VertexA = Simplex[2];
                Simplex.Clear();
                Simplex.Add(VertexA);
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
        // A uses -Normal (Backwards), B uses +Normal (Forwards)
        DbVector3 SupportA = SupportWorldComplex(ColliderA, PositionA, YawRadiansA, Negate(Normal));
        DbVector3 SupportB = SupportWorldComplex(ColliderB, PositionB, YawRadiansB, Normal);

        float DistanceA = Dot(SupportA, Normal);
        float DistanceB = Dot(SupportB, Normal);

        float Gap = DistanceB - DistanceA;

        // FIX: If Gap is positive, we are overlapping. Return the Gap.
        // If Gap is negative, we are separated. Return 0.
        if (Gap <= 0f) return 0f;
        return Gap;
    }
}
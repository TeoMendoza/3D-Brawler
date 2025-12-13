using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public struct GjkDistanceResult
    {
        public bool Intersects;
        public float Distance;
        public DbVector3 SeparationDirection;
        public DbVector3 PointOnA;
        public DbVector3 PointOnB;
        public List<GjkVertex> Simplex;
        public DbVector3 LastDirection;
    }

    public static bool SolveGjkDistance(
        List<ConvexHullCollider> ColliderA,
        DbVector3 PositionA,
        float YawRadiansA,
        List<ConvexHullCollider> ColliderB,
        DbVector3 PositionB,
        float YawRadiansB,
        out GjkDistanceResult Result,
        int MaxIterations = 32)
    {
        List<GjkVertex> Simplex = new List<GjkVertex>(4);

        DbVector3 InitialDirection = new DbVector3(0.7f, 0.4f, 0.6f);
        DbVector3 SearchDirection = NearZero(InitialDirection) ? new DbVector3(0f, 0f, 1f) : Normalize(InitialDirection);

        GjkVertex InitialVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
        float InitialDot = Dot(InitialVertex.MinkowskiPoint, SearchDirection);

        Simplex.Add(InitialVertex);

        if (InitialDot <= 0f)
        {
            ComputeDistanceFromSimplex(
                Simplex,
                out float Distance,
                out DbVector3 PointOnA,
                out DbVector3 PointOnB,
                out DbVector3 SeparationDirection,
                out DbVector3 ClosestMinkowskiPoint);

            Result = new GjkDistanceResult
            {
                Intersects = false,
                Distance = Distance,
                SeparationDirection = SeparationDirection,
                PointOnA = PointOnA,
                PointOnB = PointOnB,
                Simplex = Simplex,
                LastDirection = SearchDirection
            };
            return true;
        }

        SearchDirection = Negate(InitialVertex.MinkowskiPoint);
        if (NearZero(SearchDirection))
        {
            SearchDirection = new DbVector3(0f, 0f, 1f);
        }

        for (int IterationIndex = 0; IterationIndex < MaxIterations; IterationIndex++)
        {
            GjkVertex SupportVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
            float SupportDot = Dot(SupportVertex.MinkowskiPoint, SearchDirection);

            if (SupportDot <= 0f)
            {
                ComputeDistanceFromSimplex(
                    Simplex,
                    out float Distance,
                    out DbVector3 PointOnA,
                    out DbVector3 PointOnB,
                    out DbVector3 SeparationDirection,
                    out DbVector3 ClosestMinkowskiPoint);

                Result = new GjkDistanceResult
                {
                    Intersects = false,
                    Distance = Distance,
                    SeparationDirection = SeparationDirection,
                    PointOnA = PointOnA,
                    PointOnB = PointOnB,
                    Simplex = Simplex,
                    LastDirection = SearchDirection
                };
                return true;
            }

            Simplex.Add(SupportVertex);

            if (UpdateSimplex(ref Simplex, ref SearchDirection))
            {
                DbVector3 SeparationDirection = NearZero(SearchDirection) ? new DbVector3(0f, 1f, 0f) : Normalize(SearchDirection);

                Result = new GjkDistanceResult
                {
                    Intersects = true,
                    Distance = 0f,
                    SeparationDirection = SeparationDirection,
                    PointOnA = SupportVertex.SupportPointA,
                    PointOnB = SupportVertex.SupportPointB,
                    Simplex = Simplex,
                    LastDirection = SearchDirection
                };
                return true;
            }

            if (NearZero(SearchDirection))
            {
                ComputeDistanceFromSimplex(
                    Simplex,
                    out float Distance,
                    out DbVector3 PointOnA,
                    out DbVector3 PointOnB,
                    out DbVector3 SeparationDirection,
                    out DbVector3 ClosestMinkowskiPoint);

                Result = new GjkDistanceResult
                {
                    Intersects = false,
                    Distance = Distance,
                    SeparationDirection = SeparationDirection,
                    PointOnA = PointOnA,
                    PointOnB = PointOnB,
                    Simplex = Simplex,
                    LastDirection = SearchDirection
                };
                return true;
            }
        }

        ComputeDistanceFromSimplex(
            Simplex,
            out float FinalDistance,
            out DbVector3 FinalPointOnA,
            out DbVector3 FinalPointOnB,
            out DbVector3 FinalSeparationDirection,
            out DbVector3 FinalClosestMinkowskiPoint);

        Result = new GjkDistanceResult
        {
            Intersects = false,
            Distance = FinalDistance,
            SeparationDirection = FinalSeparationDirection,
            PointOnA = FinalPointOnA,
            PointOnB = FinalPointOnB,
            Simplex = Simplex,
            LastDirection = SearchDirection
        };
        return true;
    }

    static void ComputeDistanceFromSimplex(
        List<GjkVertex> Simplex,
        out float Distance,
        out DbVector3 PointOnA,
        out DbVector3 PointOnB,
        out DbVector3 SeparationDirection,
        out DbVector3 ClosestMinkowskiPoint)
    {
        if (Simplex.Count == 0)
        {
            Distance = 0f;
            PointOnA = new DbVector3(0f, 0f, 0f);
            PointOnB = new DbVector3(0f, 0f, 0f);
            SeparationDirection = new DbVector3(0f, 0f, 1f);
            ClosestMinkowskiPoint = new DbVector3(0f, 0f, 0f);
            return;
        }

        if (Simplex.Count == 1)
        {
            GjkVertex A = Simplex[0];

            ClosestMinkowskiPoint = A.MinkowskiPoint;
            PointOnA = A.SupportPointA;
            PointOnB = A.SupportPointB;

            Distance = Length(ClosestMinkowskiPoint);

            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        if (Simplex.Count == 2)
        {
            ComputeDistanceFromSegment(Simplex[0], Simplex[1], out Distance, out PointOnA, out PointOnB, out SeparationDirection, out ClosestMinkowskiPoint);
            return;
        }

        if (Simplex.Count == 3)
        {
            ComputeDistanceFromTriangle(Simplex[0], Simplex[1], Simplex[2], out Distance, out PointOnA, out PointOnB, out SeparationDirection, out ClosestMinkowskiPoint);
            return;
        }

        ComputeDistanceFromTetrahedron(Simplex[0], Simplex[1], Simplex[2], Simplex[3], out Distance, out PointOnA, out PointOnB, out SeparationDirection, out ClosestMinkowskiPoint);
    }

    static void ComputeDistanceFromSegment(
        GjkVertex VertexA,
        GjkVertex VertexB,
        out float Distance,
        out DbVector3 PointOnA,
        out DbVector3 PointOnB,
        out DbVector3 SeparationDirection,
        out DbVector3 ClosestMinkowskiPoint)
    {
        DbVector3 A = VertexA.MinkowskiPoint;
        DbVector3 B = VertexB.MinkowskiPoint;
        DbVector3 AB = Sub(B, A);

        float Denominator = Dot(AB, AB);
        float T = 0f;

        if (Denominator > 1e-12f)
        {
            T = Clamp01Value(-Dot(A, AB) / Denominator);
        }

        ClosestMinkowskiPoint = Add(A, Mul(AB, T));

        PointOnA = Add(VertexA.SupportPointA, Mul(Sub(VertexB.SupportPointA, VertexA.SupportPointA), T));
        PointOnB = Add(VertexA.SupportPointB, Mul(Sub(VertexB.SupportPointB, VertexA.SupportPointB), T));

        Distance = Length(ClosestMinkowskiPoint);

        DbVector3 Direction = Negate(ClosestMinkowskiPoint);
        SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
    }

    static void ComputeDistanceFromTriangle(
        GjkVertex VertexA,
        GjkVertex VertexB,
        GjkVertex VertexC,
        out float Distance,
        out DbVector3 PointOnA,
        out DbVector3 PointOnB,
        out DbVector3 SeparationDirection,
        out DbVector3 ClosestMinkowskiPoint)
    {
        DbVector3 A = VertexA.MinkowskiPoint;
        DbVector3 B = VertexB.MinkowskiPoint;
        DbVector3 C = VertexC.MinkowskiPoint;

        DbVector3 AB = Sub(B, A);
        DbVector3 AC = Sub(C, A);
        DbVector3 AO = Negate(A);

        float D1 = Dot(AB, AO);
        float D2 = Dot(AC, AO);

        if (D1 <= 0f && D2 <= 0f)
        {
            ClosestMinkowskiPoint = A;
            PointOnA = VertexA.SupportPointA;
            PointOnB = VertexA.SupportPointB;
            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        DbVector3 BO = Negate(B);
        float D3 = Dot(AB, BO);
        float D4 = Dot(AC, BO);

        if (D3 >= 0f && D4 <= D3)
        {
            ClosestMinkowskiPoint = B;
            PointOnA = VertexB.SupportPointA;
            PointOnB = VertexB.SupportPointB;
            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        float VC = D1 * D4 - D3 * D2;
        if (VC <= 0f && D1 >= 0f && D3 <= 0f)
        {
            float V = D1 / (D1 - D3);

            ClosestMinkowskiPoint = Add(A, Mul(AB, V));

            PointOnA = Add(VertexA.SupportPointA, Mul(Sub(VertexB.SupportPointA, VertexA.SupportPointA), V));
            PointOnB = Add(VertexA.SupportPointB, Mul(Sub(VertexB.SupportPointB, VertexA.SupportPointB), V));

            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        DbVector3 CO = Negate(C);
        float D5 = Dot(AB, CO);
        float D6 = Dot(AC, CO);

        if (D6 >= 0f && D5 <= D6)
        {
            ClosestMinkowskiPoint = C;
            PointOnA = VertexC.SupportPointA;
            PointOnB = VertexC.SupportPointB;
            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        float VB = D5 * D2 - D1 * D6;
        if (VB <= 0f && D2 >= 0f && D6 <= 0f)
        {
            float W = D2 / (D2 - D6);

            ClosestMinkowskiPoint = Add(A, Mul(AC, W));

            PointOnA = Add(VertexA.SupportPointA, Mul(Sub(VertexC.SupportPointA, VertexA.SupportPointA), W));
            PointOnB = Add(VertexA.SupportPointB, Mul(Sub(VertexC.SupportPointB, VertexA.SupportPointB), W));

            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        float VA = D3 * D6 - D5 * D4;
        if (VA <= 0f && (D4 - D3) >= 0f && (D5 - D6) >= 0f)
        {
            float W = (D4 - D3) / ((D4 - D3) + (D5 - D6));

            DbVector3 BC = Sub(C, B);
            ClosestMinkowskiPoint = Add(B, Mul(BC, W));

            PointOnA = Add(VertexB.SupportPointA, Mul(Sub(VertexC.SupportPointA, VertexB.SupportPointA), W));
            PointOnB = Add(VertexB.SupportPointB, Mul(Sub(VertexC.SupportPointB, VertexB.SupportPointB), W));

            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        float Denominator = VA + VB + VC;
        if (Denominator <= 1e-12f)
        {
            ClosestMinkowskiPoint = A;
            PointOnA = VertexA.SupportPointA;
            PointOnB = VertexA.SupportPointB;
            Distance = Length(ClosestMinkowskiPoint);
            DbVector3 Direction = Negate(ClosestMinkowskiPoint);
            SeparationDirection = NearZero(Direction) ? new DbVector3(0f, 1f, 0f) : Normalize(Direction);
            return;
        }

        float InverseDenominator = 1f / Denominator;
        float VFace = VB * InverseDenominator;
        float WFace = VC * InverseDenominator;
        float UFace = 1f - VFace - WFace;

        ClosestMinkowskiPoint = Add(Add(Mul(A, UFace), Mul(B, VFace)), Mul(C, WFace));

        PointOnA = Add(Add(Mul(VertexA.SupportPointA, UFace), Mul(VertexB.SupportPointA, VFace)), Mul(VertexC.SupportPointA, WFace));
        PointOnB = Add(Add(Mul(VertexA.SupportPointB, UFace), Mul(VertexB.SupportPointB, VFace)), Mul(VertexC.SupportPointB, WFace));

        Distance = Length(ClosestMinkowskiPoint);

        DbVector3 DirectionFace = Negate(ClosestMinkowskiPoint);
        SeparationDirection = NearZero(DirectionFace) ? new DbVector3(0f, 1f, 0f) : Normalize(DirectionFace);
    }

    static void ComputeDistanceFromTetrahedron(
        GjkVertex VertexA,
        GjkVertex VertexB,
        GjkVertex VertexC,
        GjkVertex VertexD,
        out float Distance,
        out DbVector3 PointOnA,
        out DbVector3 PointOnB,
        out DbVector3 SeparationDirection,
        out DbVector3 ClosestMinkowskiPoint)
    {
        float BestDistance = float.MaxValue;

        Distance = 0f;
        PointOnA = new DbVector3(0f, 0f, 0f);
        PointOnB = new DbVector3(0f, 0f, 0f);
        SeparationDirection = new DbVector3(0f, 1f, 0f);
        ClosestMinkowskiPoint = new DbVector3(0f, 0f, 0f);

        EvaluateFace(VertexA, VertexB, VertexC, ref BestDistance, ref Distance, ref PointOnA, ref PointOnB, ref SeparationDirection, ref ClosestMinkowskiPoint);
        EvaluateFace(VertexA, VertexC, VertexD, ref BestDistance, ref Distance, ref PointOnA, ref PointOnB, ref SeparationDirection, ref ClosestMinkowskiPoint);
        EvaluateFace(VertexA, VertexD, VertexB, ref BestDistance, ref Distance, ref PointOnA, ref PointOnB, ref SeparationDirection, ref ClosestMinkowskiPoint);
        EvaluateFace(VertexB, VertexD, VertexC, ref BestDistance, ref Distance, ref PointOnA, ref PointOnB, ref SeparationDirection, ref ClosestMinkowskiPoint);
    }

    static void EvaluateFace(
        GjkVertex FaceA,
        GjkVertex FaceB,
        GjkVertex FaceC,
        ref float BestDistance,
        ref float Distance,
        ref DbVector3 PointOnA,
        ref DbVector3 PointOnB,
        ref DbVector3 SeparationDirection,
        ref DbVector3 ClosestMinkowskiPoint)
    {
        ComputeDistanceFromTriangle(
            FaceA, FaceB, FaceC,
            out float FaceDistance,
            out DbVector3 FacePointOnA,
            out DbVector3 FacePointOnB,
            out DbVector3 FaceSeparationDirection,
            out DbVector3 FaceClosestMinkowskiPoint);

        if (FaceDistance < BestDistance)
        {
            BestDistance = FaceDistance;
            Distance = FaceDistance;
            PointOnA = FacePointOnA;
            PointOnB = FacePointOnB;
            SeparationDirection = FaceSeparationDirection;
            ClosestMinkowskiPoint = FaceClosestMinkowskiPoint;
        }
    }

    static float Clamp01Value(float Value)
    {
        if (Value < 0f) return 0f;
        if (Value > 1f) return 1f;
        return Value;
    }

    static float Length(DbVector3 Value)
    {
        return MathF.Sqrt(Dot(Value, Value));
    }

// public struct GjkDistanceResult
//     {
//         public bool Intersects;
//         public float Distance;
//         public DbVector3 SeparationDirection;
//         public DbVector3 PointOnA;
//         public DbVector3 PointOnB;
//         public List<GjkVertex> Simplex;
//         public DbVector3 LastDirection;
//     }


//     public static bool SolveGjkDistance(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, out GjkDistanceResult Result, int MaxIterations = 32)
//     {
//         List<GjkVertex> Simplex = new List<GjkVertex>(4);

//         DbVector3 InitialDirection = new(0.7f, 0.4f, 0.6f);
//         DbVector3 SearchDirection = Normalize(InitialDirection);

//         GjkVertex InitialVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
//         float InitialDot = Dot(InitialVertex.MinkowskiPoint, SearchDirection);

//         if (InitialDot <= 0f)
//         {
//             ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection);

//             Result = new GjkDistanceResult
//             {
//                 Intersects = false,
//                 Distance = Distance,
//                 SeparationDirection = SeparationDirection,
//                 PointOnA = PointOnA,
//                 PointOnB = PointOnB,
//                 Simplex = Simplex,
//                 LastDirection = SearchDirection
//             };
//             return true;
//         }

//         Simplex.Add(InitialVertex);
//         SearchDirection = Negate(InitialVertex.MinkowskiPoint);
//         if (NearZero(SearchDirection))
//         {
//             SearchDirection = new DbVector3(0f, 0f, 1f);
//         }

//         for (int IterationIndex = 0; IterationIndex < MaxIterations; IterationIndex++)
//         {
//             GjkVertex SupportVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
//             float SupportDot = Dot(SupportVertex.MinkowskiPoint, SearchDirection);

//             if (SupportDot <= 0f)
//             {
//                 ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection);

//                 Result = new GjkDistanceResult
//                 {
//                     Intersects = false,
//                     Distance = Distance,
//                     SeparationDirection = SeparationDirection,
//                     PointOnA = PointOnA,
//                     PointOnB = PointOnB,
//                     Simplex = Simplex,
//                     LastDirection = SearchDirection
//                 };
//                 return true;
//             }

//             Simplex.Add(SupportVertex);

//             if (UpdateSimplex(ref Simplex, ref SearchDirection))
//             {
//                 DbVector3 SeparationDirection = NearZero(SearchDirection) ? new DbVector3(0f, 1f, 0f) : Normalize(SearchDirection);

//                 Result = new GjkDistanceResult
//                 {
//                     Intersects = true,
//                     Distance = 0f,
//                     SeparationDirection = SeparationDirection,
//                     PointOnA = SupportVertex.SupportPointA,
//                     PointOnB = SupportVertex.SupportPointB,
//                     Simplex = Simplex,
//                     LastDirection = SearchDirection
//                 };
//                 return true;
//             }

//             if (NearZero(SearchDirection))
//             {
//                 ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection);

//                 Result = new GjkDistanceResult
//                 {
//                     Intersects = false,
//                     Distance = Distance,
//                     SeparationDirection = SeparationDirection,
//                     PointOnA = PointOnA,
//                     PointOnB = PointOnB,
//                     Simplex = Simplex,
//                     LastDirection = SearchDirection
//                 };
//                 return true;
//             }
//         }

//         ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float FinalDistance, out DbVector3 FinalPointOnA, out DbVector3 FinalPointOnB, out DbVector3 FinalSeparationDirection);

//         Result = new GjkDistanceResult
//         {
//             Intersects = false,
//             Distance = FinalDistance,
//             SeparationDirection = FinalSeparationDirection,
//             PointOnA = FinalPointOnA,
//             PointOnB = FinalPointOnB,
//             Simplex = Simplex,
//             LastDirection = SearchDirection
//         };
//         return true;
//     }


//     static void ComputeSeparationOnAxis(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, DbVector3 Axis, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection)
//     {
//         DbVector3 Direction = Axis;
//         if (!NearZero(Direction))
//         {
//             Direction = Normalize(Direction);
//         }
//         else
//         {
//             Direction = new DbVector3(0f, 0f, 1f);
//         }

//         DbVector3 SupportA = SupportWorldComplex(ColliderA, PositionA, YawRadiansA, Direction);
//         DbVector3 SupportB = SupportWorldComplex(ColliderB, PositionB, YawRadiansB, Negate(Direction));

//         float DistanceA = Dot(SupportA, Direction);
//         float DistanceB = Dot(SupportB, Direction);

//         float Gap = DistanceB - DistanceA;
//         if (Gap < 0f) Gap = 0f;

//         Distance = Gap;
//         PointOnA = SupportA;
//         PointOnB = SupportB;

//         SeparationDirection = Direction;
//     }

}
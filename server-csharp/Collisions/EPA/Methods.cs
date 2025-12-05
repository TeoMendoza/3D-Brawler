using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    // public static bool EpaSolve(GjkResult Gjk, List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, out ContactEPA Contact)
    // {
    //     const int MaxIterations = 16;
    //     const float Epsilon = 1e-4f;

    //     Contact = new ContactEPA(new DbVector3(0f, 0f, 0f));

    //     List<GjkVertex> PolytopeVertices = Gjk.Simplex;
    //     if (PolytopeVertices == null || PolytopeVertices.Count < 4)
    //     {
    //         return false;
    //     }

    //     List<EpaFace> Faces = new List<EpaFace>();
    //     AddFace(PolytopeVertices, Faces, 0, 1, 2);
    //     AddFace(PolytopeVertices, Faces, 0, 3, 1);
    //     AddFace(PolytopeVertices, Faces, 0, 2, 3);
    //     AddFace(PolytopeVertices, Faces, 1, 3, 2);

    //     for (int Iteration = 0; Iteration < MaxIterations; Iteration++)
    //     {
    //         int ClosestFaceIndex = -1;
    //         float ClosestDistance = float.MaxValue;

    //         for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
    //         {
    //             EpaFace Face = Faces[FaceIndex];
    //             if (Face.Obsolete)
    //             {
    //                 continue;
    //             }

    //             if (Face.Distance < ClosestDistance)
    //             {
    //                 ClosestDistance = Face.Distance;
    //                 ClosestFaceIndex = FaceIndex;
    //             }
    //         }

    //         if (ClosestFaceIndex < 0)
    //         {
    //             return false;
    //         }

    //         EpaFace ClosestFace = Faces[ClosestFaceIndex];
    //         DbVector3 SearchDirection = ClosestFace.Normal;

    //         GjkVertex NewVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);

    //         float Projection = Dot(NewVertex.MinkowskiPoint, SearchDirection);
    //         float Improvement = Projection - ClosestFace.Distance;

    //         if (Improvement < Epsilon)
    //         {
    //             DbVector3 Normal = ClosestFace.Normal;
    //             float LengthSquared = Dot(Normal, Normal);

    //             if (LengthSquared > 1e-12f)
    //             {
    //                 float InverseLength = 1f / Sqrt(LengthSquared);
    //                 Normal = Mul(Normal, InverseLength);
    //             }
    //             else
    //             {
    //                 Normal = NormalizeSmallVector(Gjk.LastDirection, new DbVector3(0f, 1f, 0f));
    //             }

    //             Normal.y = 0f;
    //             float HorizontalLengthSquared = Dot(Normal, Normal);
    //             if (HorizontalLengthSquared > 1e-12f)
    //             {
    //                 float InverseHorizontalLength = 1f / Sqrt(HorizontalLengthSquared);
    //                 Normal = Mul(Normal, InverseHorizontalLength);
    //             }

    //             DbVector3 RelativeBToA = Sub(PositionA, PositionB);
    //             if (Dot(Normal, RelativeBToA) < 0f)
    //             {
    //                 Normal = Negate(Normal);
    //             }

    //             Contact = new ContactEPA(Normal);
    //             return true;
    //         }

    //         int NewVertexIndex = PolytopeVertices.Count;
    //         PolytopeVertices.Add(NewVertex);

    //         List<EpaEdge> Edges = new List<EpaEdge>();

    //         for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
    //         {
    //             EpaFace Face = Faces[FaceIndex];
    //             if (Face.Obsolete)
    //             {
    //                 continue;
    //             }

    //             DbVector3 FacePoint = PolytopeVertices[Face.IndexA].MinkowskiPoint;
    //             DbVector3 ToNewPoint = Sub(NewVertex.MinkowskiPoint, FacePoint);
    //             float DotValue = Dot(Face.Normal, ToNewPoint);

    //             if (DotValue > 0f)
    //             {
    //                 Face.Obsolete = true;
    //                 Faces[FaceIndex] = Face;

    //                 AddEdge(Edges, Face.IndexA, Face.IndexB);
    //                 AddEdge(Edges, Face.IndexB, Face.IndexC);
    //                 AddEdge(Edges, Face.IndexC, Face.IndexA);
    //             }
    //         }

    //         for (int FaceIndex = Faces.Count - 1; FaceIndex >= 0; FaceIndex--)
    //         {
    //             if (Faces[FaceIndex].Obsolete)
    //             {
    //                 Faces.RemoveAt(FaceIndex);
    //             }
    //         }

    //         for (int EdgeIndex = 0; EdgeIndex < Edges.Count; EdgeIndex++)
    //         {
    //             EpaEdge Edge = Edges[EdgeIndex];
    //             if (Edge.Obsolete)
    //             {
    //                 continue;
    //             }

    //             AddFace(PolytopeVertices, Faces, Edge.IndexA, Edge.IndexB, NewVertexIndex);
    //         }
    //     }

    //     int FinalClosestFaceIndex = -1;
    //     float FinalClosestDistance = float.MaxValue;

    //     for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
    //     {
    //         EpaFace Face = Faces[FaceIndex];
    //         if (Face.Obsolete)
    //         {
    //             continue;
    //         }

    //         if (Face.Distance < FinalClosestDistance)
    //         {
    //             FinalClosestDistance = Face.Distance;
    //             FinalClosestFaceIndex = FaceIndex;
    //         }
    //     }

    //     if (FinalClosestFaceIndex >= 0)
    //     {
    //         EpaFace Face = Faces[FinalClosestFaceIndex];

    //         DbVector3 Normal = Face.Normal;
    //         float LengthSquared = Dot(Normal, Normal);

    //         if (LengthSquared > 1e-12f)
    //         {
    //             float InverseLength = 1f / Sqrt(LengthSquared);
    //             Normal = Mul(Normal, InverseLength);
    //         }
    //         else
    //         {
    //             Normal = NormalizeSmallVector(Gjk.LastDirection, new DbVector3(0f, 1f, 0f));
    //         }

    //         Normal.y = 0f;
    //         float HorizontalLengthSquared = Dot(Normal, Normal);
    //         if (HorizontalLengthSquared > 1e-12f)
    //         {
    //             float InverseHorizontalLength = 1f / Sqrt(HorizontalLengthSquared);
    //             Normal = Mul(Normal, InverseHorizontalLength);
    //         }

    //         DbVector3 RelativeBToA = Sub(PositionA, PositionB);
    //         if (Dot(Normal, RelativeBToA) < 0f)
    //         {
    //             Normal = Negate(Normal);
    //         }

    //         Contact = new ContactEPA(Normal);
    //         return false;
    //     }

    //     return false;
    // }


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
        DbVector3 WorldUp = new(0f, 1f, 0f);
        float MinGroundDot = MathF.Cos(ToRadians(50f));

        DbVector3 Normal = Normalize(RawNormal);

        DbVector3 RelativeBToA = Sub(PositionA, PositionB);
        if (Dot(Normal, RelativeBToA) < 0f) 
            Normal = Negate(Normal);
        

        float UpDot = Dot(Normal, WorldUp);
        if (MathF.Abs(UpDot) > MinGroundDot && UpDot < 0f)
            Normal = Negate(Normal);

        if (UpDot > MinGroundDot)
            Normal = WorldUp;

        else if (MathF.Abs(UpDot) < 0.1f) 
        {
            Normal.y = 0f;
            Normal = Normalize(Normal);
        }

        return Normal;
    }
}
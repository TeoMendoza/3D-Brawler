using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    static Raycast RaycastMatch(ReducerContext Ctx, DbVector3 RayOrigin, DbVector3 RayDirection, float MaxDistance)
    {
        bool HasHit = false;
        float BestDistance = MaxDistance;
        DbVector3 BestPoint = default;
        RaycastHitType BestType = RaycastHitType.None;
        Identity BestIdentity = default;
        long BestEntityId = 0;
        Magician Magician = Ctx.Db.magician.identity.Find(Ctx.Sender) ?? throw new Exception("Magician not found");

        foreach (Magician Other in Ctx.Db.magician.MatchId.Filter(Magician.GameId))
        {
            if (Other.identity == Ctx.Sender) continue;

            Raycast Hit = RaycastComplexCollider(RayOrigin, RayDirection, MaxDistance, Other.Collider, Other.Position, ToRadians(Other.Rotation.Yaw), RaycastHitType.Magician, Other.identity, Other.Id);
            if (Hit.Hit && Hit.HitDistance < BestDistance)
            {
                HasHit = true;
                BestDistance = Hit.HitDistance;
                BestPoint = Hit.HitPoint;
                BestType = Hit.HitType;
                BestIdentity = Hit.HitIdentity;
                BestEntityId = Hit.HitEntityId;
            }
        }

        foreach (Map MapPiece in Ctx.Db.Map.Iter())
        {
            Raycast Hit = RaycastComplexColliderWorldSpace(RayOrigin, RayDirection, BestDistance, MapPiece.Collider, RaycastHitType.MapPiece, default, MapPiece.Id);
            if (Hit.Hit && Hit.HitDistance < BestDistance)
            {
                HasHit = true;
                BestDistance = Hit.HitDistance;
                BestPoint = Hit.HitPoint;
                BestType = Hit.HitType;
                BestIdentity = Hit.HitIdentity;
                BestEntityId = Hit.HitEntityId;
            }
        }

        return new Raycast(HasHit, BestDistance, BestPoint, BestType, BestIdentity, BestEntityId);
    }

    static Raycast RaycastComplexCollider(DbVector3 RayOrigin, DbVector3 RayDirection, float MaxDistance, ComplexCollider Collider, DbVector3 ColliderWorldPosition, float ColliderYawRadians, RaycastHitType HitType, Identity HitIdentity, long HitEntityId)
    {
        bool HasHit = false;
        float BestDistance = MaxDistance;
        DbVector3 BestPoint = default;

        Quaternion InverseYaw = Quaternion.Inverse(Quaternion.CreateFromYawPitchRoll(ColliderYawRadians, 0f, 0f));

        DbVector3 LocalOrigin = Rotate(Sub(RayOrigin, ColliderWorldPosition), InverseYaw);
        DbVector3 LocalDirection = Normalize(Rotate(RayDirection, InverseYaw));

        for (int HullIndex = 0; HullIndex < Collider.ConvexHulls.Count; HullIndex++)
        {
            ConvexHullCollider Hull = Collider.ConvexHulls[HullIndex];

            if (RaycastConvexHullTriangles(LocalOrigin, LocalDirection, BestDistance, Hull, out float HitDistanceLocal))
            {
                HasHit = true;
                BestDistance = HitDistanceLocal;
                DbVector3 LocalHitPoint = Add(LocalOrigin, Mul(LocalDirection, HitDistanceLocal));
                BestPoint = Add(ColliderWorldPosition, Rotate(LocalHitPoint, Quaternion.CreateFromYawPitchRoll(ColliderYawRadians, 0f, 0f)));
            }
        }

        return new Raycast(HasHit, BestDistance, BestPoint, HasHit ? HitType : RaycastHitType.None, HitIdentity, HitEntityId);
    }

    static Raycast RaycastComplexColliderWorldSpace(DbVector3 RayOrigin, DbVector3 RayDirection, float MaxDistance, ComplexCollider Collider, RaycastHitType HitType, Identity HitIdentity, long HitEntityId)
    {
        bool HasHit = false;
        float BestDistance = MaxDistance;
        DbVector3 BestPoint = default;

        for (int HullIndex = 0; HullIndex < Collider.ConvexHulls.Count; HullIndex++)
        {
            ConvexHullCollider Hull = Collider.ConvexHulls[HullIndex];

            if (RaycastConvexHullTriangles(RayOrigin, RayDirection, BestDistance, Hull, out float HitDistance))
            {
                HasHit = true;
                BestDistance = HitDistance;
                BestPoint = Add(RayOrigin, Mul(RayDirection, HitDistance));
            }
        }

        return new Raycast(HasHit, BestDistance, BestPoint, HasHit ? HitType : RaycastHitType.None, HitIdentity, HitEntityId);
    }

    static bool RaycastConvexHullTriangles(DbVector3 RayOriginLocal, DbVector3 RayDirectionLocal, float MaxDistance, ConvexHullCollider Hull, out float HitDistance)
    {
        HitDistance = MaxDistance;
        bool HasHit = false;

        List<DbVector3> Vertices = Hull.VerticesLocal;
        List<int> Triangles = Hull.TriangleIndicesLocal;

        for (int Index = 0; Index < Triangles.Count; Index += 3)
        {
            DbVector3 A = Vertices[Triangles[Index]];
            DbVector3 B = Vertices[Triangles[Index + 1]];
            DbVector3 C = Vertices[Triangles[Index + 2]];

            if (RayIntersectsTriangle(RayOriginLocal, RayDirectionLocal, A, B, C, out float TriangleDistance))
            {
                if (TriangleDistance >= 0f && TriangleDistance < HitDistance)
                {
                    HasHit = true;
                    HitDistance = TriangleDistance;
                }
            }
        }

        return HasHit;
    }

    static bool RayIntersectsTriangle(DbVector3 RayOrigin, DbVector3 RayDirection, DbVector3 A, DbVector3 B, DbVector3 C, out float Distance)
    {
        Distance = 0f;

        DbVector3 Edge1 = Sub(B, A);
        DbVector3 Edge2 = Sub(C, A);

        DbVector3 Pvec = Cross(RayDirection, Edge2);
        float Det = Dot(Edge1, Pvec);

        float Epsilon = 1e-7f;
        if (Det > -Epsilon && Det < Epsilon) return false;

        float InverseDet = 1f / Det;

        DbVector3 Tvec = Sub(RayOrigin, A);
        float U = Dot(Tvec, Pvec) * InverseDet;
        if (U < 0f || U > 1f) return false;

        DbVector3 Qvec = Cross(Tvec, Edge1);
        float V = Dot(RayDirection, Qvec) * InverseDet;
        if (V < 0f || U + V > 1f) return false;

        float T = Dot(Edge2, Qvec) * InverseDet;
        if (T < 0f) return false;

        Distance = T;
        return true;
    }
}
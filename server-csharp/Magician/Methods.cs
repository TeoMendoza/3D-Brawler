using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static void AdjustGrounded(ReducerContext ctx, DbVector3 MoveVelocity, ref Magician Magician)
    {

        if (Magician.KinematicInformation.Grounded is true) {
             Magician.KinematicInformation.Falling = false;
            RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
            RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanCrouch").Subscribers, "Jump"); 
        }
        else {
            Magician.KinematicInformation.Falling = MoveVelocity.y < -2f;
            AddSubscriberUnique(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
            AddSubscriberUnique(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanCrouch").Subscribers, "Jump");   
        }

    }

    public static void ResolveContacts(ref Magician CharacterLocal, List<CollisionContact> Contacts, DbVector3 InputVelocity)
    {
        DbVector3 WorldUp = new(0f, 1f, 0f);

        float MinGroundDot = 0.75f;

        float DepthEpsilon = 2e-3f;
        float MaxDepth = 0.08f;

        float CorrectionFactor = 0.5f;
        float TargetPenetration = 0.01f;

        float MaxPositionCorrection = 0.015f;

        float GroundStickUpThreshold = 0.03f;
        float InputUpCancelThreshold = 0.03f;

        DbVector3 CorrectedVelocity = InputVelocity;

        bool IsGroundedOnMap = false;
        DbVector3 GroundSupportNormal = WorldUp;

        bool HasAnyPositionCorrection = false;
        DbVector3 TotalPositionCorrection = new(0f, 0f, 0f);

        foreach (CollisionContact Contact in Contacts)
        {
            DbVector3 Normal = Contact.Normal;

            float UpDot = Dot(Normal, WorldUp);
            bool IsWalkableMapContact = Contact.CollisionType == CollisionEntryType.Map && UpDot >= MinGroundDot;

            if (IsWalkableMapContact)
            {
                IsGroundedOnMap = true;
                GroundSupportNormal = Normal;
            }

            float NormalVelocityComponent = Dot(Normal, CorrectedVelocity);
            if (NormalVelocityComponent < 0f)
                CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, NormalVelocityComponent));

            float Depth = Contact.PenetrationDepth - TargetPenetration;
            if (Depth > DepthEpsilon)
            {
                if (Depth > MaxDepth) Depth = MaxDepth;
                HasAnyPositionCorrection = true;
                TotalPositionCorrection = Add(TotalPositionCorrection, Mul(Normal, Depth));
            }
        }

        if (HasAnyPositionCorrection)
        {
            float CorrectionMagnitudeSq = Dot(TotalPositionCorrection, TotalPositionCorrection);
            if (CorrectionMagnitudeSq > 1e-8f)
            {
                float CorrectionMagnitude = MathF.Sqrt(CorrectionMagnitudeSq);
                if (CorrectionMagnitude > MaxPositionCorrection)
                    TotalPositionCorrection = Mul(Normalize(TotalPositionCorrection), MaxPositionCorrection);

                TotalPositionCorrection = Mul(TotalPositionCorrection, CorrectionFactor);
                CharacterLocal.Position = Add(CharacterLocal.Position, TotalPositionCorrection);
            }
        }

        if (IsGroundedOnMap)
        {
            float DesiredHorizontalSpeedSq = InputVelocity.x * InputVelocity.x + InputVelocity.z * InputVelocity.z;

            if (DesiredHorizontalSpeedSq < 0.001f)
            {
                CorrectedVelocity.x = 0f;
                CorrectedVelocity.z = 0f;
            }
            else
            {
                float DesiredHorizontalSpeed = MathF.Sqrt(DesiredHorizontalSpeedSq);
                float PreservedCorrectedY = CorrectedVelocity.y;

                DbVector3 TangentVelocity = Sub(CorrectedVelocity, Mul(GroundSupportNormal, Dot(CorrectedVelocity, GroundSupportNormal)));
                float TangentSpeedSq = Dot(TangentVelocity, TangentVelocity);

                if (TangentSpeedSq > 1e-8f)
                {
                    DbVector3 TangentDirection = Mul(TangentVelocity, 1f / MathF.Sqrt(TangentSpeedSq));

                    float TangentDirectionHorizontalSq = TangentDirection.x * TangentDirection.x + TangentDirection.z * TangentDirection.z;
                    if (TangentDirectionHorizontalSq > 1e-8f)
                    {
                        float TangentDirectionHorizontal = MathF.Sqrt(TangentDirectionHorizontalSq);
                        float Scale = DesiredHorizontalSpeed / TangentDirectionHorizontal;

                        DbVector3 DesiredTangentVelocity = Mul(TangentDirection, Scale);

                        CorrectedVelocity.x = DesiredTangentVelocity.x;
                        CorrectedVelocity.z = DesiredTangentVelocity.z;
                        CorrectedVelocity.y = PreservedCorrectedY;
                    }
                }
            }

            if (CorrectedVelocity.y <= GroundStickUpThreshold)
                CorrectedVelocity.y = 0f;

            if (InputVelocity.y <= InputUpCancelThreshold)
                CharacterLocal.Velocity.y = 0f;
        }

        CharacterLocal.IsColliding = Contacts.Count > 0;
        CharacterLocal.CorrectedVelocity = CorrectedVelocity;
        CharacterLocal.KinematicInformation.Grounded = CharacterLocal.KinematicInformation.Grounded || IsGroundedOnMap;
    }
    private static bool TryBuildContactForEntry(ReducerContext Ctx, ref Magician CharacterLocal, CollisionEntry CollisionEntry, List<CollisionContact> Contacts)
    {
        DbVector3 PositionA = CharacterLocal.Position;
        float YawRadiansA = ToRadians(CharacterLocal.Rotation.Yaw);

        if (CollisionEntry.Type == CollisionEntryType.Magician)
        {
            Magician OtherMagician = Ctx.Db.magician.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Magician Not Found");
            if (OtherMagician.Id == CharacterLocal.Id) return false;

            List<ConvexHullCollider> ColliderA = CharacterLocal.GjkCollider.ConvexHulls;
            List<ConvexHullCollider> ColliderB = OtherMagician.GjkCollider.ConvexHulls;

            DbVector3 PositionB = OtherMagician.Position;
            float YawRadiansB = ToRadians(OtherMagician.Rotation.Yaw);

            bool IntersectsMagician = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMagician);
            if (IntersectsMagician is false) return false;

            DbVector3 CenterAWorld = GetColliderCenterWorld(CharacterLocal.GjkCollider, PositionA, YawRadiansA);
            DbVector3 CenterBWorld = GetColliderCenterWorld(OtherMagician.GjkCollider, PositionB, YawRadiansB);

            if (EpaSolve(GjkResultMagician, ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out Contact EpaContact))
            {
                DbVector3 ContactNormal = ComputeContactNormal(EpaContact.Normal, CenterAWorld, CenterBWorld);
                float PenetrationDepth = EpaContact.Depth;

                Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth, CollisionEntryType.Magician));
                return true;
            }
        }

        if (CollisionEntry.Type == CollisionEntryType.Map)
        {
            Map MapPiece = Ctx.Db.Map.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

            List<ConvexHullCollider> ColliderA = CharacterLocal.GjkCollider.ConvexHulls;
            List<ConvexHullCollider> ColliderB = MapPiece.GjkCollider.ConvexHulls;

            DbVector3 PositionB = new DbVector3(0f, 0f, 0f);
            float YawRadiansB = 0f;

            bool IntersectsMap = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMap);
            if (IntersectsMap is false) return false;

            DbVector3 CenterAWorld = GetColliderCenterWorld(CharacterLocal.GjkCollider, PositionA, YawRadiansA);
            DbVector3 CenterBWorld = GetColliderCenterWorld(MapPiece.GjkCollider, PositionB, YawRadiansB);

            if (EpaSolve(GjkResultMap, ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out Contact EpaContact))
            {
                DbVector3 ContactNormal = ComputeContactNormal(EpaContact.Normal, CenterAWorld, CenterBWorld);
                float PenetrationDepth = EpaContact.Depth;

                Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth, CollisionEntryType.Map));
                return true;
            }
        }

        return false;
    }

    static bool TryForceOverlapForEntry(ReducerContext Ctx, ref Magician Character, CollisionEntry Entry, bool WasGrounded)
    {
        if (Entry.Type != CollisionEntryType.Map) return false;
        if (WasGrounded is false && Character.KinematicInformation.Grounded is false) return false;

        float UpwardVelocityBlockThreshold = 0.03f;
        if (Character.Velocity.y > UpwardVelocityBlockThreshold) return false;

        DbVector3 WorldUp = new DbVector3(0f, 1f, 0f);

        float MinGroundDot = 0.75f;
        float FloorUpDot = 0.98f;

        float MaxVerticalGapRamp = 0.045f;
        float MaxVerticalSnap = 0.01f;

        float TinyOverlap = 0.0005f;
        float OverlapEnableGap = 0.01f;

        ComplexCollider ColliderAComplex = Character.GjkCollider;
        List<ConvexHullCollider> ColliderA = ColliderAComplex.ConvexHulls;

        Map MapPiece = Ctx.Db.Map.Id.Find(Entry.Id) ?? throw new Exception("Colliding Map Piece Not Found");
        ComplexCollider ColliderBComplex = MapPiece.GjkCollider;
        List<ConvexHullCollider> ColliderB = ColliderBComplex.ConvexHulls;

        DbVector3 PositionA = Character.Position;
        DbVector3 PositionB = new DbVector3(0f, 0f, 0f);

        float YawA = ToRadians(Character.Rotation.Yaw);
        float YawB = 0f;

        if (SolveGjkDistance(ColliderA, PositionA, YawA, ColliderB, PositionB, YawB, out GjkDistanceResult DistanceResult) is false)
            return false;

        DbVector3 CenterAWorld = GetColliderCenterWorld(ColliderAComplex, PositionA, YawA);
        DbVector3 CenterBWorld = GetColliderCenterWorld(ColliderBComplex, PositionB, YawB);

        DbVector3 ContactNormal = ComputeContactNormal(DistanceResult.SeparationDirection, CenterAWorld, CenterBWorld);

        float UpDot = Dot(ContactNormal, WorldUp);
        if (UpDot < MinGroundDot) return false;

        if (UpDot > FloorUpDot) return false;

        DbVector3 Delta = Sub(DistanceResult.PointOnA, DistanceResult.PointOnB);
        float VerticalGap = Dot(Delta, WorldUp);

        if (VerticalGap <= 0f) return false;
        if (VerticalGap > MaxVerticalGapRamp) return false;

        float SnapDown = VerticalGap;
        if (VerticalGap <= OverlapEnableGap)
            SnapDown = VerticalGap + TinyOverlap;

        if (SnapDown > MaxVerticalSnap)
            SnapDown = MaxVerticalSnap;

        if (SnapDown <= 1e-6f) return false;

        Character.Position = Add(Character.Position, Mul(WorldUp, -SnapDown));
        return true;
    }
}

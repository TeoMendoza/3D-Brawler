using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [Reducer]
    public static void MoveMagiciansCCD(ReducerContext Ctx, Move_All_Magicians_Timer Timer)
    {
        float TickTime = Timer.tick_rate;
        float MinTimeStep = 1e-4f;
        int MaxSubsteps = 4;

        float CcdSlop = 0.05f;

        foreach (var CharacterRow in Ctx.Db.magician.Iter())
        {
            Magician Character = CharacterRow;

            bool WasGrounded = Character.KinematicInformation.Grounded;
            Character.KinematicInformation.Grounded = false;
            Character.IsColliding = false;
            Character.CorrectedVelocity = Character.Velocity;

            float RemainingTime = TickTime;
            int SubstepCount = 0;

            List<CollisionEntry> ResolvedEntriesThisTick = new List<CollisionEntry>();

            while (RemainingTime > MinTimeStep && SubstepCount < MaxSubsteps)
            {
                SubstepCount += 1;

                DbVector3 CurrentStepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;

                bool HasEarliestHit = false;
                float EarliestCollisionTime = RemainingTime;
                CollisionEntry EarliestCollisionEntry = default;

                DbVector3 PositionAStart = Character.Position;
                float YawRadiansAStart = ToRadians(Character.Rotation.Yaw);

                List<CollisionContact> ContactsThisStep = new List<CollisionContact>();
                List<CollisionEntry> ContactEntriesThisStep = new List<CollisionEntry>();

                foreach (CollisionEntry CollisionEntry in Character.CollisionEntries)
                {
                    if (ResolvedEntriesThisTick.Contains(CollisionEntry)) continue;

                    DbVector3 PositionA = PositionAStart;
                    DbVector3 VelocityA = CurrentStepVelocity;
                    float YawRadiansA = YawRadiansAStart;

                    DbVector3 PositionB;
                    float YawRadiansB;
                    List<ConvexHullCollider> ColliderA;
                    List<ConvexHullCollider> ColliderB;

                    if (TryBuildContactForEntryCCD(Ctx, ref Character, CollisionEntry, ContactsThisStep))
                    {
                        ContactEntriesThisStep.Add(CollisionEntry);
                        continue;
                    }

                    switch (CollisionEntry.Type)
                    {
                        case CollisionEntryType.Magician:
                        {
                            Magician OtherMagician = Ctx.Db.magician.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Magician Not Found");
                            if (OtherMagician.Id == Character.Id) break;

                            ColliderA = Character.Collider.ConvexHulls;
                            ColliderB = OtherMagician.Collider.ConvexHulls;
                            PositionB = OtherMagician.Position;
                            YawRadiansB = ToRadians(OtherMagician.Rotation.Yaw);
                            DbVector3 VelocityB = OtherMagician.IsColliding ? OtherMagician.CorrectedVelocity : OtherMagician.Velocity;

                            if (SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkDistanceResult Dist) is false)
                                break;

                            float Sep = Dist.Distance;

                            DbVector3 CenterAWorld = GetColliderCenterWorld(Character.Collider, PositionA, YawRadiansA);
                            DbVector3 CenterBWorld = GetColliderCenterWorld(OtherMagician.Collider, PositionB, YawRadiansB);

                            DbVector3 Normal = ComputeContactNormal(Dist.SeparationDirection, CenterAWorld, CenterBWorld);

                            DbVector3 RelVel = Sub(VelocityA, VelocityB);
                            float RelSpeedSq = Dot(RelVel, RelVel);
                            if (RelSpeedSq < 1e-6f)
                                break;

                            float Closing = Dot(RelVel, Normal);
                            if (Closing >= 0f)
                                break;

                            float EffectiveSeparation = Sep - CcdSlop;
                            if (EffectiveSeparation < 0f) EffectiveSeparation = 0f;

                            float HitTime = EffectiveSeparation / -Closing;
                            if (HitTime < 0f)
                                break;

                            if (HitTime > RemainingTime)
                                break;

                            if (HasEarliestHit is false || HitTime < EarliestCollisionTime)
                            {
                                HasEarliestHit = true;
                                EarliestCollisionTime = HitTime;
                                EarliestCollisionEntry = CollisionEntry;
                            }

                            break;
                        }

                        case CollisionEntryType.Map:
                        {
                            Map MapPiece = Ctx.Db.Map.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

                            ColliderA = Character.Collider.ConvexHulls;
                            ColliderB = MapPiece.Collider.ConvexHulls;
                            PositionB = new DbVector3(0f, 0f, 0f);
                            YawRadiansB = 0f;

                            if (SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkDistanceResult DistanceResultMap) is false)
                                break;

                            float SeparationDistanceMap = DistanceResultMap.Distance;

                            DbVector3 CenterAWorld = GetColliderCenterWorld(Character.Collider, PositionA, YawRadiansA);
                            DbVector3 CenterBWorld = GetColliderCenterWorld(MapPiece.Collider, PositionB, YawRadiansB);

                            DbVector3 Normal = ComputeContactNormal(DistanceResultMap.SeparationDirection, CenterAWorld, CenterBWorld);

                            DbVector3 RelativeVelocityMap = VelocityA;
                            float RelativeSpeedSquaredMap = Dot(RelativeVelocityMap, RelativeVelocityMap);
                            if (RelativeSpeedSquaredMap < 1e-6f)
                                break;

                            float ClosingSpeedMap = Dot(RelativeVelocityMap, Normal);
                            if (ClosingSpeedMap >= 0f)
                                break;

                            float EffectiveSeparationMap = SeparationDistanceMap - CcdSlop;
                            if (EffectiveSeparationMap < 0f) EffectiveSeparationMap = 0f;

                            float CandidateHitTimeMap = EffectiveSeparationMap / -ClosingSpeedMap;
                            if (CandidateHitTimeMap < 0f)
                                break;

                            if (CandidateHitTimeMap > RemainingTime)
                                break;

                            if (HasEarliestHit is false || CandidateHitTimeMap < EarliestCollisionTime)
                            {
                                HasEarliestHit = true;
                                EarliestCollisionTime = CandidateHitTimeMap;
                                EarliestCollisionEntry = CollisionEntry;
                            }

                            break;
                        }

                        default:
                            break;
                    }
                }

                if (ContactsThisStep.Count > 0)
                {
                    ResolveContactsCCD(ref Character, ContactsThisStep, Character.Velocity);

                    foreach (CollisionEntry ContactEntry in ContactEntriesThisStep)
                    {
                        if (ResolvedEntriesThisTick.Contains(ContactEntry) is false)
                            ResolvedEntriesThisTick.Add(ContactEntry);
                    }

                    continue;
                }

                if (HasEarliestHit is false)
                {
                    DbVector3 FreeMoveVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;
                    Character.Position = new DbVector3(
                        Character.Position.x + FreeMoveVelocity.x * RemainingTime,
                        Character.Position.y + FreeMoveVelocity.y * RemainingTime,
                        Character.Position.z + FreeMoveVelocity.z * RemainingTime
                    );
                    Character.IsColliding = false;
                    Character.CorrectedVelocity = FreeMoveVelocity;
                    RemainingTime = 0f;
                    break;
                }

                DbVector3 CollisionStepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;

                Character.Position = new DbVector3(
                    Character.Position.x + CollisionStepVelocity.x * EarliestCollisionTime,
                    Character.Position.y + CollisionStepVelocity.y * EarliestCollisionTime,
                    Character.Position.z + CollisionStepVelocity.z * EarliestCollisionTime
                );

                RemainingTime -= EarliestCollisionTime;
                ContactsThisStep.Clear();

                if (TryBuildContactForEntryCCD(Ctx, ref Character, EarliestCollisionEntry, ContactsThisStep) is false)
                    TryBuildArtificialGjkContactForEntry(Ctx, ref Character, EarliestCollisionEntry, ContactsThisStep);

                if (ContactsThisStep.Count > 0)
                {
                    ResolveContactsCCD(ref Character, ContactsThisStep, Character.Velocity);

                    if (!ResolvedEntriesThisTick.Contains(EarliestCollisionEntry))
                        ResolvedEntriesThisTick.Add(EarliestCollisionEntry);
                }
            }

            DbVector3 FinalStepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;
            float GroundStickVelocityThreshold = 2f;
            bool GroundedThisTick = Character.KinematicInformation.Grounded;

            if (GroundedThisTick is false && WasGrounded is true && MathF.Abs(FinalStepVelocity.y) < GroundStickVelocityThreshold)
                Character.KinematicInformation.Grounded = true;

            AdjustGrounded(Ctx, FinalStepVelocity, ref Character);

            Ctx.Db.magician.identity.Update(Character);
        }
    }


    static bool TryBuildArtificialGjkContactForEntry(ReducerContext Ctx, ref Magician Character, CollisionEntry Entry, List<CollisionContact> ContactsThisStep)
    {
        float SkinThicknessMap = 0.05f;
        float DetectionThresholdMap = 0.15f;

        float SkinThicknessMagician = 0.05f;
        float DetectionThresholdMagician = 0.15f;

        ComplexCollider ColliderAComplex = Character.Collider;
        ComplexCollider ColliderBComplex;

        List<ConvexHullCollider> ColliderA = ColliderAComplex.ConvexHulls;
        List<ConvexHullCollider> ColliderB;
        DbVector3 PositionB;
        float YawA = ToRadians(Character.Rotation.Yaw);
        float YawB;

        CollisionEntryType EntryType;

        switch (Entry.Type)
        {
            case CollisionEntryType.Magician:
            {
                Magician OtherMagician = Ctx.Db.magician.Id.Find(Entry.Id) ?? throw new Exception("Colliding Magician Not Found");
                if (OtherMagician.Id == Character.Id) return false;

                EntryType = CollisionEntryType.Magician;
                ColliderBComplex = OtherMagician.Collider;
                ColliderB = ColliderBComplex.ConvexHulls;
                PositionB = OtherMagician.Position;
                YawB = ToRadians(OtherMagician.Rotation.Yaw);
                break;
            }

            case CollisionEntryType.Map:
            {
                Map MapPiece = Ctx.Db.Map.Id.Find(Entry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

                EntryType = CollisionEntryType.Map;
                ColliderBComplex = MapPiece.Collider;
                ColliderB = ColliderBComplex.ConvexHulls;
                PositionB = new DbVector3(0f, 0f, 0f);
                YawB = 0f;
                break;
            }

            default:
                return false;
        }

        float SkinThickness = (EntryType == CollisionEntryType.Magician) ? SkinThicknessMagician : SkinThicknessMap;
        float DetectionThreshold = (EntryType == CollisionEntryType.Magician) ? DetectionThresholdMagician : DetectionThresholdMap;

        DbVector3 PositionA = Character.Position;

        if (SolveGjkDistance(ColliderA, PositionA, YawA, ColliderB, PositionB, YawB, out GjkDistanceResult DistanceResult) is false)
            return false;

        float Separation = DistanceResult.Distance;
        if (Separation > DetectionThreshold)
            return false;

        DbVector3 CenterAWorld = GetColliderCenterWorld(ColliderAComplex, PositionA, YawA);
        DbVector3 CenterBWorld = GetColliderCenterWorld(ColliderBComplex, PositionB, YawB);

        DbVector3 ContactNormal = ComputeContactNormal(DistanceResult.SeparationDirection, CenterAWorld, CenterBWorld);

        float ArtificialPenetration = SkinThickness - Separation;
        if (ArtificialPenetration < 0f) ArtificialPenetration = 0f;

        ContactsThisStep.Clear();
        ContactsThisStep.Add(new CollisionContact(ContactNormal, ArtificialPenetration, EntryType));

        return true;
    }



    private static bool TryBuildContactForEntryCCD(ReducerContext Ctx, ref Magician CharacterLocal, CollisionEntry CollisionEntry, List<CollisionContact> Contacts)
    {
        DbVector3 PositionA = CharacterLocal.Position;
        float YawRadiansA = ToRadians(CharacterLocal.Rotation.Yaw);
        DbVector3 VelocityA = CharacterLocal.IsColliding ? CharacterLocal.CorrectedVelocity : CharacterLocal.Velocity;;
        if (CollisionEntry.Type == CollisionEntryType.Magician)
        {
            Magician OtherMagician = Ctx.Db.magician.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Magician Not Found");
            if (OtherMagician.Id == CharacterLocal.Id) return false;

            List<ConvexHullCollider> ColliderA = CharacterLocal.Collider.ConvexHulls;
            List<ConvexHullCollider> ColliderB = OtherMagician.Collider.ConvexHulls;

            DbVector3 PositionB = OtherMagician.Position;
            float YawRadiansB = ToRadians(OtherMagician.Rotation.Yaw);
            DbVector3 VelocityB = OtherMagician.IsColliding ? OtherMagician.CorrectedVelocity : OtherMagician.Velocity;

            bool IntersectsMagician = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMagician);
            if (IntersectsMagician is false) return false;

            GjkVertex SkinPoints = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, GjkResultMagician.LastDirection);
            DbVector3 PointOnA = SkinPoints.SupportPointA;
            DbVector3 PointOnB = SkinPoints.SupportPointB;

            DbVector3 CenterAWorld = GetColliderCenterWorld(CharacterLocal.Collider, PositionA, YawRadiansA);
            DbVector3 CenterBWorld = GetColliderCenterWorld(OtherMagician.Collider, PositionB, YawRadiansB);

            DbVector3 ContactNormal = ComputeContactNormal(GjkResultMagician.LastDirection, CenterAWorld, CenterBWorld);

            float DistanceA = Dot(PointOnA, ContactNormal);
            float DistanceB = Dot(PointOnB, ContactNormal);
            float Gap = DistanceB - DistanceA;
            float PenetrationDepth = (Gap > 0f) ? Gap : 0f;

            Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth, CollisionEntryType.Magician));
            return true;
    
        }

        if (CollisionEntry.Type == CollisionEntryType.Map)
        {
            Map MapPiece = Ctx.Db.Map.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

            List<ConvexHullCollider> ColliderA = CharacterLocal.Collider.ConvexHulls;
            List<ConvexHullCollider> ColliderB = MapPiece.Collider.ConvexHulls;

            DbVector3 PositionB = new DbVector3(0f, 0f, 0f);
            float YawRadiansB = 0f;
            DbVector3 VelocityB = new DbVector3(0f, 0f, 0f);

            bool IntersectsMap = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMap);
            if (IntersectsMap is false) return false;
            
            GjkVertex SkinPoints = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, GjkResultMap.LastDirection);
            DbVector3 PointOnA = SkinPoints.SupportPointA;
            DbVector3 PointOnB = SkinPoints.SupportPointB;

            DbVector3 CenterAWorld = GetColliderCenterWorld(CharacterLocal.Collider, PositionA, YawRadiansA);
            DbVector3 CenterBWorld = GetColliderCenterWorld(MapPiece.Collider, PositionB, YawRadiansB);

            DbVector3 ContactNormal = ComputeContactNormal(GjkResultMap.LastDirection, CenterAWorld, CenterBWorld);

            float DistanceA = Dot(PointOnA, ContactNormal);
            float DistanceB = Dot(PointOnB, ContactNormal);
            float Gap = DistanceB - DistanceA;
            float PenetrationDepth = (Gap > 0f) ? Gap : 0f;

            Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth, CollisionEntryType.Map));
            return true;
        }

        return false;
    }

    public static void ResolveContactsCCD(ref Magician CharacterLocal, List<CollisionContact> Contacts, DbVector3 InputVelocity)
    {
        DbVector3 WorldUp = new(0f, 1f, 0f);

        float MinGroundDot = 0.7f;
        float DepthEpsilon = 1e-4f;
        float MaxDepth = 0.25f;

        float CorrectionFactor = 1f;
        float TargetPenetration = 0.025f;

        float GroundStickUpThreshold = 0.1f;
        float InputUpCancelThreshold = 0.1f;

        DbVector3 CorrectedVelocity = InputVelocity;

        bool IsGroundedOnMap = false;

        float MaxPenetrationDepth = 0f;
        DbVector3 BestPositionNormal = WorldUp;
        bool HasPositionContact = false;

        foreach (CollisionContact Contact in Contacts)
        {
            DbVector3 Normal = Contact.Normal;

            float UpDot = Dot(Normal, WorldUp);
            if (UpDot >= MinGroundDot && Contact.CollisionType == CollisionEntryType.Map)
                IsGroundedOnMap = true;

            float NormalVelocityComponent = Dot(Normal, CorrectedVelocity);
            if (NormalVelocityComponent < 0f)
                CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, NormalVelocityComponent));

            if (Contact.PenetrationDepth > MaxPenetrationDepth)
            {
                MaxPenetrationDepth = Contact.PenetrationDepth;
                BestPositionNormal = Normal;
                HasPositionContact = true;
            }
        }

        if (IsGroundedOnMap)
        {
            float HorizInputSq = InputVelocity.x * InputVelocity.x + InputVelocity.z * InputVelocity.z;
            if (HorizInputSq < 0.001f)
            {
                CorrectedVelocity.x = 0f;
                CorrectedVelocity.z = 0f;
            }

            if (CorrectedVelocity.y <= GroundStickUpThreshold)
                CorrectedVelocity.y = 0f;

            if (InputVelocity.y <= InputUpCancelThreshold)
                CharacterLocal.Velocity.y = 0f;
        }

        if (HasPositionContact && MaxPenetrationDepth > DepthEpsilon)
        {
            if (MaxPenetrationDepth > MaxDepth)
                MaxPenetrationDepth = MaxDepth;

            float EffectiveDepth = MaxPenetrationDepth - TargetPenetration;
            if (EffectiveDepth < 0f)
                EffectiveDepth = 0f;

            DbVector3 PositionCorrection = Mul(BestPositionNormal, EffectiveDepth * CorrectionFactor);
            CharacterLocal.Position = Add(CharacterLocal.Position, PositionCorrection);
        }

        CharacterLocal.IsColliding = Contacts.Count > 0;
        CharacterLocal.CorrectedVelocity = CorrectedVelocity;
        CharacterLocal.KinematicInformation.Grounded = CharacterLocal.KinematicInformation.Grounded || IsGroundedOnMap;
    }
}
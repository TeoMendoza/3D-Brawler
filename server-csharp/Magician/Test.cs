using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    
    [Reducer]
public static void MoveMagicians(ReducerContext Ctx, Move_All_Magicians_Timer Timer)
{
    float TickTime = Timer.tick_rate;
    float MinTimeStep = 1e-4f;
    int MaxSubsteps = 4;

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

                if (TryBuildContactForEntry(Ctx, ref Character, CollisionEntry, ContactsThisStep))
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

                        ColliderA = Character.GjkCollider.ConvexHulls;
                        ColliderB = OtherMagician.GjkCollider.ConvexHulls;
                        PositionB = OtherMagician.Position;
                        YawRadiansB = ToRadians(OtherMagician.Rotation.Yaw);
                        DbVector3 VelocityB = OtherMagician.IsColliding ? OtherMagician.CorrectedVelocity : OtherMagician.Velocity;

                        GjkDistanceResult Dist;
                        if (SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out Dist) is false) break;
                        
                        float Sep = Dist.Distance;
                        DbVector3 Normal = Negate(Dist.SeparationDirection);

                        DbVector3 RelVel = Sub(VelocityA, VelocityB);
                        float RelSpeedSq = Dot(RelVel, RelVel);
                        if (RelSpeedSq < 1e-6f)
                            break;
                        
                        float Closing = Dot(RelVel, Normal);
                        if (Closing >= 0f)
                            break;
                        
                        float HitTime = Sep / -Closing;
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

                        ColliderA = Character.GjkCollider.ConvexHulls;
                        ColliderB = MapPiece.GjkCollider.ConvexHulls;
                        PositionB = new DbVector3(0f, 0f, 0f);
                        YawRadiansB = 0f;

                        GjkDistanceResult DistanceResultMap;
                        if (SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out DistanceResultMap) is false) break;
                        
                        float SeparationDistanceMap = DistanceResultMap.Distance;
                        DbVector3 Normal = Negate(DistanceResultMap.SeparationDirection);

                        DbVector3 RelativeVelocityMap = VelocityA;
                        float RelativeSpeedSquaredMap = Dot(RelativeVelocityMap, RelativeVelocityMap);
                        if (RelativeSpeedSquaredMap < 1e-6f)
                            break;
                        
                        float ClosingSpeedMap = Dot(RelativeVelocityMap, Normal);
                        if (ClosingSpeedMap >= 0f)
                            break;
                        
                        float CandidateHitTimeMap = SeparationDistanceMap / -ClosingSpeedMap;
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
                ResolveContacts(ref Character, ContactsThisStep, Character.Velocity);

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

            if (TryBuildContactForEntry(Ctx, ref Character, EarliestCollisionEntry, ContactsThisStep) is false)
                TryBuildArtificialGjkContactForEntry(Ctx, ref Character, EarliestCollisionEntry, ContactsThisStep);

            if (ContactsThisStep.Count > 0)
            {
                ResolveContacts(ref Character, ContactsThisStep, Character.Velocity);

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
        float SkinThickness = 0.05f;
        float DetectionThreshold = 0.075f;

        ComplexCollider ColliderAComplex = Character.GjkCollider;
        ComplexCollider ColliderBComplex;

        List<ConvexHullCollider> ColliderA = ColliderAComplex.ConvexHulls;
        List<ConvexHullCollider> ColliderB;
        DbVector3 PositionB;
        float YawA = ToRadians(Character.Rotation.Yaw);
        float YawB;

        switch (Entry.Type)
        {
            case CollisionEntryType.Magician:
            {
                Magician OtherMagician = Ctx.Db.magician.Id.Find(Entry.Id) ?? throw new Exception("Colliding Magician Not Found");
                if (OtherMagician.Id == Character.Id) return false;

                ColliderBComplex = OtherMagician.GjkCollider;
                ColliderB = ColliderBComplex.ConvexHulls;
                PositionB = OtherMagician.Position;
                YawB = ToRadians(OtherMagician.Rotation.Yaw);
                break;
            }

            case CollisionEntryType.Map:
            {
                Map MapPiece = Ctx.Db.Map.Id.Find(Entry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

                ColliderBComplex = MapPiece.GjkCollider;
                ColliderB = ColliderBComplex.ConvexHulls;
                PositionB = new DbVector3(0f, 0f, 0f);
                YawB = 0f;
                break;
            }

            default:
                return false;
        }

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
        ContactsThisStep.Add(new CollisionContact
        {
            Normal = ContactNormal,
            PenetrationDepth = ArtificialPenetration
        });

        return true;
    }


    private static bool TryBuildContactForEntry(ReducerContext Ctx, ref Magician CharacterLocal, CollisionEntry CollisionEntry, List<CollisionContact> Contacts)
    {
        DbVector3 PositionA = CharacterLocal.Position;
        float YawRadiansA = ToRadians(CharacterLocal.Rotation.Yaw);
        DbVector3 VelocityA = CharacterLocal.IsColliding ? CharacterLocal.CorrectedVelocity : CharacterLocal.Velocity;;
        if (CollisionEntry.Type == CollisionEntryType.Magician)
        {
            Magician OtherMagician = Ctx.Db.magician.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Magician Not Found");
            if (OtherMagician.Id == CharacterLocal.Id) return false;

            List<ConvexHullCollider> ColliderA = CharacterLocal.GjkCollider.ConvexHulls;
            List<ConvexHullCollider> ColliderB = OtherMagician.GjkCollider.ConvexHulls;

            DbVector3 PositionB = OtherMagician.Position;
            float YawRadiansB = ToRadians(OtherMagician.Rotation.Yaw);
            DbVector3 VelocityB = OtherMagician.IsColliding ? OtherMagician.CorrectedVelocity : OtherMagician.Velocity;

            bool IntersectsMagician = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMagician);
            if (IntersectsMagician is false) return false;

            GjkVertex SkinPoints = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, GjkResultMagician.LastDirection);
            DbVector3 PointOnA = SkinPoints.SupportPointA;
            DbVector3 PointOnB = SkinPoints.SupportPointB;

            DbVector3 CenterAWorld = GetColliderCenterWorld(CharacterLocal.GjkCollider, PositionA, YawRadiansA);
            DbVector3 CenterBWorld = GetColliderCenterWorld(OtherMagician.GjkCollider, PositionB, YawRadiansB);

            DbVector3 ContactNormal = ComputeContactNormal(GjkResultMagician.LastDirection, CenterAWorld, CenterBWorld);

            float DistanceA = Dot(PointOnA, ContactNormal);
            float DistanceB = Dot(PointOnB, ContactNormal);
            float Gap = DistanceB - DistanceA;
            float PenetrationDepth = (Gap > 0f) ? Gap : 0f;

            Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth));
            return true;
    
        }

        if (CollisionEntry.Type == CollisionEntryType.Map)
        {
            Map MapPiece = Ctx.Db.Map.Id.Find(CollisionEntry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

            List<ConvexHullCollider> ColliderA = CharacterLocal.GjkCollider.ConvexHulls;
            List<ConvexHullCollider> ColliderB = MapPiece.GjkCollider.ConvexHulls;

            DbVector3 PositionB = new DbVector3(0f, 0f, 0f);
            float YawRadiansB = 0f;
            DbVector3 VelocityB = new DbVector3(0f, 0f, 0f);

            bool IntersectsMap = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMap);
            if (IntersectsMap is false) return false;
            
            GjkVertex SkinPoints = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, GjkResultMap.LastDirection);
            DbVector3 PointOnA = SkinPoints.SupportPointA;
            DbVector3 PointOnB = SkinPoints.SupportPointB;

            DbVector3 CenterAWorld = GetColliderCenterWorld(CharacterLocal.GjkCollider, PositionA, YawRadiansA);
            DbVector3 CenterBWorld = GetColliderCenterWorld(MapPiece.GjkCollider, PositionB, YawRadiansB);

            DbVector3 ContactNormal = ComputeContactNormal(GjkResultMap.LastDirection, CenterAWorld, CenterBWorld);

            float DistanceA = Dot(PointOnA, ContactNormal);
            float DistanceB = Dot(PointOnB, ContactNormal);
            float Gap = DistanceB - DistanceA;
            float PenetrationDepth = (Gap > 0f) ? Gap : 0f;

            Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth));
            return true;
        }

        return false;
    }

    public static void ResolveContacts(ref Magician CharacterLocal, List<CollisionContact> Contacts, DbVector3 InputVelocity)
    {
        DbVector3 WorldUp = new(0f, 1f, 0f);
        
        // Thresholds
        float MinGroundDot = 0.7f; // Must match ComputeContactNormal (approx 45 deg)
        float DepthEpsilon = 1e-4f;
        float MaxDepth = 0.25f;
        
        // Tuning (Soft & Precise)
        float CorrectionFactor = 0.8f; 
        float TargetPenetration = 0.025f; 
        float ClimbVelocityThreshold = 0.1f;

        DbVector3 CorrectedVelocity = InputVelocity;
        bool IsGrounded = false;

        float MaxPenetrationDepth = 0f;
        DbVector3 BestPositionNormal = WorldUp;
        bool HasPositionContact = false;

        foreach (CollisionContact Contact in Contacts)
        {
            DbVector3 Normal = Contact.Normal; 

            // 1. Update Ground Flag
            float UpDot = Dot(Normal, WorldUp);
            if (UpDot >= MinGroundDot) 
                IsGrounded = true;
            
            // 2. Velocity Resolution
            float NormalVelocityComponent = Dot(Normal, CorrectedVelocity);
            if (NormalVelocityComponent < 0f) 
                CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, NormalVelocityComponent));
            

            // 3. Track Deepest Penetration for Position Correction
            if (Contact.PenetrationDepth > MaxPenetrationDepth)
            {
                MaxPenetrationDepth = Contact.PenetrationDepth;
                BestPositionNormal = Normal;
                HasPositionContact = true;
            }
        }

        // 4. Grounding cleanup (Prevents jittery sliding down flat surfaces)
        if (IsGrounded)
        {
            float HorizInputSq = InputVelocity.x * InputVelocity.x + InputVelocity.z * InputVelocity.z;
            if (HorizInputSq < 0.001f)
            {
                CorrectedVelocity.x = 0f;
                CorrectedVelocity.z = 0f;
            }     
        }

        // 5. Apply Position Correction
        if (HasPositionContact && MaxPenetrationDepth > DepthEpsilon)
        {
            if (MaxPenetrationDepth > MaxDepth) MaxPenetrationDepth = MaxDepth;

            float EffectiveDepth = MaxPenetrationDepth - TargetPenetration;
            if (EffectiveDepth < 0f) EffectiveDepth = 0f;

            DbVector3 PositionCorrection = Mul(BestPositionNormal, EffectiveDepth * CorrectionFactor);
            CharacterLocal.Position = Add(CharacterLocal.Position, PositionCorrection);
        }

        CharacterLocal.IsColliding = Contacts.Count > 0;
        CharacterLocal.CorrectedVelocity = CorrectedVelocity;
        CharacterLocal.KinematicInformation.Grounded = CharacterLocal.KinematicInformation.Grounded || IsGrounded;
    }



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


    public static bool SolveGjkDistance(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, out GjkDistanceResult Result, int MaxIterations = 32)
    {
        List<GjkVertex> Simplex = new List<GjkVertex>(4);

        DbVector3 InitialDirection = new(0.7f, 0.4f, 0.6f);
        DbVector3 SearchDirection = Normalize(InitialDirection);

        GjkVertex InitialVertex = SupportPairWorld(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection);
        float InitialDot = Dot(InitialVertex.MinkowskiPoint, SearchDirection);

        if (InitialDot <= 0f)
        {
            ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection);

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

        Simplex.Add(InitialVertex);
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
                ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection);

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
                ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection);

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

        ComputeSeparationOnAxis(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, SearchDirection, out float FinalDistance, out DbVector3 FinalPointOnA, out DbVector3 FinalPointOnB, out DbVector3 FinalSeparationDirection);

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


    static void ComputeSeparationOnAxis(List<ConvexHullCollider> ColliderA, DbVector3 PositionA, float YawRadiansA, List<ConvexHullCollider> ColliderB, DbVector3 PositionB, float YawRadiansB, DbVector3 Axis, out float Distance, out DbVector3 PointOnA, out DbVector3 PointOnB, out DbVector3 SeparationDirection)
    {
        DbVector3 Direction = Axis;
        if (!NearZero(Direction))
        {
            Direction = Normalize(Direction);
        }
        else
        {
            Direction = new DbVector3(0f, 0f, 1f);
        }

        DbVector3 SupportA = SupportWorldComplex(ColliderA, PositionA, YawRadiansA, Direction);
        DbVector3 SupportB = SupportWorldComplex(ColliderB, PositionB, YawRadiansB, Negate(Direction));

        float DistanceA = Dot(SupportA, Direction);
        float DistanceB = Dot(SupportB, Direction);

        float Gap = DistanceB - DistanceA;
        if (Gap < 0f) Gap = 0f;

        Distance = Gap;
        PointOnA = SupportA;
        PointOnB = SupportB;

        SeparationDirection = Direction;
    }

    static DbVector3 GetColliderCenterWorld(ComplexCollider collider, DbVector3 position, float yawRadians)
    {
        DbVector3 rotatedCenter = RotateAroundYAxis(collider.CenterPoint, yawRadians);
        return Add(position, rotatedCenter);
    }


}
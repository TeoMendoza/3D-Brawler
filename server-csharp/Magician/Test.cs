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
        int MaxSubsteps = 10;
        float CcdSlop = 0.025f;
        float WonkySeparationThreshold = 0.01f;

        foreach (var CharacterRow in Ctx.Db.magician.Iter())
        {
            Magician Character = CharacterRow;

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

                DbVector3 EarliestSeparationDirection = default;
                bool HasEarliestSeparationDirection = false;

                List<CollisionContact> ContactsThisStep = new List<CollisionContact>();
                List<CollisionEntry> ContactEntriesThisStep = new List<CollisionEntry>();

                DbVector3 PositionAStart = Character.Position;
                float YawRadiansAStart = ToRadians(Character.Rotation.Yaw);
                foreach (CollisionEntry CollisionEntry in Character.CollisionEntries)
                {
                    if (ResolvedEntriesThisTick.Contains(CollisionEntry)) continue;

                    bool EntryHasContact = TryBuildContactForEntry(Ctx, ref Character, CollisionEntry, ContactsThisStep);
                    if (EntryHasContact)
                    {
                        ContactEntriesThisStep.Add(CollisionEntry);
                        continue;
                    }

                    DbVector3 PositionA = PositionAStart;
                    DbVector3 VelocityA = CurrentStepVelocity;
                    float YawRadiansA = YawRadiansAStart;

                    DbVector3 PositionB;
                    float YawRadiansB;
                    List<ConvexHullCollider> ColliderA;
                    List<ConvexHullCollider> ColliderB;

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

                            GjkDistanceResult DistanceResultMagician;
                            bool DistanceSolvedMagician = SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out DistanceResultMagician);
                            if (DistanceSolvedMagician is false) break;

                            float SeparationDistanceMagician = DistanceResultMagician.Distance;
                            DbVector3 SeparationDirectionMagician = DistanceResultMagician.SeparationDirection;

                            if (Character.Id == 1) {
                                Log.Info($"Position A: {PositionA}, Position B: {PositionB}, Seperation Direction: {SeparationDirectionMagician}, Seperation Distance: {SeparationDistanceMagician}, Velocity A: {VelocityA}, Velocity B: {VelocityB}");
                            }

                            DbVector3 FromCharacterToOther = Sub(PositionB, PositionA);
                            if (Dot(SeparationDirectionMagician, FromCharacterToOther) < 0f) SeparationDirectionMagician = Negate(SeparationDirectionMagician);

                            DbVector3 RelativeVelocityMagician = Sub(VelocityA, VelocityB);
                            float RelativeSpeedSquaredMagician = Dot(RelativeVelocityMagician, RelativeVelocityMagician);
                            if (RelativeSpeedSquaredMagician < 1e-6f) break;

                            float ClosingSpeedMagician = Dot(RelativeVelocityMagician, SeparationDirectionMagician);
                            if (ClosingSpeedMagician <= 0f) break;

                            float CandidateHitTimeMagician = (SeparationDistanceMagician + CcdSlop) / ClosingSpeedMagician;
                            if (CandidateHitTimeMagician < 0f) break;
                            if (CandidateHitTimeMagician > RemainingTime) break;

                            if (HasEarliestHit is false || CandidateHitTimeMagician < EarliestCollisionTime)
                            {
                                HasEarliestHit = true;
                                EarliestCollisionTime = CandidateHitTimeMagician;
                                EarliestCollisionEntry = CollisionEntry;
                                EarliestSeparationDirection = SeparationDirectionMagician;
                                HasEarliestSeparationDirection = true;
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
                            bool DistanceSolvedMap = SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out DistanceResultMap);
                            if (DistanceSolvedMap is false) break;

                            float SeparationDistanceMap = DistanceResultMap.Distance;
                            DbVector3 SeparationDirectionMap = DistanceResultMap.SeparationDirection;

                            DbVector3 FromCharacterToMap = Sub(PositionB, PositionA);
                            if (Dot(SeparationDirectionMap, FromCharacterToMap) < 0f) SeparationDirectionMap = Mul(SeparationDirectionMap, -1f);

                            DbVector3 RelativeVelocityMap = VelocityA;
                            float RelativeSpeedSquaredMap = Dot(RelativeVelocityMap, RelativeVelocityMap);
                            if (RelativeSpeedSquaredMap < 1e-6f) break;

                            float ClosingSpeedMap = Dot(RelativeVelocityMap, SeparationDirectionMap);
                            if (ClosingSpeedMap <= 0f) break;

                            float CandidateHitTimeMap = (SeparationDistanceMap + CcdSlop) / ClosingSpeedMap;
                            if (CandidateHitTimeMap < 0f) break;
                            if (CandidateHitTimeMap > RemainingTime) break;

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
                    ResolveContacts(ref Character, ContactsThisStep, CurrentStepVelocity);

                    foreach (CollisionEntry ContactEntry in ContactEntriesThisStep)
                    {
                        if (ResolvedEntriesThisTick.Contains(ContactEntry) is false) ResolvedEntriesThisTick.Add(ContactEntry);
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
                ContactEntriesThisStep.Clear();

                bool HasContactAfterMove = false;
                HasContactAfterMove = TryBuildContactForEntry(Ctx, ref Character, EarliestCollisionEntry, ContactsThisStep);
                if (HasContactAfterMove)
                {
                    ContactEntriesThisStep.Add(EarliestCollisionEntry);
                    ResolveContacts(ref Character, ContactsThisStep, CollisionStepVelocity);
                    if (ResolvedEntriesThisTick.Contains(EarliestCollisionEntry) is false) ResolvedEntriesThisTick.Add(EarliestCollisionEntry);
                }
            }

            DbVector3 FinalStepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;
            AdjustGrounded(Ctx, FinalStepVelocity, ref Character);

            Ctx.Db.magician.identity.Update(Character);
        }
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

            GjkResult GjkResultMagician;
            bool IntersectsMagician = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResultMagician);
            if (IntersectsMagician is false) return false;

            DbVector3 GjkNormal = Negate(GjkResultMagician.LastDirection);
            DbVector3 ContactNormal = ComputeContactNormal(GjkNormal, PositionA, PositionB);
            float PenetrationDepth = ComputePenetrationDepthApprox(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, ContactNormal);

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

            GjkResult GjkResultMap;
            bool IntersectsMap = SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResultMap);
            if (IntersectsMap is false) return false;

            DbVector3 GjkNormal = Negate(GjkResultMap.LastDirection);
            DbVector3 ContactNormal = ComputeContactNormal(GjkNormal, PositionA, PositionB);
            float PenetrationDepth = ComputePenetrationDepthApprox(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, ContactNormal);

            Contacts.Add(new CollisionContact(ContactNormal, PenetrationDepth));
            return true;
        }

        return false;
    }

    public static void ResolveContacts(ref Magician CharacterLocal, List<CollisionContact> Contacts, DbVector3 InputVelocity)
    {
        DbVector3 WorldUp = new(0f, 1f, 0f);
        float MinGroundDot = MathF.Cos(ToRadians(50f));
        float AxisEpsilon = 1e-3f;
        float DepthEpsilon = 1e-4f;
        float MaxDepth = 0.25f;
        float CorrectionFactor = 1f;
        float TargetPenetration = 0.005f;

        DbVector3 CorrectedVelocity = InputVelocity;

        bool IsGrounded = false;
        float MaxPenetrationDepth = 0f;
        DbVector3 BestPositionNormal = WorldUp;
        bool HasPositionContact = false;

        foreach (CollisionContact Contact in Contacts)
        {
            DbVector3 ContactNormalWorld = Contact.Normal;
            DbVector3 PositionNormal = ContactNormalWorld;

            float UpDot = Dot(ContactNormalWorld, WorldUp);

            if (UpDot > 1f - AxisEpsilon)
            {
                ContactNormalWorld = WorldUp;
                UpDot = 1f;
            }

            bool IsWalkable = UpDot >= MinGroundDot && UpDot > 0f;
            if (IsWalkable) IsGrounded = true;

            if (IsWalkable is false)
            {
                ContactNormalWorld.y = 0f;
                ContactNormalWorld = Normalize(ContactNormalWorld);
            }

            float NormalVelocityComponent = Dot(ContactNormalWorld, CorrectedVelocity);
            if (NormalVelocityComponent < 0f) CorrectedVelocity = Sub(CorrectedVelocity, Mul(ContactNormalWorld, NormalVelocityComponent));

            float PenetrationDepth = Contact.PenetrationDepth;
            if (PenetrationDepth > MaxPenetrationDepth)
            {
                MaxPenetrationDepth = PenetrationDepth;
                BestPositionNormal = PositionNormal;
                HasPositionContact = true;
            }
        }

        if (IsGrounded && CorrectedVelocity.y < 0f) CorrectedVelocity.y = 0f;

        if (IsGrounded && InputVelocity.y <= 0f)
            CharacterLocal.Velocity = new DbVector3(CharacterLocal.Velocity.x, 0f, CharacterLocal.Velocity.z);

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
        CharacterLocal.KinematicInformation.Grounded = IsGrounded;
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

        DbVector3 InitialDirection = Sub(PositionB, PositionA);
        if (NearZero(InitialDirection))
        {
            InitialDirection = new DbVector3(0f, 0f, 1f);
        }

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

        DbVector3 RawSeparation = Sub(PointOnB, PointOnA);
        if (!NearZero(RawSeparation))
        {
            SeparationDirection = Normalize(RawSeparation);
        }
        else
        {
            SeparationDirection = Direction;
        }
    }


}




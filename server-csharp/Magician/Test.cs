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
    float CcdSlop = 1e-3f;

    foreach (var Charac in Ctx.Db.magician.Iter())
    {
        var Character = Charac;

        Character.KinematicInformation.Grounded = false;
        Character.IsColliding = false;
        Character.CorrectedVelocity = Character.Velocity;

        float RemainingTime = TickTime;
        int SubstepCount = 0;

        List<CollisionEntry> ResolvedEntriesThisTick = new List<CollisionEntry>();

        while (RemainingTime > MinTimeStep && SubstepCount < MaxSubsteps)
        {
            SubstepCount += 1;

            bool HasOverlap = false;
            bool HasHit = false;

            float EarliestTime = RemainingTime;
            CollisionEntry EarliestEntry = default;

            DbVector3 StepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;

            List<CollisionEntry> OverlappingEntries = new List<CollisionEntry>();

            foreach (var Entry in Character.CollisionEntries)
            {
                if (ResolvedEntriesThisTick.Contains(Entry)) continue;

                DbVector3 PositionA = Character.Position;
                DbVector3 VelocityA = StepVelocity;
                float YawRadiansA = ToRadians(Character.Rotation.Yaw);

                DbVector3 PositionB;
                DbVector3 VelocityB;
                float YawRadiansB;
                List<ConvexHullCollider> ColliderA;
                List<ConvexHullCollider> ColliderB;

                switch (Entry.Type)
                {
                    case CollisionEntryType.Magician:
                    {
                        Magician Other = Ctx.Db.magician.Id.Find(Entry.Id) ?? throw new Exception("Colliding Magician Not Found");
                        if (Other.Id == Character.Id) continue;

                        ColliderA = Character.GjkCollider.ConvexHulls;
                        ColliderB = Other.GjkCollider.ConvexHulls;

                        PositionB = Other.Position;
                        VelocityB = Other.IsColliding ? Other.CorrectedVelocity : Other.Velocity;
                        YawRadiansB = ToRadians(Other.Rotation.Yaw);

                        // 1) Authoritative overlap check
                        bool IntersectsExact = SolveGjk(
                            ColliderA, PositionA, YawRadiansA,
                            ColliderB, PositionB, YawRadiansB,
                            out GjkResult MagicianGjkResult
                        );

                        if (IntersectsExact)
                        {
                            // Already overlapping, go through overlap/contact path, no CCD
                            HasOverlap = true;
                            OverlappingEntries.Add(Entry);
                            continue;
                        }

                        // 2) Not overlapping → distance + CCD TOI
                        if (!SolveGjkDistance(
                                ColliderA, PositionA, YawRadiansA,
                                ColliderB, PositionB, YawRadiansB,
                                out GjkDistanceResult MagicianDistanceResult))
                        {
                            continue;
                        }

                        float SeparationDistanceMagician = MagicianDistanceResult.Distance;
                        DbVector3 SeparationDirectionMagician = MagicianDistanceResult.SeparationDirection;

                        // Orient separation dir from A → B in world space
                        DbVector3 FromMagicianToOther = Sub(PositionB, PositionA);
                        if (Dot(SeparationDirectionMagician, FromMagicianToOther) < 0f)
                        {
                            SeparationDirectionMagician = Mul(SeparationDirectionMagician, -1f);
                        }

                        DbVector3 RelativeVelocityMagician = Sub(VelocityA, VelocityB);
                        float ClosingMagician = Dot(RelativeVelocityMagician, SeparationDirectionMagician);
                        if (ClosingMagician <= 0f) continue;

                        float TimeToHitMagician = (SeparationDistanceMagician + CcdSlop) / ClosingMagician;
                        if (TimeToHitMagician < 0f || TimeToHitMagician > RemainingTime) continue;

                        if (!HasHit || TimeToHitMagician < EarliestTime)
                        {
                            HasHit = true;
                            EarliestTime = TimeToHitMagician;
                            EarliestEntry = Entry;
                        }

                        break;
                    }


                    case CollisionEntryType.Map:
                    {
                        Map Map = Ctx.Db.Map.Id.Find(Entry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

                        ColliderA = Character.GjkCollider.ConvexHulls;
                        ColliderB = Map.GjkCollider.ConvexHulls;

                        PositionB = new DbVector3(0f, 0f, 0f);
                        VelocityB = new DbVector3(0f, 0f, 0f);
                        YawRadiansB = 0f;

                        if (!SolveGjkDistance(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkDistanceResult MapDistanceResult)) continue;

                        if (MapDistanceResult.Intersects)
                        {
                            HasOverlap = true;
                            OverlappingEntries.Add(Entry);
                            continue;
                        }

                        float SeparationDistanceMap = MapDistanceResult.Distance;
                        DbVector3 SeparationDirectionMap = MapDistanceResult.SeparationDirection;

                        DbVector3 FromCharacterToMap = Sub(PositionB, PositionA);
                        if (Dot(SeparationDirectionMap, FromCharacterToMap) < 0f)
                        {
                            SeparationDirectionMap = Mul(SeparationDirectionMap, -1f);
                        }

                        DbVector3 RelativeVelocityMap = Sub(VelocityA, VelocityB);
                        float ClosingMap = Dot(RelativeVelocityMap, SeparationDirectionMap);
                        if (ClosingMap <= 0f) continue;

                        float TimeToHitMap = (SeparationDistanceMap + CcdSlop) / ClosingMap;
                        if (TimeToHitMap < 0f || TimeToHitMap > RemainingTime) continue;

                        if (!HasHit || TimeToHitMap < EarliestTime)
                        {
                            HasHit = true;
                            EarliestTime = TimeToHitMap;
                            EarliestEntry = Entry;
                        }

                        break;
                    }

                    default:
                        break;
                }
            }

            if (HasOverlap)
            {
                List<ContactEPA> OverlapContacts = [];
                BuildContacts(Ctx, ref Character, OverlapContacts);

                ResolveContacts(ref Character, OverlapContacts, StepVelocity);

                if (OverlapContacts.Count > 0)
                {
                    foreach (var Entry in OverlappingEntries)
                    {
                        if (!ResolvedEntriesThisTick.Contains(Entry)) ResolvedEntriesThisTick.Add(Entry);
                    }
                }

                continue;
            }

            if (!HasHit)
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

            DbVector3 HitStepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;

            Character.Position = new DbVector3(
                Character.Position.x + HitStepVelocity.x * EarliestTime,
                Character.Position.y + HitStepVelocity.y * EarliestTime,
                Character.Position.z + HitStepVelocity.z * EarliestTime
            );

            RemainingTime -= EarliestTime;

            List<ContactEPA> HitContacts = [];
            BuildContacts(Ctx, ref Character, HitContacts);

            ResolveContacts(ref Character, HitContacts, HitStepVelocity);

            if (HitContacts.Count > 0 && !ResolvedEntriesThisTick.Contains(EarliestEntry))
            {
                ResolvedEntriesThisTick.Add(EarliestEntry);
            }
        }

        DbVector3 FinalVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;
        AdjustGrounded(Ctx, FinalVelocity, ref Character);

        Ctx.Db.magician.identity.Update(Character);
    }
}




    public static void BuildContacts(ReducerContext LocalCtx, ref Magician CharacterLocal, List<ContactEPA> Contacts)
    {
        foreach (var Entry in CharacterLocal.CollisionEntries)
        {
            switch (Entry.Type)
            {
                case CollisionEntryType.Magician:
                {
                    Magician Player = LocalCtx.Db.magician.Id.Find(Entry.Id) ?? throw new Exception("Colliding Magician Not Found");

                    if (Player.Id == CharacterLocal.Id) break;

                    var ColliderA = CharacterLocal.GjkCollider.ConvexHulls;
                    var PositionA = CharacterLocal.Position;
                    float YawRadiansA = ToRadians(CharacterLocal.Rotation.Yaw);

                    var ColliderB = Player.GjkCollider.ConvexHulls;
                    var PositionB = Player.Position;
                    float YawRadiansB = ToRadians(Player.Rotation.Yaw);

                    if (SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResultMagician))
                    {
                        DbVector3 GjkNormal = Negate(GjkResultMagician.LastDirection);
                        DbVector3 Normal = ComputeContactNormal(GjkNormal, PositionA, PositionB);
                        float PenetrationDepth = ComputePenetrationDepthApprox(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, Normal);

                        Contacts.Add(new ContactEPA(Normal, PenetrationDepth));
                    }

                    break;
                }

                case CollisionEntryType.Map:
                {
                    Map Map = LocalCtx.Db.Map.Id.Find(Entry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

                    var MapColliderA = CharacterLocal.GjkCollider.ConvexHulls;
                    var MapPositionA = CharacterLocal.Position;
                    float MapYawRadiansA = ToRadians(CharacterLocal.Rotation.Yaw);

                    var MapColliderB = Map.GjkCollider.ConvexHulls;
                    var MapPositionB = new DbVector3(0f, 0f, 0f);
                    float MapYawRadiansB = 0f;

                    if (SolveGjk(MapColliderA, MapPositionA, MapYawRadiansA, MapColliderB, MapPositionB, MapYawRadiansB, out GjkResult MapGjkResult))
                    {
                        DbVector3 GjkNormal = Negate(MapGjkResult.LastDirection);
                        DbVector3 Normal = ComputeContactNormal(GjkNormal, MapPositionA, MapPositionB);
                        float PenetrationDepth = ComputePenetrationDepthApprox(MapColliderA, MapPositionA, MapYawRadiansA, MapColliderB, MapPositionB, MapYawRadiansB, Normal);

                        Contacts.Add(new ContactEPA(Normal, PenetrationDepth));
                    }

                    break;
                }

                default:
                    break;
            }
        }
    }

    public static void ResolveContacts(ref Magician CharacterLocal, List<ContactEPA> Contacts, DbVector3 InputVelocity)
    {
        DbVector3 WorldUp = new DbVector3(0f, 1f, 0f);
        float MinGroundDot = MathF.Cos(ToRadians(50f));
        float AxisEpsilon = 1e-3f;
        float DepthEpsilon = 1e-4f;
        float MaxDepth = 0.25f;
        float CorrectionFactor = 0.4f;

        DbVector3 CorrectedVelocity = InputVelocity;

        bool IsGrounded = false;
        float MaxPenetrationDepth = 0f;
        DbVector3 BestPositionNormal = WorldUp;
        bool HasPositionContact = false;

        foreach (ContactEPA Contact in Contacts)
        {
            DbVector3 Normal = Contact.Normal;
            DbVector3 PositionNormal = Normal;

            float UpDot = Dot(Normal, WorldUp);

            if (UpDot > 1f - AxisEpsilon)
            {
                Normal = WorldUp;
                UpDot = 1f;
            }

            bool IsWalkable = UpDot >= MinGroundDot && UpDot > 0f;

            if (IsWalkable)
            {
                IsGrounded = true;
            }
            else
            {
                Normal.y = 0f;
                Normal = Normalize(Normal);
            }

            float Direction = Dot(Normal, CorrectedVelocity);
            if (Direction < 0f)
            {
                CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, Direction));
            }

            float Depth = Contact.PenetrationDepth;
            if (Depth > MaxPenetrationDepth)
            {
                MaxPenetrationDepth = Depth;
                BestPositionNormal = PositionNormal;
                HasPositionContact = true;
            }
        }

        if (IsGrounded && CorrectedVelocity.y < 0f)
        {
            CorrectedVelocity.y = 0f;
        }

        if (IsGrounded && InputVelocity.y <= 0f)
        {
            CharacterLocal.Velocity = new DbVector3(CharacterLocal.Velocity.x, 0f, CharacterLocal.Velocity.z);
        }

        if (HasPositionContact && MaxPenetrationDepth > DepthEpsilon)
        {
            if (MaxPenetrationDepth > MaxDepth) MaxPenetrationDepth = MaxDepth;

            DbVector3 PositionCorrection = Mul(BestPositionNormal, MaxPenetrationDepth * CorrectionFactor);
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

        DbVector3 SupportA = SupportWorldComplex(ColliderA, PositionA, YawRadiansA, Negate(Direction));
        DbVector3 SupportB = SupportWorldComplex(ColliderB, PositionB, YawRadiansB, Direction);

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




using System.Diagnostics.Contracts;
using System.Numerics;
using System;
using SpacetimeDB;

public static partial class Module
{
    [Reducer]
    public static void HandleMovementRequestMagician(ReducerContext ctx, MovementRequest Request)
    {
        Magician Character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Magician To Move Not Found");

        Character.Rotation = Request.Aim;

        Character.Velocity = new DbVector3(0f, Character.Velocity.y, 0f);

        if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanWalk").Subscribers.Count == 0)
        {
            float LocalX = 0f;
            float LocalZ = 0f;

            if (Request.MoveForward && !Request.MoveBackward) LocalZ = 2f;
            else if (Request.MoveBackward && !Request.MoveForward) LocalZ = -1.5f;

            if (Request.MoveRight && !Request.MoveLeft) LocalX = 1.5f;
            else if (Request.MoveLeft && !Request.MoveRight) LocalX = -1.5f;

            if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint && Request.MoveForward && !Request.MoveBackward)
                LocalZ *= 2f;
            
            if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint)
                LocalX *= 1.25f;

            float YawRadians = ToRadians(Character.Rotation.Yaw);
            float CosYaw = MathF.Cos(YawRadians);
            float SinYaw = MathF.Sin(YawRadians);

            float WorldX = CosYaw * LocalX + SinYaw * LocalZ;
            float WorldZ = -SinYaw * LocalX + CosYaw * LocalZ;

            Character.Velocity = new DbVector3(WorldX, Character.Velocity.y, WorldZ);
        }

        if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanJump").Subscribers.Count == 0 && Request.Jump)
        {
            Character.Velocity.y = 7.5f;
        }

        if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanCrouch").Subscribers.Count == 0 && Request.Crouch)
        {
            Character.Velocity = new DbVector3(Character.Velocity.x * 0.5f, Character.Velocity.y, Character.Velocity.z * 0.5f);
            Character.KinematicInformation.Crouched = true;
            AddSubscriberUnique(GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");
        }

        if (Request.Crouch is false)
        {
            Character.KinematicInformation.Crouched = false;
            RemoveSubscriber(GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");
        }

        if (Character.Id != 10000) // Testing To Make Sure Moving Collisions Work
            ctx.Db.magician.identity.Update(Character);
    }


    [Reducer]
    public static void MoveMagicians2(ReducerContext ctx, Move_All_Magicians_Timer timer) // Not Used Currently
    {   
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.magician.Iter())
        {
            var character = charac;

            DbVector3 MoveVelocity = character.IsColliding ? character.CorrectedVelocity : character.Velocity;
            character.Position = new DbVector3(character.Position.x + MoveVelocity.x * time, character.Position.y + MoveVelocity.y * time, character.Position.z + MoveVelocity.z * time);

            //AdjustCollider(ctx, ref character);
            AdjustGrounded(ctx, MoveVelocity, ref character);

            List<CollisionContact> Contacts = [];
            List<CollisionEntry> EntriesToRemove = [];
            foreach (var Entry in character.CollisionEntries)
            {
                switch (Entry.Type)
                {
                    case CollisionEntryType.Magician:
                        
                        Magician Player = ctx.Db.magician.Id.Find(Entry.Id) ?? throw new Exception("Colliding Magician Not Found");
                        
                        var ColliderA = character.GjkCollider.ConvexHulls;
                        var PositionA = character.Position;
                        float YawRadiansA = ToRadians(character.Rotation.Yaw);

                        var ColliderB = Player.GjkCollider.ConvexHulls;
                        var PositionB = Player.Position;
                        float YawRadiansB = ToRadians(Player.Rotation.Yaw);

                        if (Player.Id != character.Id && SolveGjk(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, out GjkResult GjkResult))
                        {    
                            DbVector3 GjkNormal = Negate(GjkResult.LastDirection);
                            DbVector3 Normal = ComputeContactNormal(GjkNormal, GjkNormal, GjkNormal);
                            float PenetrationDepth = ComputePenetrationDepthApprox(ColliderA, PositionA, YawRadiansA, ColliderB, PositionB, YawRadiansB, Normal);

                            Contacts.Add(new CollisionContact(Normal, PenetrationDepth, CollisionEntryType.Magician));
                            
                        }
                        break;

                    case CollisionEntryType.Map:
                        Map Map = ctx.Db.Map.Id.Find(Entry.Id) ?? throw new Exception("Colliding Map Piece Not Found");

                        var MapColliderA = character.GjkCollider.ConvexHulls;
                        var MapPositionA = character.Position;
                        float MapYawRadiansA = ToRadians(character.Rotation.Yaw);

                        var MapColliderB = Map.GjkCollider.ConvexHulls;
                        var MapPositionB = new DbVector3(0,0,0);
                        float MapYawRadiansB = 0f;

                        if (SolveGjk(MapColliderA, MapPositionA, MapYawRadiansA, MapColliderB, MapPositionB, MapYawRadiansB, out GjkResult MapGjkResult))
                        {
                            DbVector3 GjkNormal = Negate(MapGjkResult.LastDirection);
                            DbVector3 Normal = ComputeContactNormal(GjkNormal, GjkNormal, GjkNormal);
                            float PenetrationDepth = ComputePenetrationDepthApprox(MapColliderA, MapPositionA, MapYawRadiansA, MapColliderB, MapPositionB, MapYawRadiansB, Normal);

                            Contacts.Add(new CollisionContact(Normal, PenetrationDepth, CollisionEntryType.Map)); 
                        }
                        break;

                    default:
                        break;
                }
            }
            
            character.CollisionEntries.RemoveAll(CollisionEntry => EntriesToRemove.Contains(CollisionEntry));

            DbVector3 WorldUp = new(0f, 1f, 0f);
            float MinGroundDot = MathF.Cos(ToRadians(50f));
            float AxisEpsilon = 1e-3f;
            float DepthEpsilon = 1e-4f;
            float MaxDepth = 0.25f;
            float CorrectionFactor = 0.4f;

            DbVector3 InputVelocity = character.Velocity;
            DbVector3 CorrectedVelocity = InputVelocity;

            bool IsGrounded = false;
            float MaxPenetrationDepth = 0f;
            DbVector3 BestPositionNormal = WorldUp;
            bool HasPositionContact = false;

            foreach (CollisionContact Contact in Contacts)
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
                character.Velocity.y = 0f;
            }

            if (HasPositionContact && MaxPenetrationDepth > DepthEpsilon)
            {
                if (MaxPenetrationDepth > MaxDepth) MaxPenetrationDepth = MaxDepth;

                DbVector3 PositionCorrection = Mul(BestPositionNormal, MaxPenetrationDepth * CorrectionFactor);
                character.Position = Add(character.Position, PositionCorrection);
            }

            character.IsColliding = Contacts.Count > 0;
            character.CorrectedVelocity = CorrectedVelocity;
            character.KinematicInformation.Grounded = IsGrounded;

            ctx.Db.magician.identity.Update(character);
        }
    }

    [Reducer]
    public static void ApplyGravityMagician(ReducerContext ctx, Gravity_Timer_Magician timer)
    {
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.magician.Iter())
        {
            var character = charac;
            character.Velocity.y = character.Velocity.y > -10f ? character.Velocity.y -= timer.gravity * time : -10f;
            ctx.Db.magician.identity.Update(character);
        }
    }


    [Reducer]
    public static void HandleActionChangeRequestMagician(ReducerContext ctx, ActionRequestMagician request)
    {
        Identity identity = ctx.Sender;
        Magician character = ctx.Db.magician.identity.Find(identity) ?? throw new Exception("Magician Not Found");
        MagicianState oldState = character.State;

        bool StateSwitched = false;
        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers.Count == 0 && request.State == MagicianState.Attack)
        {
            character.State = MagicianState.Attack;
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
            StateSwitched = true;
        }

        else if (request.State == MagicianState.Default)
        {
            character.State = MagicianState.Default;
            StateSwitched = true;
        }

        if (StateSwitched)
        {
            switch (oldState)
            {
                case MagicianState.Attack:
                    RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
                    break;
                case MagicianState.Default:
                    break;
            }
        }

        ctx.Db.magician.identity.Update(character);
    }

    [Reducer]
    public static void SpawnThrowingCard(ReducerContext ctx,  DbVector3 CameraPositionOffset, float CameraYawOffset, float CameraPitchOffset, DbVector3 HandPositionOffset, float MaxDistance)
    {
        var Magician = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Owner not found");
        DbVector3 MagicianPosition = Magician.Position;

        float MagYawRad = ToRadians(Magician.Rotation.Yaw);
        float MagPitchRad = ToRadians(Magician.Rotation.Pitch);
        Quaternion MagicianYawOnly = Quaternion.CreateFromYawPitchRoll(MagYawRad, 0f, 0f);

        DbVector3 HandPosition = Add(MagicianPosition, Rotate(HandPositionOffset, MagicianYawOnly));

        float TotalYawRad = ToRadians(Magician.Rotation.Yaw + CameraYawOffset);
        float TotalPitchRad = ToRadians(Magician.Rotation.Pitch + CameraPitchOffset);
        Quaternion CameraRotation = Quaternion.CreateFromYawPitchRoll(TotalYawRad, TotalPitchRad, 0f);

        DbVector3 CameraPosition = Add(MagicianPosition, Rotate(CameraPositionOffset, CameraRotation));

        DbVector3 BaseCameraForward = new(0f, 0f, 1f);
        DbVector3 CameraForward = Normalize(Rotate(BaseCameraForward, CameraRotation));

        DbVector3 ThrowingCardTarget = RaycastFromCamera(ctx, Magician, new Raycast(CameraPosition, CameraForward, MaxDistance));
        DbVector3 ThrowingCardDirection = Normalize(Sub(ThrowingCardTarget, HandPosition));

        ctx.Db.throwing_cards.Insert(new ThrowingCard
        {
            OwnerIdentity = Magician.identity,
            MatchId = Magician.MatchId,
            position = HandPosition,
            direction = ThrowingCardDirection,
            velocity = Mul(ThrowingCardDirection, 50f),
            Collider = new CapsuleCollider { Center = HandPosition, Direction = ThrowingCardDirection, HeightEndToEnd = 0.5f, Radius = 0.05f }, // 0.1f, 0.025f Original, extended to try and compensate for speed
        });
    }

    [Reducer]
    public static void MoveThrowingCards(ReducerContext ctx, Move_ThrowingCards_Timer timer)
    {
        var time = timer.tick_rate;
        foreach (ThrowingCard throwingCard in ctx.Db.throwing_cards.Iter())
        {
            var ThrowingCard = throwingCard;
            ThrowingCard.position = new DbVector3(
            ThrowingCard.position.x + ThrowingCard.velocity.x * time,
            ThrowingCard.position.y + ThrowingCard.velocity.y * time,
            ThrowingCard.position.z + ThrowingCard.velocity.z * time
            );

            ThrowingCard.Collider.Center = Add(ThrowingCard.position, Mul(ThrowingCard.Collider.Direction, ThrowingCard.Collider.HeightEndToEnd * 0.5f));
            ctx.Db.throwing_cards.Id.Update(ThrowingCard);
        }
    }
    
    [Reducer]
    public static void AddCollisionEntryMagician(ReducerContext ctx, CollisionEntry Entry, Identity TargetIdentity)
    {
        Magician Magician = ctx.Db.magician.identity.Find(TargetIdentity) ?? throw new Exception("Magician (Sender) Not Found");
        if (Magician.CollisionEntries.Contains(Entry) is false) Magician.CollisionEntries.Add(Entry);
        ctx.Db.magician.identity.Update(Magician);
    }

    [Reducer]
    public static void RemoveCollisionEntryMagician(ReducerContext ctx, CollisionEntry Entry, Identity TargetIdentity)
    {
        Magician Magician = ctx.Db.magician.identity.Find(TargetIdentity) ?? throw new Exception("Magician (Sender) Not Found");
        if (Magician.CollisionEntries.Contains(Entry) is true) Magician.CollisionEntries.Remove(Entry);
        ctx.Db.magician.identity.Update(Magician);
    }
    
}




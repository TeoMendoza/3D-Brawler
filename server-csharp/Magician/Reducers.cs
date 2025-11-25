using System.Diagnostics.Contracts;
using System.Numerics;
using System;
using SpacetimeDB;

public static partial class Module
{
    [Reducer]
    public static void HandleMovementRequestMagician(ReducerContext ctx, MovementRequest Request)
    {
        Magician character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Magician To Move Not Found");
        character.Rotation = Request.Aim;

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanWalk").Subscribers.Count == 0)
        {
            float YawRotation = ToRadians(character.Rotation.Yaw);
            character.Velocity = new DbVector3(MathF.Cos(YawRotation) * Request.Velocity.x + MathF.Sin(YawRotation) * Request.Velocity.z, character.Velocity.y, -MathF.Sin(YawRotation) * Request.Velocity.x + MathF.Cos(YawRotation) * Request.Velocity.z);
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers.Count == 0 && Request.Jump)
        {
            character.Velocity.y = 7.5f;
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanCrouch").Subscribers.Count == 0 && Request.Crouch)
        {
            character.Velocity = new DbVector3(character.Velocity.x * 0.5f, character.Velocity.y, character.Velocity.z * 0.5f);
            character.KinematicInformation.Crouched = true;
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");            
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint)
        {
            character.Velocity = new DbVector3(character.Velocity.x * 2f, character.Velocity.y, character.Velocity.z * 2f);
        }

        if (Request.Crouch is false)
        {
            character.KinematicInformation.Crouched = false;
            RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");
        }

        ctx.Db.magician.identity.Update(character);
    }


    [Reducer]
    public static void MoveMagicians(ReducerContext ctx, Move_All_Magicians_Timer timer)
    {   
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.magician.Iter())
        {
            var character = charac;

            DbVector3 MoveVelocity = character.IsColliding ? character.CorrectedVelocity : character.Velocity;
            character.Position = new DbVector3(character.Position.x + MoveVelocity.x * time, character.Position.y + MoveVelocity.y * time, character.Position.z + MoveVelocity.z * time);

            AdjustCollider(ctx, charac);
            AdjustGrounded(ctx, MoveVelocity, ref character);

            character.IsColliding = false;
            if (character.CollisionEntries.Count > 0)
            {
                List<ContactEPA> Contacts = [];
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
                                DbVector3 Normal = ComputeContactNormal(GjkNormal, PositionA, PositionB);
                                Contacts.Add(new ContactEPA(Normal));
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
                                DbVector3 Normal = ComputeContactNormal(GjkNormal, MapPositionA, MapPositionB);
                                Contacts.Add(new ContactEPA(Normal)); 
                            }
                            break;

                        case CollisionEntryType.ThrowingCard: // Switch To GJK - Also, Need To Remove Collision Entry From All Other Players In Match Aswell, Not Just Hit Target
                            ThrowingCard ThrowingCard = ctx.Db.throwing_cards.Id.Find(Entry.Id) ?? throw new Exception("Colliding Bullet Not Found");
                            if (ThrowingCard.OwnerIdentity != character.identity && TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(ThrowingCard.Collider), ThrowingCard.Collider, out Contact _contact))
                            {
                                ctx.Db.throwing_cards.Id.Delete(ThrowingCard.Id);
                                if (character.CollisionEntries.Contains(Entry) is true) EntriesToRemove.Add(Entry);
                            }
                            break;

                        default:
                            break;
                    }
                }
                
                character.CollisionEntries.RemoveAll(CollisionEntry => EntriesToRemove.Contains(CollisionEntry));

                DbVector3 WorldUp = new(0f, 1f, 0f);
                float MinGroundDot = MathF.Cos(ToRadians(50f));

                DbVector3 InputVelocity = character.Velocity;
                DbVector3 CorrectedVelocity = InputVelocity;

                bool IsGrounded = false;
                foreach (ContactEPA Contact in Contacts)
                {
                    DbVector3 Normal = Contact.Normal;

                    float UpDot = Dot(Normal, WorldUp);
                    bool IsWalkable = UpDot >= MinGroundDot && UpDot > 0f;

                    if (IsWalkable && InputVelocity.y <= 0f) IsGrounded = true;

                    if (!IsWalkable)
                    {
                        Normal.y = 0f;
                        Normal = Normalize(Normal);
                    }

                    float Direction = Dot(Normal, CorrectedVelocity);
                    if (Direction < 0f) CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, Direction));
                }

                character.KinematicInformation.Grounded = IsGrounded;
                if (IsGrounded && InputVelocity.y <= 0f && CorrectedVelocity.y < 0f) CorrectedVelocity.y = 0f;

                character.IsColliding = Contacts.Count > 0;
                character.CorrectedVelocity = CorrectedVelocity;
                if (IsGrounded && character.Velocity.y < 0f)
                {
                    character.Velocity.y = 0f;
                }
            }

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




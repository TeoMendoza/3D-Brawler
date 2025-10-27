using System.Diagnostics.Contracts;
using System.Numerics;
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
            float YawRotation = (float)(Math.PI / 180.0) * character.Rotation.Yaw;
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
            DbVector3 oldPosition = character.Position;
            DbVector3 MoveVelocity = character.IsColliding ? character.CorrectedVelocity : character.Velocity;

            character.Position = new DbVector3(
            character.Position.x + MoveVelocity.x * time,
            character.Position.y + MoveVelocity.y * time,
            character.Position.z + MoveVelocity.z * time
            );

            if (character.Position.y <= 0f)
            {
                character.Position.y = 0f;
                character.KinematicInformation.Grounded = true;
            }

            else
            {
                character.KinematicInformation.Grounded = false;
                AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
                AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Jump");
                AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanCrouch").Subscribers, "Jump");
            }

            if (oldPosition.y > 0f && character.Position.y <= 0f) character.KinematicInformation.Landing = true;

            if (character.Velocity.y <= 0f) character.KinematicInformation.Falling = true;
            else character.KinematicInformation.Falling = false;

            character.Collider.Center = Add(character.Position, Mul(character.Collider.Direction, character.Collider.HeightEndToEnd * 0.5f));

            if (character.CollisionEntries.Count > 0)
            {
                List<Contact> Contacts = [];
                List<CollisionEntry> EntriesToRemove = [];
                foreach (var Entry in character.CollisionEntries)
                {
                    switch (Entry.Type)
                    {
                        case CollisionEntryType.Magician:
                            Magician Player = ctx.Db.magician.Id.Find(Entry.Id) ?? throw new Exception("Colliding Magician Not Found");
                            if (Player.Id != character.Id && TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(Player.Collider), Player.Collider, out Contact contact))
                            {
                                Contacts.Add(contact);
                            }
                            break;

                        case CollisionEntryType.ThrowingCard:
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

                DbVector3 CorrectedVelocity = character.Velocity;
                if (Contacts.Count > 0)
                {
                    foreach (Contact Contact in Contacts)
                    {
                        DbVector3 Normal = Contact.Normal;

                        float Direction = Dot(Normal, CorrectedVelocity);
                        if (Direction < 0f) CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, Direction));
                    }
                }

                character.IsColliding = Contacts.Count > 0;
                character.CorrectedVelocity = CorrectedVelocity;
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
            character.Velocity.y -= timer.gravity * time;
            ctx.Db.magician.identity.Update(character);
        }
    }

    [Reducer]
    public static void MagicianFinishedLanding(ReducerContext ctx)
    {
        Magician Magician = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Could Not Find Magician Who Finished Landing");
        Magician.KinematicInformation.Landing = false;
        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanRun").Subscribers, "Jump");
        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanCrouch").Subscribers, "Jump");
        ctx.Db.magician.identity.Update(Magician);
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
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Attack");
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
                    RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Attack");
                    RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
                    break;
                case MagicianState.Default:
                    break;
            }
        }

        ctx.Db.magician.identity.Update(character);
    }

    [Reducer]
    public static void SpawnThrowingCard(ReducerContext ctx, DbVector3 direction, DbVector3 spawnPoint)
    {
        Magician character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Throwing Card Owner Not Found");
        DbVector3 velocity = new DbVector3(direction.x * 10f, direction.y * 10f, direction.z * 10f); // Direction - Unit Vector
        ThrowingCard ThrowingCard = new()
        {
            OwnerIdentity = character.identity,
            MatchId = character.MatchId,
            position = spawnPoint,
            velocity = velocity,
            direction = direction,
            Collider = new CapsuleCollider { Center = spawnPoint, Direction = direction, HeightEndToEnd = 0.1f, Radius = 0.025f }, // Accounts For Prefab Scale, 0.1f, 0.025f
        };

        ctx.Db.throwing_cards.Insert(ThrowingCard);

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




using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [Reducer]
    public static void HandleMovementRequest(ReducerContext ctx, MovementRequest Request)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player To Move Not Found");
        character.rotation = Request.Aim;

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanWalk").Subscribers.Count == 0)
        {
            float YawRotation = (float)(Math.PI / 180.0) * character.rotation.Yaw;
            float SprintMultiplier = (GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint && Request.Velocity.z > 0f) ? 2f : 1f;

            character.velocity = new DbVector3((MathF.Cos(YawRotation) * Request.Velocity.x + MathF.Sin(YawRotation) * Request.Velocity.z) * SprintMultiplier,
                character.velocity.y, (-MathF.Sin(YawRotation) * Request.Velocity.x + MathF.Cos(YawRotation) * Request.Velocity.z) * SprintMultiplier);
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers.Count == 0 && Request.Jump)
        {
            character.velocity.y = 7.5f;
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Jump");
        }

        ctx.Db.playable_character.identity.Update(character);
    }

    

    [Reducer]
    public static void HandleActionEnterRequest(ReducerContext ctx, ActionRequest request)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player To Move Not Found");
        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers.Count == 0 && request.PlayerState == PlayerState.Attack)
        {
            character.state = PlayerState.Attack;
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Attack");
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
        }

        ctx.Db.playable_character.identity.Update(character);
    }

    [Reducer]
    public static void SpawnProjectile(ReducerContext ctx, DbVector3 direction, DbVector3 spawnPoint)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Projectile Owner Not Found");
        DbVector3 velocity = new DbVector3(direction.x * 50f, direction.y * 50f, direction.z * 50f); // Direction - Unit Vector
        Projectile projectile = new()
        {
            OwnerIdentity = character.identity,
            MatchId = character.MatchId,
            position = spawnPoint,
            velocity = velocity,
            direction = direction,
            Collider = new CapsuleCollider { Center = spawnPoint, Direction = direction, HeightEndToEnd = 0.1f, Radius = 0.025f }, // Accounts For Prefab Scale, 0.1f, 0.025f
        };

        ctx.Db.projectiles.Insert(projectile);

    }

    [Reducer]
    public static void HandleActionExitRequest(ReducerContext ctx, PlayerState newPlayerState)
    {
        Identity identity = ctx.Sender;
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player To Move Not Found");
        PlayerState oldPlayerState = character.state;
        switch (oldPlayerState)
        {
            case PlayerState.Attack:
                RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Attack");
                RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
                break;
            case PlayerState.Default:
                break;
        }

        character.state = newPlayerState;
        ctx.Db.playable_character.identity.Update(character);
    }

    [Reducer]
    public static void MovePlayers(ReducerContext ctx, Move_All_Players_Timer timer)
    {
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.playable_character.Iter())
        {
            var character = charac;
            DbVector3 MoveVelocity = character.IsColliding ? character.CorrectedVelocity : character.velocity;

            character.position = new DbVector3(
            character.position.x + MoveVelocity.x * time,
            character.position.y + MoveVelocity.y * time,
            character.position.z + MoveVelocity.z * time
            );

            if (character.position.y < 0f)
            {
                character.position.y = 0f;
                RemoveSubscriber(character.PlayerPermissionConfig[2].Subscribers, "Jump");
                RemoveSubscriber(character.PlayerPermissionConfig[1].Subscribers, "Jump");
            }

            character.Collider.Center = Add(character.position, Mul(character.Collider.Direction, character.Collider.HeightEndToEnd * 0.5f));

            if (character.CollisionEntries.Count > 0)
            {
                List<Contact> Contacts = [];
                foreach (var Entry in character.CollisionEntries)
                {
                    switch (Entry.Type)
                    {
                        case CollisionEntryType.Player:
                            Playable_Character Player = ctx.Db.playable_character.Id.Find(Entry.Id) ?? throw new Exception("Colliding Player Not Found");
                            if (Player.Id != character.Id && TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(Player.Collider), Player.Collider, out Contact contact))
                            {
                                Contacts.Add(contact);
                            }
                            break;

                        case CollisionEntryType.Bullet:
                            Projectile Projectile = ctx.Db.projectiles.Id.Find(Entry.Id) ?? throw new Exception("Colliding Bullet Not Found");
                            if (Projectile.OwnerIdentity != character.identity &&  TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(Projectile.Collider), Projectile.Collider, out Contact _contact))
                            {
                                ctx.Db.projectiles.Id.Delete(Projectile.Id);
                                if (character.CollisionEntries.Contains(Entry) is true) character.CollisionEntries.Remove(Entry);
                            }
                            break;

                        default:
                            break;
                    }
                    
                }

                DbVector3 CorrectedVelocity = character.velocity;
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
            
            ctx.Db.playable_character.identity.Update(character);
        }

    }
    
    [Reducer]
    public static void ApplyGravity(ReducerContext ctx, Gravity_Timer timer)
    {
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.playable_character.Iter())
        {
            var character = charac;
            character.velocity.y -= timer.gravity * time;
            ctx.Db.playable_character.identity.Update(character);
        }
    }

    [Reducer]
    public static void MoveProjectiles(ReducerContext ctx, Move_Projectiles_Timer timer) 
    {
        var time = timer.tick_rate;
        foreach (Projectile projectile in ctx.Db.projectiles.Iter())
        {
            var Projectile = projectile;
            Projectile.position = new DbVector3(
            Projectile.position.x + Projectile.velocity.x * time,
            Projectile.position.y + Projectile.velocity.y * time,
            Projectile.position.z + Projectile.velocity.z * time
            );

            Projectile.Collider.Center = Add(Projectile.position, Mul(Projectile.Collider.Direction, Projectile.Collider.HeightEndToEnd * 0.5f));
            ctx.Db.projectiles.Id.Update(Projectile);
        }
    }

    [Reducer]
    public static void AddCollisionEntry(ReducerContext ctx, CollisionEntry Entry)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player (Sender) Not Found"); // THIS IS THE ISSUE, DOESN"T ADD/REMOVE TO OUR TEST PLAYER LIST
        if (character.CollisionEntries.Contains(Entry) is false) character.CollisionEntries.Add(Entry);
        ctx.Db.playable_character.identity.Update(character);
    }

    [Reducer]
    public static void RemoveCollisionEntry(ReducerContext ctx, CollisionEntry Entry)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player (Sender) Not Found");
        if (character.CollisionEntries.Contains(Entry) is true) character.CollisionEntries.Remove(Entry);
        ctx.Db.playable_character.identity.Update(character);
    }

    
}
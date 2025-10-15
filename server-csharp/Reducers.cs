using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    
    [Reducer(ReducerKind.Init)] // The `init` parameter passed to the reducer macro indicates to SpacetimeDB that it should be called once upon database creation.
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"Initializing...");
        ctx.Db.match.Insert(new Match { maxPlayers = 12, currentPlayers = 0, inProgress = false });
        ctx.Db.playable_character.Insert(new Playable_Character
        {
            identity = new Identity(),
            Id = 10000,
            Name = "Test Teo",
            MatchId = 1,
            position = new DbVector3 { x = 20, y = 0, z = 0 },
            rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
            velocity = new DbVelocity3 { vx = 0, vy = 0, vz = 0 },
            state = PlayerState.Default,
            Collider = new CapsuleCollider { Center = new DbVector3 { x = 20, y = 0, z = 0 }, Direction = new DbVector3 { x = 0, y = 1, z = 0 }, HeightEndToEnd = 2f, Radius = 0.2f },
            PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", [])
                ],
            CollidingIds = []
        });

        ctx.Db.move_all_players.Insert(new Move_All_Players_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });

        ctx.Db.gravity.Insert(new Gravity_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f,
            gravity = 20
        });

        ctx.Db.move_projectiles_and_check_collisions.Insert(new Move_Projectiles_And_Check_Collisions_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });
    }


    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"{ctx.Sender} just connected.");
        var player = ctx.Db.logged_out_players.identity.Find(ctx.Sender);
        if (player != null)
        {
            ctx.Db.player.Insert(player.Value);
            ctx.Db.logged_out_players.identity.Delete(player.Value.identity);
        }
        else
        {
            ctx.Db.player.Insert(new Player
            {
                identity = ctx.Sender,
                Name = "Test Player"
            });
        }

        var Player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found after insert/restore");


        ctx.Db.playable_character.Insert(
            new Playable_Character
            {
                identity = Player.identity,
                Id = Player.Id,
                Name = Player.Name,
                MatchId = 1,
                position = new DbVector3 { x = 0, y = 0, z = 0 },
                rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
                velocity = new DbVelocity3 { vx = 0, vy = 0, vz = 0 },
                state = PlayerState.Default,
                Collider = new CapsuleCollider { Center = new DbVector3 { x = 0, y = 0, z = 0 }, Direction = new DbVector3 { x = 0, y = 1, z = 0 }, HeightEndToEnd = 2f, Radius = 0.2f }, // Height & Radius Are Manual For Now, Have To Change If Collider Changes
                PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", [])
                ],
                CollidingIds = []
            });

        var Match = ctx.Db.match.Id.Find(1);
        if (Match != null)
        {
            var match = Match.Value;
            match.currentPlayers += 1;
            ctx.Db.match.Id.Update(match);
        }
    }


    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        var character = ctx.Db.playable_character.identity.Find(ctx.Sender);

        if (character != null)
        {
            // Removes Player From Match
            var Match = ctx.Db.match.Id.Find(character.Value.MatchId);
            if (Match != null && Match.Value.currentPlayers > 0)
            {
                var match = Match.Value;
                match.currentPlayers -= 1;
                ctx.Db.match.Id.Update(match);
            }
            ctx.Db.playable_character.identity.Delete(ctx.Sender);

            // Removes Player Projectiles
            foreach (Projectile projectile in ctx.Db.projectiles.Iter())
            {
                if (projectile.OwnerIdentity == character.Value.identity)
                    ctx.Db.projectiles.Id.Delete(projectile.Id);
            }
        }

        ctx.Db.logged_out_players.Insert(player);
        ctx.Db.player.identity.Delete(player.identity);
    }

    [Reducer]
    public static void HandleMovementRequest(ReducerContext ctx, MovementRequest Request)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player To Move Not Found");
        character.rotation = Request.Aim;

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanWalk").Subscribers.Count == 0)
        {
            float YawRotation = (float)(Math.PI / 180.0) * character.rotation.Yaw;
            float SprintMultiplier = (GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint && Request.Velocity.vz > 0f) ? 2f : 1f;

            character.velocity = new DbVelocity3((MathF.Cos(YawRotation) * Request.Velocity.vx + MathF.Sin(YawRotation) * Request.Velocity.vz) * SprintMultiplier,
                character.velocity.vy, (-MathF.Sin(YawRotation) * Request.Velocity.vx + MathF.Cos(YawRotation) * Request.Velocity.vz) * SprintMultiplier);
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers.Count == 0 && Request.Jump)
        {
            character.velocity.vy = 7.5f;
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
        DbVelocity3 velocity = new DbVelocity3(direction.x * 2f, direction.y * 2f, direction.z * 2f); // Direction - Unit Vector
        Projectile projectile = new()
        {
            OwnerIdentity = character.identity,
            MatchId = character.MatchId,
            position = spawnPoint,
            velocity = velocity,
            direction = direction,
            Collider = new CapsuleCollider { Center = spawnPoint, Direction = direction, HeightEndToEnd = 0.1f, Radius = 0.025f }, // Accounts For Prefab Scale
            ProjectileType = ProjectileType.Bullet
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

            character.position = new DbVector3(
            character.position.x + character.velocity.vx * time,
            character.position.y + character.velocity.vy * time,
            character.position.z + character.velocity.vz * time
            );

            if (character.position.y < 0f)
            {
                character.position.y = 0f;
                RemoveSubscriber(character.PlayerPermissionConfig[2].Subscribers, "Jump");
                RemoveSubscriber(character.PlayerPermissionConfig[1].Subscribers, "Jump");
            }

            character.Collider.Center = Add(character.position, Mul(character.Collider.Direction, character.Collider.HeightEndToEnd * 0.5f));
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
            character.velocity.vy -= timer.gravity * time;
            ctx.Db.playable_character.identity.Update(character);
        }

    }

    [Reducer]
    public static void MoveProjectilesAndCheckCollisions(ReducerContext ctx, Move_Projectiles_And_Check_Collisions_Timer timer) // Maybe Seperate Move Proj And The Collision Check Into Seperate Timed Reducers
    {
        var time = timer.tick_rate;
        foreach (Projectile projectile in ctx.Db.projectiles.Iter())
        {
            var Projectile = projectile;
            Projectile.position = new DbVector3(
            Projectile.position.x + Projectile.velocity.vx * time,
            Projectile.position.y + Projectile.velocity.vy * time,
            Projectile.position.z + Projectile.velocity.vz * time
            );

            Projectile.Collider.Center = Add(Projectile.position, Mul(Projectile.Collider.Direction, Projectile.Collider.HeightEndToEnd * 0.5f));
            ctx.Db.projectiles.Id.Update(Projectile);
        }
    }

    [Reducer]
    public static void HandleBulletPlayerCollision(ReducerContext ctx, Identity PlayerIdentity, uint BulletId)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(PlayerIdentity) ?? throw new Exception("Player Hit By Bullet Not Found");
        Projectile Bullet = ctx.Db.projectiles.Id.Find(BulletId) ?? throw new Exception("Bullet That Hit Player Not Found");

        if (TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(Bullet.Collider), Bullet.Collider, out Contact contact))
        {
            ctx.Db.projectiles.Id.Delete(Bullet.Id);
        }
    }

    [Reducer]
    public static void HandlePlayerPlayerCollision(ReducerContext ctx, uint playerId)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player (Sender) Not Found");
        if (playerId == character.Id) return;
        if (character.CollidingIds.Contains(playerId) is false) character.CollidingIds.Add(playerId);
        ctx.Db.playable_character.identity.Update(character);
    }

}




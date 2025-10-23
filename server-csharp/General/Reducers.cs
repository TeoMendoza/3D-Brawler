using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [Reducer(ReducerKind.Init)] // The `init` parameter passed to the reducer macro indicates to SpacetimeDB that it should be called once upon database creation.
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"Initializing...");
        ctx.Db.match.Insert(new Match { maxPlayers = 12, currentPlayers = 0, inProgress = false });

        ctx.Db.move_all_players.Insert(new Move_All_Players_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 120.0)),
            tick_rate = 1.0f / 120.0f
        });

        ctx.Db.move_all_magicians.Insert(new Move_All_Magicians_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 120.0)),
            tick_rate = 1.0f / 120.0f
        });

        ctx.Db.gravity.Insert(new Gravity_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f,
            gravity = 20
        });

        ctx.Db.gravity_magician.Insert(new Gravity_Timer_Magician
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f,
            gravity = 20
        });

        ctx.Db.move_projectiles.Insert(new Move_Projectiles_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 90.0)),
            tick_rate = 1.0f / 90.0f
        });
    }


    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"{ctx.Sender} just connected.");
        var player = ctx.Db.logged_out_players.identity.Find(ctx.Sender);
        if (player != null)
        {
            ctx.Db.logged_in_players.Insert(player.Value);
            ctx.Db.logged_out_players.identity.Delete(player.Value.identity);
        }
        else
        {
            ctx.Db.logged_in_players.Insert(new Player
            {
                identity = ctx.Sender,
                Name = "Test Player"
            });
        }

        var Player = ctx.Db.logged_in_players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found after insert/restore");

        ctx.Db.magician.Insert(
            new Magician
            {
                identity = Player.identity,
                Id = Player.Id,
                Name = Player.Name,
                MatchId = 1,
                Position = new DbVector3 { x = 0, y = 0, z = 0 },
                Rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
                Velocity = new DbVector3 { x = 0, y = 0, z = 0 },
                KinematicInformation = new KinematicInformation(falling: false, crouched: false, grounded: true, landing: false, sprinting: false),
                State = MagicianState.Default,
                Collider = new CapsuleCollider { Center = new DbVector3 { x = 0, y = 0, z = 0 }, Direction = new DbVector3 { x = 0, y = 1, z = 0 }, HeightEndToEnd = 2f, Radius = 0.2f }, // Height & Radius Are Manual For Now, Have To Change If Collider Changes
                PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", []),
                    new("CanCrouch", [])
                ],
                CollisionEntries = [],
                IsColliding = false,
                CorrectedVelocity = new DbVector3 { x = 0, y = 0, z = 0 }
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
        var player = ctx.Db.logged_in_players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        var character = ctx.Db.magician.identity.Find(ctx.Sender);

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

            ctx.Db.magician.identity.Delete(ctx.Sender);

            // Removes Player Projectiles
            foreach (Projectile projectile in ctx.Db.projectiles.Iter())
            {
                if (projectile.OwnerIdentity == character.Value.identity)
                    ctx.Db.projectiles.Id.Delete(projectile.Id);
            }
        }

        ctx.Db.logged_out_players.Insert(player);
        ctx.Db.logged_in_players.identity.Delete(player.identity);
    }
        
}
using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;
using System.Linq;

public static partial class Module
{
    [Reducer(ReducerKind.Init)] // The `init` parameter passed to the reducer macro indicates to SpacetimeDB that it should be called once upon database creation.
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"Initializing...");

        ctx.Db.Map.Insert(new Map {Name = "Floor", Collider = FloorCollider});
        ctx.Db.Map.Insert(new Map {Name = "Ramp", Collider = RampCollider});
        ctx.Db.Map.Insert(new Map {Name = "Ramp2", Collider = Ramp2Collider});
        ctx.Db.Map.Insert(new Map {Name = "Platform", Collider = PlatformCollider});

        ctx.Db.move_all_magicians.Insert(new Move_All_Magicians_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });

        ctx.Db.handle_magician_timers_timer.Insert(new Handle_Magician_Timers_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });

        ctx.Db.gravity_magician.Insert(new Gravity_Timer_Magician
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f,
            gravity = 20
        });

        var Match = ctx.Db.match.Insert( new Match { maxPlayers = 12, currentPlayers = 1, inProgress = false });  

        //Test Player
        ctx.Db.magician.Insert(new Magician
        {
            identity = new Identity(),
            Id = 10000,
            Name = "Test Magician",
            MatchId = Match.Id,
            Position = new DbVector3 { x = 0f, y = 0f, z = 5f},
            Rotation = new DbRotation2 { Yaw = 180, Pitch = 0 },
            Velocity = new DbVector3 { x = 0, y = 0, z = 0f},
            CorrectedVelocity = new DbVector3 { x = 0, y = 0, z = 0 },
            Collider = MagicianIdleCollider,
            CollisionEntries = [new CollisionEntry(CollisionEntryType.Map, id: 1)],
            IsColliding = true,
            KinematicInformation = new KinematicInformation(jump: false, falling: false, crouched: false, grounded: true, sprinting: false),
            State = MagicianState.Default,
            PlayerPermissionConfig =
            [
                new("CanWalk", []),
                new("CanRun", []),
                new("CanJump", []),
                new("CanAttack", []),
                new("CanCrouch", [])
            ],
            Timers = [new Timer("Attack", 0.7f, 0.7f)]
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

        var Matches = ctx.Db.match.Started.Filter(false);
        var MatchesList = Matches.ToList();
        var Match = MatchesList.Count > 0 ? MatchesList[0] : ctx.Db.match.Insert( new Match { maxPlayers = 12, currentPlayers = 0, inProgress = false });
    
        Match.currentPlayers += 1;
        ctx.Db.match.Id.Update(Match);

        ctx.Db.magician.Insert(
            new Magician
            {
                identity = Player.identity,
                Id = Player.Id,
                Name = Player.Name,
                MatchId = Match.Id,
                Position = new DbVector3 { x = 0f, y = 0f, z = 0f},
                Rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
                Velocity = new DbVector3 { x = 0, y = 0, z = 0 },
                CorrectedVelocity = new DbVector3 { x = 0, y = 0, z = 0 },
                Collider = MagicianIdleCollider, 
                CollisionEntries = [new CollisionEntry(CollisionEntryType.Map, id: 1)],
                IsColliding = true,
                KinematicInformation = new KinematicInformation(jump: false, falling: false, crouched: false, grounded: true, sprinting: false),
                State = MagicianState.Default,
                PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", []),
                    new("CanCrouch", [])
                ],
                Timers = [new Timer("Attack", 0.7f, 0.7f)]
            });
    }

    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        var player = ctx.Db.logged_in_players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        var character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Magician not found");
        
        // Removes Player From Match
        var Match = ctx.Db.match.Id.Find(character.MatchId);
        if (Match != null && Match.Value.currentPlayers > 0)
        {
            var match = Match.Value;
            match.currentPlayers -= 1;
            ctx.Db.match.Id.Update(match);
        }

        ctx.Db.magician.Id.Delete(character.Id);
        
        // Removes Collision Entries Of Exited Player
        CollisionEntry CollisionEntry = new(CollisionEntryType.Magician, character.Id);
        foreach (Magician Magician in ctx.Db.magician.MatchId.Filter(character.MatchId))
        {
            if (Magician.CollisionEntries.Contains(CollisionEntry)) Magician.CollisionEntries.Remove(CollisionEntry); 
            ctx.Db.magician.Id.Update(Magician);
        }

        ctx.Db.logged_out_players.Insert(player);
        ctx.Db.logged_in_players.identity.Delete(player.identity);

        Log.Info("Disconnected");
    }
        
}
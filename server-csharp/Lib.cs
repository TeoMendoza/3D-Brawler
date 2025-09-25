using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    // Tables

    [Table(Name = "player", Public = true)]
    [Table(Name = "logged_out_players")]
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity;

        [Unique, AutoInc]
        public uint Id;
        public string Name;
    }

    [Table(Name = "match", Public = true)]
    public partial struct Match
    {
        [PrimaryKey, Unique, AutoInc]
        public uint Id;

        public uint maxPlayers;

        public uint currentPlayers;

        public bool inProgress;
    }


    [Table(Name = "playable_character", Public = true)]
    public partial struct Playable_Character
    {
        [PrimaryKey]
        public Identity identity;

        [Unique]
        public uint Id;
        public string Name;
        public uint MatchId;
        public DbVector3 position;
        public DbRotation2 rotation;
        public DbVelocity3 velocity;
    }

    [Table(Name = "move_all_players", Scheduled = nameof(MovePlayers), ScheduledAt = nameof(scheduled_at))]
    public partial struct Move_All_Players_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }


    // Reducers

    // Note the `init` parameter passed to the reducer macro.
    // That indicates to SpacetimeDB that it should be called
    // once upon database creation.

    [Reducer(ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"Initializing...");
        ctx.Db.match.Insert(new Match { maxPlayers = 12, currentPlayers = 0, inProgress = false });
        ctx.Db.move_all_players.Insert(new Move_All_Players_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)), tick_rate = 1.0f / 60.0f
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
                velocity = new DbVelocity3 { vx = 0, vy = 0, vz = 0 }
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
            var Match = ctx.Db.match.Id.Find(character.Value.MatchId);
            if (Match != null && Match.Value.currentPlayers > 0)
            {
                var match = Match.Value;
                match.currentPlayers -= 1;
                ctx.Db.match.Id.Update(match);
            }
            ctx.Db.playable_character.identity.Delete(ctx.Sender);
        }

        ctx.Db.logged_out_players.Insert(player);
        ctx.Db.player.identity.Delete(player.identity);
    }

    [Reducer]
    public static void Test(ReducerContext ctx)
    {
        Log.Info("Test Reducer Called");
    }

    [Reducer]
    public static void HandleMovementRequest(ReducerContext ctx, DbVelocity3 vel)
    {
        Playable_Character character = ctx.Db.playable_character.identity.Find(ctx.Sender) ?? throw new Exception("Player To Move Not Found");
        
        // Keep existing vy (gravity/jumps are server-owned)
        character.velocity = new DbVelocity3(vel.vx, character.velocity.vy, vel.vz);

        ctx.Db.playable_character.identity.Update(character);

        //Log.Info("HandleMovementRequest Called");
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
            ctx.Db.playable_character.identity.Update(character);
            Log.Info($"Position: {character.position}. Velocity: {character.velocity}");
        }
        
    }

    // Types

    [SpacetimeDB.Type]
    public partial struct DbVector3(float x, float y, float z)
    {
        public float x = x;
        public float y = y;
        public float z = z;
    }

    [SpacetimeDB.Type]
    public partial struct DbRotation2(float Yaw, float Pitch)
    {
        public float Yaw = Yaw; // Y axis, horizontal
        public float Pitch = Pitch; // X axis, verticle
    }

    [SpacetimeDB.Type]
    public partial struct DbVelocity3(float vx, float vy, float vz)
    {
        public float vx = vx; // X axis, right/left
        public float vy = vy; // Y axis, up/down
        public float vz = vz; // Z axis, forward/back
    }

}

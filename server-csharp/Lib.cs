using SpacetimeDB;

public static partial class Module
{
    // Tables

    [Table(Name = "Players", Public = true)]
    [Table(Name = "Logged_Out_Players")]
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity;

        [Unique, AutoInc]
        public uint Id;
        public string Name;
    }

    [Table(Name = "Matches", Public = true)]
    public partial struct Match
    {
        [PrimaryKey, Unique, AutoInc]
        public uint Id;

        public uint maxPlayers;

        public uint currentPlayers;

        public bool inProgress;
    }


    [Table(Name = "Prototype_Characters", Public = true)]
    public partial struct Character
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

    // Reducers

    // Note the `init` parameter passed to the reducer macro.
    // That indicates to SpacetimeDB that it should be called
    // once upon database creation.
    [Reducer(ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"Initializing...");
        ctx.Db.Matches.Insert(new Match { maxPlayers = 12, currentPlayers = 0, inProgress = false });
    }

    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"{ctx.Sender} just connected.");
        var player = ctx.Db.Logged_Out_Players.identity.Find(ctx.Sender);
        if (player != null)
        {
            ctx.Db.Players.Insert(player.Value);
            ctx.Db.Logged_Out_Players.identity.Delete(player.Value.identity);
        }
        else
        {
            ctx.Db.Players.Insert(new Player
            {
                identity = ctx.Sender,
                Name = "Test Player"
            });
        }

        var Player = ctx.Db.Players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found after insert/restore");

        ctx.Db.Prototype_Characters.Insert(
            new Character
            {
                identity = Player.identity,
                Id = Player.Id,
                Name = Player.Name,
                MatchId = 1,
                position = new DbVector3 { x = 0, y = 0, z = 0 },
                rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
                velocity = new DbVelocity3 { vx = 0, vy = 0, vz = 0 }
            });
        
        var Match = ctx.Db.Matches.Id.Find(1);
        if (Match != null)
        {
            var match = Match.Value;
            match.currentPlayers += 1;
            ctx.Db.Matches.Update(match);
        }
    }

    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        var player = ctx.Db.Players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");

        var character = ctx.Db.Prototype_Characters.identity.Find(ctx.Sender);
        if (character != null)
        {
            var match = ctx.Db.Matches.Id.Find(character.Value.MatchId);
            if (match != null && match.Value.currentPlayers > 0)
            {
                match.Value.currentPlayers -= 1;
                ctx.Db.Matches.Update(match.Value);
            }
            ctx.Db.Prototype_Characters.identity.Delete(ctx.Sender);
        }

        ctx.Db.Logged_Out_Players.Insert(player);
        ctx.Db.Players.identity.Delete(player.identity);
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

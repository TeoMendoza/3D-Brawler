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
        public PlayerState state;
        public List<PermissionEntry> PlayerPermissionConfig;
    }

    [Table(Name = "move_all_players", Scheduled = nameof(MovePlayers), ScheduledAt = nameof(scheduled_at))]
    public partial struct Move_All_Players_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }

    [Table(Name = "gravity", Scheduled = nameof(ApplyGravity), ScheduledAt = nameof(scheduled_at))]
    public partial struct Gravity_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
        public float gravity;
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
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });
        
        ctx.Db.gravity.Insert(new Gravity_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f,
            gravity = 20
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
                PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", [])
                ]

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
    public static void HandleActionExitRequest(ReducerContext ctx, PlayerState newPlayerState)
    {
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
            if (character.position.y < 0) {
                character.position.y = 0;
                RemoveSubscriber(character.PlayerPermissionConfig[2].Subscribers, "Jump");
                RemoveSubscriber(character.PlayerPermissionConfig[1].Subscribers, "Jump");
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
            character.velocity.vy -= timer.gravity * time;
            ctx.Db.playable_character.identity.Update(character);
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

    [SpacetimeDB.Type]
    public enum PlayerState
    {
        Default,
        Attack
    }

    [SpacetimeDB.Type]
    public partial struct MovementRequest(DbVelocity3 velocity, bool sprint, bool jump, DbRotation2 aim)
    {
        public DbVelocity3 Velocity = velocity;
        public bool Sprint = sprint;
        public bool Jump = jump;
        public DbRotation2 Aim = aim;
    }

    [SpacetimeDB.Type]
    public partial struct ActionRequest(PlayerState playerState)
    {
        public PlayerState PlayerState = playerState;
    }

    [SpacetimeDB.Type]
    public partial struct PermissionEntry(string key, List<string> subscribers)
    {
        public string Key = key;
        public List<string> Subscribers = subscribers;
    }


    // Funcs

    static void AddSubscriberUnique(List<string> subscribers, string reason) {
        if (subscribers.Contains(reason)) return;
        subscribers.Add(reason);
    }

    static void RemoveSubscriber(List<string> subscribers, string reason)
    {
        for (int i = subscribers.Count - 1; i >= 0; i--)
            if (subscribers[i] == reason) { subscribers.RemoveAt(i); break; }
    }
    
    private static PermissionEntry GetPermissionEntry(List<PermissionEntry> entries, string key)
    {
        foreach (var entry in entries)
        {
            if (entry.Key == key)
                return entry;
        }
        return entries[0];
    }





}

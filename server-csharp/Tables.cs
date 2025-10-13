using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

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
        public CapsuleCollider Collider;
        public List<PermissionEntry> PlayerPermissionConfig;
        // Add Internal Capsule Class To Keep Track Of Collision Box
    }

    [Table(Name = "projectiles", Public = true)]
    public partial struct Projectile
    {
        [PrimaryKey, AutoInc]
        public uint Id;
        public Identity OwnerIdentity;
        public uint MatchId;
        public DbVector3 position;
        public DbVelocity3 velocity;
        public DbVector3 direction;
        public CapsuleCollider Collider;
        public ProjectileType ProjectileType;

        // public List<Effect> Effects; // Add once effects are being implemented
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

    [Table(Name = "move_projectiles_and_check_collisions", Scheduled = nameof(MoveProjectilesAndCheckCollisions), ScheduledAt = nameof(scheduled_at))]
    public partial struct Move_Projectiles_And_Check_Collisions_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }
    
}

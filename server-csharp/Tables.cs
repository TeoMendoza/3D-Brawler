using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    [Table(Name = "logged_in_players", Public = true)]
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

    [Table(Name = "magician", Public = true)]
    public partial struct Magician
    {
        [PrimaryKey]
        public Identity identity;

        [Unique]
        public uint Id;
        public string Name;
        public uint MatchId;
        public DbVector3 Position;
        public DbRotation2 Rotation;
        public DbVector3 Velocity;
        public MagicianState State;
        public KinematicInformation KinematicInformation;
        public CapsuleCollider Collider;
        public List<PermissionEntry> PlayerPermissionConfig;
        public List<CollisionEntry> CollisionEntries;
        public DbVector3 CorrectedVelocity;
        public bool IsColliding;
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
        public DbVector3 velocity;
        public PlayerState state;
        public CapsuleCollider Collider;
        public List<PermissionEntry> PlayerPermissionConfig;
        public List<CollisionEntry> CollisionEntries;
        public bool IsColliding;
        public DbVector3 CorrectedVelocity;
    }

    [Table(Name = "projectiles", Public = true)]
    public partial struct Projectile
    {
        [PrimaryKey, AutoInc]
        public uint Id;
        public Identity OwnerIdentity;
        public uint MatchId;
        public DbVector3 position;
        public DbVector3 velocity;
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

    [Table(Name = "move_projectiles", Scheduled = nameof(MoveProjectiles), ScheduledAt = nameof(scheduled_at))]
    public partial struct Move_Projectiles_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }

    [Table(Name = "move_all_magicians", Scheduled = nameof(MoveMagicians), ScheduledAt = nameof(scheduled_at))]
    public partial struct Move_All_Magicians_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }

    [Table(Name = "gravity_magician", Scheduled = nameof(ApplyGravityMagician), ScheduledAt = nameof(scheduled_at))]
    public partial struct Gravity_Timer_Magician
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
        public float gravity;
    }

}

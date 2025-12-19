using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [Table(Name = "magician", Public = true)]
    [SpacetimeDB.Index.BTree(Name = "SameMatchPlayers", Columns = [nameof(MatchId), nameof(Id)])]
    public partial struct Magician
    {
        [PrimaryKey]
        public Identity identity;

        [Unique, AutoInc]
        public uint Id;
        public string Name;

        [SpacetimeDB.Index.BTree(Name = "MatchId")]
        public uint MatchId;
        public DbVector3 Position;
        public DbRotation2 Rotation;
        public DbVector3 Velocity;
        public DbVector3 CorrectedVelocity;
        public ComplexCollider Collider;
        public List<CollisionEntry> CollisionEntries;
        public bool IsColliding;
        public MagicianState State;
        public KinematicInformation KinematicInformation;
        public List<PermissionEntry> PlayerPermissionConfig;
        public List<Timer> Timers;

        //public List<ThrowingCard> AttackMagazine;
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

    [Table(Name = "handle_magician_timers_timer", Scheduled = nameof(HandleMagicianTimers), ScheduledAt = nameof(scheduled_at))]
    public partial struct Handle_Magician_Timers_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }

    
}
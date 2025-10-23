using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
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
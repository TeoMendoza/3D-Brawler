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

        [Unique]
        public uint Id;
        public string Name;

        [SpacetimeDB.Index.BTree(Name = "MatchId")]
        public uint MatchId;
        public DbVector3 Position;
        public DbRotation2 Rotation;
        public DbVector3 Velocity;
        public MagicianState State;
        public KinematicInformation KinematicInformation;
        public CapsuleCollider Collider;
        public ConvexHullCollider GjkCollider;
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

    [Table(Name = "throwing_cards", Public = true)]
    public partial struct ThrowingCard
    {
        [PrimaryKey, AutoInc]
        public uint Id;

        [SpacetimeDB.Index.BTree(Name = "OwnerIdentity")]
        public Identity OwnerIdentity;

        [SpacetimeDB.Index.BTree(Name = "MatchId")]
        public uint MatchId;
        public DbVector3 position;
        public DbVector3 velocity;
        public DbVector3 direction;
        public CapsuleCollider Collider;

        // public List<Effect> Effects; // Add once effects are being implemented
    }
    
    [Table(Name = "move_throwing_cards", Scheduled = nameof(MoveThrowingCards), ScheduledAt = nameof(scheduled_at))]
    public partial struct Move_ThrowingCards_Timer
    {
        [PrimaryKey, AutoInc] public ulong scheduled_id;
        public ScheduleAt scheduled_at;
        public float tick_rate;
    }
}
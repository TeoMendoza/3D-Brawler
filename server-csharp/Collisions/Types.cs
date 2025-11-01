using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    [SpacetimeDB.Type]
    public partial struct SphereCollider(DbVector3 center, float radius)
    {
        public DbVector3 Center = center;
        public float Radius = radius;
    }

    [SpacetimeDB.Type]
    public partial struct BoxCollider(DbVector3 center, DbVector3 size, DbVector3 direction)
    {
        public DbVector3 Center = center;
        public DbVector3 Size = size;   // width, height, length
        public DbVector3 Direction = direction; // Normalized Already

    }

    [SpacetimeDB.Type]
    public partial struct CapsuleCollider(DbVector3 center, float heightEndToEnd, float radius, DbVector3 direction)
    {
        public DbVector3 Center = center;
        public float HeightEndToEnd = heightEndToEnd;
        public float Radius = radius;
        public DbVector3 Direction = direction; // Normalized Already
    }

    [SpacetimeDB.Type]
    public enum Shape
    {
        Capsule,
        Sphere,
        Box,
    }

    [SpacetimeDB.Type]
    public partial struct CollisionEntry(CollisionEntryType type, uint id)
    {
        public CollisionEntryType Type = type;
        public uint Id = id;

    }

    [SpacetimeDB.Type]
    public enum CollisionEntryType
    {
        Magician,
        ThrowingCard,
        Map,
        Player, // Remove Eventually
        Bullet  // Remove Eventually  
    }

    public partial struct Contact
    {
        public DbVector3 Normal; // Object B -> A
        public float Depth;
    }

    public partial struct Raycast(DbVector3 position, DbVector3 forward, float maxDistance)
    {
        public DbVector3 Position = position;
        public DbVector3 Forward = forward;
        public float MaxDistance = maxDistance;
    }

}
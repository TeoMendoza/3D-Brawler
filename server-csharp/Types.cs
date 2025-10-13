using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    
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
    public enum ProjectileType
    {
        Bullet,
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
        Box
    }

    public partial struct Contact
    {
        public DbVector3 Normal; // Object A -> B
        public float Depth;
    }

}
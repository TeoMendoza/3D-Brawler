using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public partial struct DbVector3(float x, float y, float z)
    {
        public float x = x; // Velocity right/left
        public float y = y; // Velocity up/down
        public float z = z; // Velocity forward/back
    }

    [SpacetimeDB.Type]
    public partial struct DbRotation2(float Yaw, float Pitch)
    {
        public float Yaw = Yaw; // Y axis, horizontal
        public float Pitch = Pitch; // X axis, verticle
    }

    [SpacetimeDB.Type]
    public partial struct PermissionEntry(string key, List<string> subscribers)
    {
        public string Key = key;
        public List<string> Subscribers = subscribers;
    }

    [SpacetimeDB.Type]
    public partial struct MovementRequest(bool moveForward, bool moveBackward, bool moveLeft, bool moveRight, bool sprint, bool jump, bool crouch, DbRotation2 aim)
    {
        public bool MoveForward = moveForward;
        public bool MoveBackward = moveBackward;
        public bool MoveLeft = moveLeft;
        public bool MoveRight = moveRight;
        public bool Sprint = sprint;
        public bool Jump = jump;
        public bool Crouch = crouch;
        public DbRotation2 Aim = aim;
    }

    [SpacetimeDB.Type]
    public enum CharacterType
    {
        Magician,
        Hunter,
        Monk
    }

    [SpacetimeDB.Type]
    public partial struct Timer(string name, float resetTime, float currentTime)
    {
        public string Name = name;
        public float ResetTime = resetTime; // Seconds
        public float CurrentTime = currentTime; // Seconds
    }
}
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public partial struct KinematicInformation(bool jump, bool falling, bool crouched, bool grounded, bool sprinting)
    {
        public bool Jump = jump;
        public bool Falling = falling;
        public bool Crouched = crouched;
        public bool Grounded = grounded;
        public bool Sprinting = sprinting;
    }

    [SpacetimeDB.Type]
    public partial struct ActionRequestMagician(MagicianState state, AttackInformation attackInformation)
    {
        public MagicianState State = state;
        public AttackInformation AttackInformation = attackInformation;
    }
    
    [SpacetimeDB.Type]
    public partial struct AttackInformation(DbVector3 cameraPositionOffset, float cameraYawOffset, float cameraPitchOffset, DbVector3 spawnPointOffset, float maxDistance)
    {
        public DbVector3 CameraPositionOffset = cameraPositionOffset;
        public float CameraYawOffset = cameraYawOffset;
        public float CameraPitchOffset = cameraPitchOffset;
        public DbVector3 SpawnPointOffset = spawnPointOffset;
        public float MaxDistance = maxDistance;
    }

    [SpacetimeDB.Type]
    public enum MagicianState
    {
        Default,
        Attack
    }
    [SpacetimeDB.Type]
    public partial struct ThrowingCard
    {
        //public List<Effect> Effects; // Add once effects are being implemented
    }

}
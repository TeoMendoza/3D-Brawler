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
    public partial struct ActionRequestMagician(MagicianState state, AttackInformation attackInformation, ReloadInformation reloadInformation)
    {
        public MagicianState State = state;
        public AttackInformation AttackInformation = attackInformation;
        public ReloadInformation ReloadInformation = reloadInformation;
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
    public partial struct ReloadInformation() {} // Empty For Now - Might Have Info Later On

    [SpacetimeDB.Type]
    public enum MagicianState
    {
        Default,
        Attack,
        Reload
    }

    [SpacetimeDB.Type]
    public partial struct ThrowingCard(List<Effect> effects)
    {
        public List<Effect> Effects = effects;
    }

    [SpacetimeDB.Type]
    public partial struct Effect(EffectType type)
    {
        public EffectType Type = type; // Add A Struct For Each Different Effect That Has The Neccessary Information - Similar To Action Request System
    }

    [SpacetimeDB.Type]
    public enum EffectType
    {
        Damage, // Etc
    }

}
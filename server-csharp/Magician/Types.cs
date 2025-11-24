using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public enum MagicianState
    {
        Default,
        Attack
    }

    [SpacetimeDB.Type]
    public partial struct ActionRequestMagician(MagicianState state)
    {
        public MagicianState State = state;
    }
    
    [SpacetimeDB.Type]
    public partial struct KinematicInformation(bool falling, bool crouched, bool grounded, bool sprinting)
    {
        public bool Falling = falling;
        public bool Crouched = crouched;
        public bool Grounded = grounded;
        public bool Sprinting = sprinting;
    }
}
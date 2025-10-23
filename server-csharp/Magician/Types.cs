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
    public partial struct KinematicInformation(bool falling, bool crouched, bool grounded, bool landing, bool sprinting)
    {
        public bool Falling = falling;
        public bool Crouched = crouched;
        public bool Grounded = grounded;
        public bool Landing = landing;
        public bool Sprinting = sprinting;
    }
}
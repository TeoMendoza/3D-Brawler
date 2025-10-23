using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public partial struct ActionRequest(PlayerState playerState)
    {
        public PlayerState PlayerState = playerState;
    }
    
    [SpacetimeDB.Type]
    public enum PlayerState
    {
        Default,
        Attack
    }
}
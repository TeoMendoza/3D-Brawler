using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public partial struct MagicianConfig(Player player, uint matchId, DbVector3 position)
    {
        public Player Player = player;
        public uint MatchId = matchId;
        public DbVector3 Position = position;
    }
}
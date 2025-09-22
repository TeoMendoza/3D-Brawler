using SpacetimeDB;

public static partial class Module
{
    [Table(Name = "Players", Public = true)]
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity;
        
        [Unique, AutoInc]
        public uint Id;
        public string Name;
        public int MatchId;
    }

    [SpacetimeDB.Reducer]
    public static void Add(ReducerContext ctx, string name, int matchId)
    {
        var player = ctx.Db.Players.Insert(new Player { Name = name, MatchId = matchId });
        Log.Info($"Inserted {player.Name} under #{player.Id}");
    }
    
    [Reducer]
    public static void Debug(ReducerContext ctx)
    {
        Log.Info($"This reducer was called by {ctx.Sender}");	  
    }

}

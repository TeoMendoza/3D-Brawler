using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;
using System.Linq;

public static partial class Module
{
    [Reducer(ReducerKind.Init)] // The `init` parameter passed to the reducer macro indicates to SpacetimeDB that it should be called once upon database creation.
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"Initializing...");

        ctx.Db.Map.Insert(new Map {Name = "Floor", GjkCollider = FloorCollider});
        ctx.Db.Map.Insert(new Map {Name = "Ramp", GjkCollider = RampCollider});

        ctx.Db.move_all_magicians.Insert(new Move_All_Magicians_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });

        ctx.Db.gravity_magician.Insert(new Gravity_Timer_Magician
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f,
            gravity = 20
        });

        ctx.Db.move_throwing_cards.Insert(new Move_ThrowingCards_Timer
        {
            scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(1000.0 / 60.0)),
            tick_rate = 1.0f / 60.0f
        });
    }


    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"{ctx.Sender} just connected.");
        var player = ctx.Db.logged_out_players.identity.Find(ctx.Sender);
        if (player != null)
        {
            ctx.Db.logged_in_players.Insert(player.Value);
            ctx.Db.logged_out_players.identity.Delete(player.Value.identity);
        }
        else
        {
            ctx.Db.logged_in_players.Insert(new Player
            {
                identity = ctx.Sender,
                Name = "Test Player"
            });
        }

        var Player = ctx.Db.logged_in_players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found after insert/restore");

        var Matches = ctx.Db.match.Started.Filter(false);
        var MatchesList = Matches.ToList();
        var Match = MatchesList.Count > 0 ? MatchesList[0] : ctx.Db.match.Insert( new Match { maxPlayers = 12, currentPlayers = 0, inProgress = false });
    
        Match.currentPlayers += 1;
        ctx.Db.match.Id.Update(Match);

        ctx.Db.magician.Insert(
            new Magician
            {
                identity = Player.identity,
                Id = Player.Id,
                Name = Player.Name,
                MatchId = Match.Id,
                Position = new DbVector3 { x = 0f, y = 0f, z = 0f},
                Rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
                Velocity = new DbVector3 { x = 0, y = 0, z = 0 },
                KinematicInformation = new KinematicInformation(falling: false, crouched: false, grounded: true, sprinting: false),
                State = MagicianState.Default,
                Collider = new CapsuleCollider { Center = new DbVector3 { x = 0, y = 0, z = 0 }, Direction = new DbVector3 { x = 0, y = 1, z = 0 }, HeightEndToEnd = 2f, Radius = 0.2f }, // Height & Radius Are Manual For Now, Have To Change If Collider Changes
                GjkCollider = MagicianIdleCollider, 
                PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", []),
                    new("CanCrouch", [])
                ],
                CollisionEntries = [new CollisionEntry(CollisionEntryType.Map, id: 1)],
                IsColliding = true,
                CorrectedVelocity = new DbVector3 { x = 0, y = 0, z = 0 }
            });

            //Test Player
            ctx.Db.magician.Insert(new Magician
            {
                identity = new Identity(),
                Id = 10000,
                Name = "Test Magician",
                MatchId = Match.Id,
                Position = new DbVector3 { x = 0f, y = 0f, z = 5f},
                Rotation = new DbRotation2 { Yaw = 180, Pitch = 0 },
                Velocity = new DbVector3 { x = 0, y = 0, z = 0f},
                KinematicInformation = new KinematicInformation(falling: false, crouched: false, grounded: true, sprinting: false),
                State = MagicianState.Default,
                Collider = new CapsuleCollider { Center = new DbVector3 { x = 0, y = 0, z = 0 }, Direction = new DbVector3 { x = 0, y = 1, z = 0 }, HeightEndToEnd = 2f, Radius = 0.2f }, // Height & Radius Are Manual For Now, Have To Change If Collider Changes
                GjkCollider = MagicianIdleCollider,
                PlayerPermissionConfig =
                [
                    new("CanWalk", []),
                    new("CanRun", []),
                    new("CanJump", []),
                    new("CanAttack", []),
                    new("CanCrouch", [])
                ],
                CollisionEntries = [new CollisionEntry(CollisionEntryType.Map, id: 1)],
                IsColliding = true,
                CorrectedVelocity = new DbVector3 { x = 0, y = 0, z = 0 }
            });
    }


    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        var player = ctx.Db.logged_in_players.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");
        var character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Magician not found");
        
        // Removes Player From Match
        var Match = ctx.Db.match.Id.Find(character.MatchId);
        if (Match != null && Match.Value.currentPlayers > 0)
        {
            var match = Match.Value;
            match.currentPlayers -= 1;
            ctx.Db.match.Id.Update(match);
        }

        ctx.Db.magician.identity.Delete(ctx.Sender);
        ctx.Db.magician.Id.Delete(10000);

        // Removes Player Objects
        foreach(ThrowingCard ThrowingCard in ctx.Db.throwing_cards.OwnerIdentity.Filter(character.identity)) ctx.Db.throwing_cards.Id.Delete(ThrowingCard.Id);

        // Removes Collision Entries Of Exited Player
        CollisionEntry CollisionEntry = new(CollisionEntryType.Magician, character.Id);
        foreach (Magician Magician in ctx.Db.magician.MatchId.Filter(character.MatchId)) // Consider Making Some Sort Of Shared Entities Table To Make This Faster
        {
            // Could Also Make This An Index With The Match Id, Probably Will Be Useful When Making A Combined Table Of Everything
            if (Magician.CollisionEntries.Contains(CollisionEntry)) Magician.CollisionEntries.Remove(CollisionEntry); 
                
            foreach (ThrowingCard ThrowingCard in ctx.Db.throwing_cards.OwnerIdentity.Filter(character.identity)) // This Filter Can Be Done Outside The Magician For Loop To Save Compute, But May Want To Keep It Inside Due To Bullets Possibly Being Removed While Loop Is Running?
            {
                CollisionEntry ThrowingCardCollisionEntry = new(CollisionEntryType.ThrowingCard, ThrowingCard.Id);
                if (Magician.CollisionEntries.Contains(ThrowingCardCollisionEntry)) Magician.CollisionEntries.Remove(CollisionEntry);
            }

            ctx.Db.magician.Id.Update(Magician);
        }

        ctx.Db.logged_out_players.Insert(player);
        ctx.Db.logged_in_players.identity.Delete(player.identity);

        Log.Info("Disconnected");
    }
        
}
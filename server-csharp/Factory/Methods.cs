using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    
    public static Magician CreateMagician(MagicianConfig Config)
    {
        Player Player = Config.Player;
        uint MatchId = Config.MatchId;
        DbVector3 Position = Config.Position;

        Magician Magician = new()
        {
                identity = Player.identity,
                Id = Player.Id,
                Name = Player.Name,
                MatchId = MatchId,
                Position = Position,
                Rotation = new DbRotation2 { Yaw = 0, Pitch = 0 },
                Velocity = new DbVector3 { x = 0, y = 0, z = 0 },
                CorrectedVelocity = new DbVector3 { x = 0, y = 0, z = 0 },
                Collider = MagicianIdleCollider, 
                CollisionEntries = [new CollisionEntry(CollisionEntryType.Map, id: 1)],
                IsColliding = false,
                KinematicInformation = new KinematicInformation(jump: false, falling: false, crouched: false, grounded: false, sprinting: false),
                State = MagicianState.Default,
                PlayerPermissionConfig = [new("CanWalk", []), new("CanRun", []), new("CanJump", []), new("CanCrouch", []), new("CanAttack", []), new("CanReload", [])],
                Timers = [new Timer("Attack", 0.7f, 0.7f), new Timer("Reload", 2.2f, 2.2f)],
                Bullets = [],
                BulletCapacity = 8
        };

        List<ThrowingCard> Bullets = new List<ThrowingCard>(Magician.BulletCapacity);
        for (int i = 0; i < Magician.BulletCapacity; i++)
        {
            ThrowingCard ThrowingCard = CreateThrowingCard();
            Bullets.Add(ThrowingCard);
        }
        Magician.Bullets = Bullets;
        
        return Magician;
    }

    public static ThrowingCard CreateThrowingCard()
    {
        List<Effect> Effects = new List<Effect> { new(EffectType.Damage) };
        ThrowingCard ThrowingCard = new(effects: Effects);
        return ThrowingCard;
    }
}

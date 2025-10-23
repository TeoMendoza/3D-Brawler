using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [Reducer]
    public static void HandleMovementRequestMagician(ReducerContext ctx, MovementRequest Request)
    {
        Magician character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Magician To Move Not Found");
        character.Rotation = Request.Aim;

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanWalk").Subscribers.Count == 0)
        {
            float YawRotation = (float)(Math.PI / 180.0) * character.Rotation.Yaw;
            character.Velocity = new DbVector3(MathF.Cos(YawRotation) * Request.Velocity.x + MathF.Sin(YawRotation) * Request.Velocity.z, character.Velocity.y, -MathF.Sin(YawRotation) * Request.Velocity.x + MathF.Cos(YawRotation) * Request.Velocity.z);
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers.Count == 0 && Request.Jump)
        {
            character.Velocity.y = 7.5f;
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanCrouch").Subscribers.Count == 0 && Request.Crouch)
        {
            character.Velocity = new DbVector3(character.Velocity.x * 0.5f, character.Velocity.y, character.Velocity.z * 0.5f);
            character.KinematicInformation.Crouched = true;
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");            
        }

        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint)
        {
            character.Velocity = new DbVector3(character.Velocity.x * 2f, character.Velocity.y, character.Velocity.z * 2f);
        }

        if (Request.Crouch is false)
        {
            character.KinematicInformation.Crouched = false;
            RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");
        }

        ctx.Db.magician.identity.Update(character);
    }


    [Reducer]
    public static void MoveMagicians(ReducerContext ctx, Move_All_Magicians_Timer timer)
    {
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.magician.Iter())
        {
            var character = charac;
            DbVector3 oldPosition = character.Position;
            DbVector3 MoveVelocity = character.IsColliding ? character.CorrectedVelocity : character.Velocity;

            character.Position = new DbVector3(
            character.Position.x + MoveVelocity.x * time,
            character.Position.y + MoveVelocity.y * time,
            character.Position.z + MoveVelocity.z * time
            );

            if (character.Position.y <= 0f)
            {
                character.Position.y = 0f;
                character.KinematicInformation.Grounded = true;
            }

            else
            {
                character.KinematicInformation.Grounded = false;
                AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
                AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanRun").Subscribers, "Jump");
                AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanCrouch").Subscribers, "Jump");
            }

            if (oldPosition.y > 0f && character.Position.y <= 0f) character.KinematicInformation.Landing = true;

            if (character.Velocity.y <= 0f) character.KinematicInformation.Falling = true;
            else character.KinematicInformation.Falling = false;

            character.Collider.Center = Add(character.Position, Mul(character.Collider.Direction, character.Collider.HeightEndToEnd * 0.5f));

            if (character.CollisionEntries.Count > 0)
            {
                List<Contact> Contacts = [];
                foreach (var Entry in character.CollisionEntries)
                {
                    switch (Entry.Type)
                    {
                        case CollisionEntryType.Player:
                            Playable_Character Player = ctx.Db.playable_character.Id.Find(Entry.Id) ?? throw new Exception("Colliding Player Not Found");
                            if (Player.Id != character.Id && TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(Player.Collider), Player.Collider, out Contact contact))
                            {
                                Contacts.Add(contact);
                            }
                            break;

                        case CollisionEntryType.Bullet:
                            Projectile Projectile = ctx.Db.projectiles.Id.Find(Entry.Id) ?? throw new Exception("Colliding Bullet Not Found");
                            if (Projectile.OwnerIdentity != character.identity && TryOverlap(GetColliderShape(character.Collider), character.Collider, GetColliderShape(Projectile.Collider), Projectile.Collider, out Contact _contact))
                            {
                                ctx.Db.projectiles.Id.Delete(Projectile.Id);
                                if (character.CollisionEntries.Contains(Entry) is true) character.CollisionEntries.Remove(Entry);
                            }
                            break;

                        default:
                            break;
                    }

                }

                DbVector3 CorrectedVelocity = character.Velocity;
                if (Contacts.Count > 0)
                {
                    foreach (Contact Contact in Contacts)
                    {
                        DbVector3 Normal = Contact.Normal;

                        float Direction = Dot(Normal, CorrectedVelocity);
                        if (Direction < 0f) CorrectedVelocity = Sub(CorrectedVelocity, Mul(Normal, Direction));
                    }
                }

                character.IsColliding = Contacts.Count > 0;
                character.CorrectedVelocity = CorrectedVelocity;
            }

            ctx.Db.magician.identity.Update(character);
        }

    }

    [Reducer]
    public static void ApplyGravityMagician(ReducerContext ctx, Gravity_Timer_Magician timer)
    {
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.magician.Iter())
        {
            var character = charac;
            character.Velocity.y -= timer.gravity * time;
            ctx.Db.magician.identity.Update(character);
        }
    }

    [Reducer]
    public static void MagicianFinishedLanding(ReducerContext ctx)
    {
        Magician Magician = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Could Not Find Magician Who Finished Landing");
        Magician.KinematicInformation.Landing = false;
        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanJump").Subscribers, "Jump");
        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanRun").Subscribers, "Jump");
        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanCrouch").Subscribers, "Jump");
        ctx.Db.magician.identity.Update(Magician);
    }
    
}




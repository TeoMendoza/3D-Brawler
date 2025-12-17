using System.Diagnostics.Contracts;
using System.Numerics;
using System;
using SpacetimeDB;

public static partial class Module
{
    [Reducer]
    public static void HandleMovementRequestMagician(ReducerContext ctx, MovementRequest Request)
    {
        Magician Character = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Magician To Move Not Found");

        Character.Rotation = Request.Aim;
        Character.Velocity = new DbVector3(0f, Character.Velocity.y, 0f);
        Character.KinematicInformation.Jump = false;

        if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanWalk").Subscribers.Count == 0)
        {
            float LocalX = 0f;
            float LocalZ = 0f;

            if (Request.MoveForward && !Request.MoveBackward) LocalZ = 2f;
            else if (Request.MoveBackward && !Request.MoveForward) LocalZ = -2f;

            if (Request.MoveRight && !Request.MoveLeft) LocalX = 2f;
            else if (Request.MoveLeft && !Request.MoveRight) LocalX = -2f;

            if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint && Request.MoveForward && !Request.MoveBackward)
                LocalZ *= 2.5f;
            
            if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers.Count == 0 && Request.Sprint)
                LocalX *= 1.5f;

            float YawRadians = ToRadians(Character.Rotation.Yaw);
            float CosYaw = MathF.Cos(YawRadians);
            float SinYaw = MathF.Sin(YawRadians);

            float WorldX = CosYaw * LocalX + SinYaw * LocalZ;
            float WorldZ = -SinYaw * LocalX + CosYaw * LocalZ;

            Character.Velocity = new DbVector3(WorldX, Character.Velocity.y, WorldZ);
        }

        if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanJump").Subscribers.Count == 0 && Request.Jump)
        {
            Character.KinematicInformation.Jump = true;
            Character.Velocity.y = 7.5f;
        }

        if (GetPermissionEntry(Character.PlayerPermissionConfig, "CanCrouch").Subscribers.Count == 0 && Request.Crouch)
        {
            Character.Velocity = new DbVector3(Character.Velocity.x * 0.5f, Character.Velocity.y, Character.Velocity.z * 0.5f);
            Character.KinematicInformation.Crouched = true;
            AddSubscriberUnique(GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");
        }

        if (Request.Crouch is false)
        {
            Character.KinematicInformation.Crouched = false;
            RemoveSubscriber(GetPermissionEntry(Character.PlayerPermissionConfig, "CanRun").Subscribers, "Crouch");
        }

        if (Character.Id != 10000) // Testing To Make Sure Moving Collisions Work
            ctx.Db.magician.identity.Update(Character);
    }   

    [Reducer]
    public static void HandleActionChangeRequestMagician(ReducerContext ctx, ActionRequestMagician request)
    {
        Identity identity = ctx.Sender;
        Magician character = ctx.Db.magician.identity.Find(identity) ?? throw new Exception("Magician Not Found");
        MagicianState oldState = character.State;

        bool StateSwitched = false;
        if (GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers.Count == 0 && request.State == MagicianState.Attack)
        {
            character.State = MagicianState.Attack;
            AddSubscriberUnique(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
            StateSwitched = true;
        }

        else if (request.State == MagicianState.Default)
        {
            character.State = MagicianState.Default;
            StateSwitched = true;
        }

        if (StateSwitched)
        {
            switch (oldState)
            {
                case MagicianState.Attack:
                    RemoveSubscriber(GetPermissionEntry(character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
                    break;
                case MagicianState.Default:
                    break;
            }
        }

        ctx.Db.magician.identity.Update(character);
    }

    [Reducer]
    public static void MoveMagicians(ReducerContext Ctx, Move_All_Magicians_Timer Timer)
    {
        float TickTime = Timer.tick_rate;
        float MinTimeStep = 1e-4f;
        int MaxSubsteps = 4;

        foreach (var CharacterRow in Ctx.Db.magician.Iter())
        {
            Magician Character = CharacterRow;

            bool WasGrounded = Character.KinematicInformation.Grounded;
            Character.KinematicInformation.Grounded = false;
            Character.IsColliding = false;
            Character.CorrectedVelocity = Character.Velocity;

            List<CollisionContact> PreContacts = new List<CollisionContact>();
            foreach (CollisionEntry Entry in Character.CollisionEntries)
            {
                TryBuildContactForEntry(Ctx, ref Character, Entry, PreContacts);
            }

            if (PreContacts.Count > 0)
            {
                ResolveContacts(ref Character, PreContacts, Character.Velocity);
            }

            float RemainingTime = TickTime;
            int SubstepCount = 0;

            while (RemainingTime > MinTimeStep && SubstepCount < MaxSubsteps)
            {
                SubstepCount += 1;
                float StepTime = RemainingTime / (MaxSubsteps - SubstepCount + 1);
                DbVector3 StepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;

                Character.Position = new DbVector3(
                    Character.Position.x + StepVelocity.x * StepTime,
                    Character.Position.y + StepVelocity.y * StepTime,
                    Character.Position.z + StepVelocity.z * StepTime
                );

                foreach (CollisionEntry Entry in Character.CollisionEntries)
                {
                    if (TryForceOverlapForEntry(Ctx, ref Character, Entry, WasGrounded) is true)
                        break;
                }

                List<CollisionContact> PostContacts = new List<CollisionContact>();
                foreach (CollisionEntry Entry in Character.CollisionEntries)
                {
                    TryBuildContactForEntry(Ctx, ref Character, Entry, PostContacts);
                }

                if (PostContacts.Count > 0)
                {
                    ResolveContacts(ref Character, PostContacts, Character.Velocity);
                }

                RemainingTime -= StepTime;
            }

            DbVector3 FinalStepVelocity = Character.IsColliding ? Character.CorrectedVelocity : Character.Velocity;

            float GroundStickVelocityThreshold = 2f;
            bool GroundedThisTick = Character.KinematicInformation.Grounded;

            if (GroundedThisTick is false && WasGrounded is true && MathF.Abs(FinalStepVelocity.y) < GroundStickVelocityThreshold)
                Character.KinematicInformation.Grounded = true;

            AdjustGrounded(Ctx, FinalStepVelocity, ref Character);

            Ctx.Db.magician.identity.Update(Character);
        }
    }


    [Reducer]
    public static void ApplyGravityMagician(ReducerContext ctx, Gravity_Timer_Magician timer)
    {
        var time = timer.tick_rate;
        foreach (var charac in ctx.Db.magician.Iter())
        {
            var character = charac;
            character.Velocity.y = character.Velocity.y > -10f ? character.Velocity.y -= timer.gravity * time : -10f;
            ctx.Db.magician.identity.Update(character);
        }
    }
    
    [Reducer]
    public static void AddCollisionEntryMagician(ReducerContext ctx, CollisionEntry Entry, Identity TargetIdentity)
    {
        Magician Magician = ctx.Db.magician.identity.Find(TargetIdentity) ?? throw new Exception("Magician (Sender) Not Found");
        if (Magician.CollisionEntries.Contains(Entry) is false) Magician.CollisionEntries.Add(Entry);
        ctx.Db.magician.identity.Update(Magician);
    }

    [Reducer]
    public static void RemoveCollisionEntryMagician(ReducerContext ctx, CollisionEntry Entry, Identity TargetIdentity)
    {
        Magician Magician = ctx.Db.magician.identity.Find(TargetIdentity) ?? throw new Exception("Magician (Sender) Not Found");
        if (Magician.CollisionEntries.Contains(Entry) is true) Magician.CollisionEntries.Remove(Entry);
        ctx.Db.magician.identity.Update(Magician);
    }
    
}




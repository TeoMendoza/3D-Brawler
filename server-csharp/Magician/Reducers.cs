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

        ctx.Db.magician.identity.Update(Character);
    }   

    [Reducer]
    public static void HandleActionChangeRequestMagician(ReducerContext Ctx, ActionRequestMagician Request)
    {
        Identity Identity = Ctx.Sender;
        Magician Character = Ctx.Db.magician.identity.Find(Identity) ?? throw new Exception("Magician Not Found");
        MagicianState OldState = Character.State;

        if (Request.State is MagicianState.Attack && GetPermissionEntry(Character.PlayerPermissionConfig, "CanAttack").Subscribers.Count == 0 && Character.Bullets.Count > 0)
        {
            Character.State = MagicianState.Attack;
            AddSubscriberUnique(GetPermissionEntry(Character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
            AddSubscriberUnique(GetPermissionEntry(Character.PlayerPermissionConfig, "CanReload").Subscribers, "Attack");
            TryPerformAttack(Ctx, ref Character, Request.AttackInformation);
        }

        else if (Request.State is MagicianState.Reload && GetPermissionEntry(Character.PlayerPermissionConfig, "CanReload").Subscribers.Count == 0 && Character.Bullets.Count < Character.BulletCapacity)
        {
            Character.State = MagicianState.Reload;
            AddSubscriberUnique(GetPermissionEntry(Character.PlayerPermissionConfig, "CanReload").Subscribers, "Reload");
        }

        bool StateSwitched = OldState != Character.State;
        if (StateSwitched)
        {
            ResetAllTimers(ref Character);
            switch (OldState)
            {
                case MagicianState.Attack:
                    RemoveSubscriber(GetPermissionEntry(Character.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
                    RemoveSubscriber(GetPermissionEntry(Character.PlayerPermissionConfig, "CanReload").Subscribers, "Attack");
                    break;

                case MagicianState.Reload:
                    RemoveSubscriber(GetPermissionEntry(Character.PlayerPermissionConfig, "CanReload").Subscribers, "Reload");
                    break;

                default:
                    break;
            }
        }

        Ctx.Db.magician.identity.Update(Character);
    }

    [Reducer]
    public static void HandleMagicianTimers(ReducerContext Ctx, Handle_Magician_Timers_Timer timer)
    {
        float Time = timer.tick_rate;
        foreach (Magician magician in Ctx.Db.magician.MatchId.Filter(timer.MatchId))
        {
            // If Certain Timers Ever Get Introduced That Are Irrelevant To Magician State, Seperate Timers Into Two Properties: Stateless and Stateful Timers, One That Relies On The State To Know Which Timer To Adjust, One That Adjusts All The Timers Regardless Of State. This Would Be The Stateful Timer
            Magician Magician = magician;
            Timer Timer;
            switch (Magician.State)
            {
                case MagicianState.Attack:
                    Timer = Magician.Timers[TryFindTimerIndex(ref Magician, "Attack")];
                    Timer.CurrentTime -= Time;

                    if (Timer.CurrentTime <= 0) 
                    {
                        Timer.CurrentTime = Timer.ResetTime;
                        if (Magician.Bullets.Count > 0)
                            Magician.State = MagicianState.Default;
                        
                        else {
                            Magician.State = MagicianState.Reload;
                            AddSubscriberUnique(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanReload").Subscribers, "Reload");
                        }

                        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanAttack").Subscribers, "Attack");
                        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanReload").Subscribers, "Attack");  
                    }

                    Magician.Timers[TryFindTimerIndex(ref Magician, "Attack")] = Timer;
                    break;
                
                case MagicianState.Reload:
                    Timer = Magician.Timers[TryFindTimerIndex(ref Magician, "Reload")];
                    Timer.CurrentTime -= Time;

                    if (Timer.CurrentTime <= 0) 
                    {
                        Timer.CurrentTime = Timer.ResetTime;
                        Magician.State = MagicianState.Default;
                        RemoveSubscriber(GetPermissionEntry(Magician.PlayerPermissionConfig, "CanReload").Subscribers, "Reload");
                        TryReload(Ctx, ref Magician);
                    }

                    Magician.Timers[TryFindTimerIndex(ref Magician, "Reload")] = Timer;
                    break;

                case MagicianState.Default:
                    break;

                default:
                    break;
            }

            Ctx.Db.magician.identity.Update(Magician);
        }
    }

    [Reducer]
    public static void ApplyGravityMagician(ReducerContext ctx, Gravity_Timer_Magician Timer)
    {
        var Time = Timer.tick_rate;
        foreach (var charac in ctx.Db.magician.MatchId.Filter(Timer.MatchId))
        {
            var character = charac;
            character.Velocity.y = character.Velocity.y > -10f ? character.Velocity.y -= Timer.gravity * Time : -10f;
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

    [Reducer]
    public static void MoveMagicians(ReducerContext Ctx, Move_All_Magicians_Timer Timer)
    {
        float TickTime = Timer.tick_rate;
        float MinTimeStep = 1e-4f;
        int MaxSubsteps = 4;

        foreach (var CharacterRow in Ctx.Db.magician.MatchId.Filter(Timer.MatchId))
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
    
}




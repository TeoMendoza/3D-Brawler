using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    [Reducer]
    public static void SpawnThrowingCard(ReducerContext ctx,  DbVector3 CameraPositionOffset, float CameraYawOffset, float CameraPitchOffset, DbVector3 HandPositionOffset, float MaxDistance)
    {
        var Magician = ctx.Db.magician.identity.Find(ctx.Sender) ?? throw new Exception("Owner not found");
        DbVector3 MagicianPosition = Magician.Position;

        float MagYawRad = ToRadians(Magician.Rotation.Yaw);
        float MagPitchRad = ToRadians(Magician.Rotation.Pitch);
        Quaternion MagicianYawOnly = Quaternion.CreateFromYawPitchRoll(MagYawRad, 0f, 0f);

        DbVector3 HandPosition = Add(MagicianPosition, Rotate(HandPositionOffset, MagicianYawOnly));

        float TotalYawRad = ToRadians(Magician.Rotation.Yaw + CameraYawOffset);
        float TotalPitchRad = ToRadians(Magician.Rotation.Pitch + CameraPitchOffset);
        Quaternion CameraRotation = Quaternion.CreateFromYawPitchRoll(TotalYawRad, TotalPitchRad, 0f);

        DbVector3 CameraPosition = Add(MagicianPosition, Rotate(CameraPositionOffset, CameraRotation));

        DbVector3 BaseCameraForward = new(0f, 0f, 1f);
        DbVector3 CameraForward = Normalize(Rotate(BaseCameraForward, CameraRotation));

        DbVector3 ThrowingCardTarget = RaycastFromCamera(ctx, Magician, new Raycast(CameraPosition, CameraForward, MaxDistance));
        DbVector3 ThrowingCardDirection = Normalize(Sub(ThrowingCardTarget, HandPosition));

        ctx.Db.throwing_cards.Insert(new ThrowingCard
        {
            OwnerIdentity = Magician.identity,
            MatchId = Magician.MatchId,
            position = HandPosition,
            direction = ThrowingCardDirection,
            velocity = Mul(ThrowingCardDirection, 50f),
            Collider = new CapsuleCollider { Center = HandPosition, Direction = ThrowingCardDirection, HeightEndToEnd = 0.5f, Radius = 0.05f }, // 0.1f, 0.025f Original, extended to try and compensate for speed
        });
    }

    [Reducer]
    public static void MoveThrowingCards(ReducerContext ctx, Move_ThrowingCards_Timer timer)
    {
        var time = timer.tick_rate;
        foreach (ThrowingCard throwingCard in ctx.Db.throwing_cards.Iter())
        {
            var ThrowingCard = throwingCard;
            ThrowingCard.position = new DbVector3(
            ThrowingCard.position.x + ThrowingCard.velocity.x * time,
            ThrowingCard.position.y + ThrowingCard.velocity.y * time,
            ThrowingCard.position.z + ThrowingCard.velocity.z * time
            );

            ThrowingCard.Collider.Center = Add(ThrowingCard.position, Mul(ThrowingCard.Collider.Direction, ThrowingCard.Collider.HeightEndToEnd * 0.5f));
            ctx.Db.throwing_cards.Id.Update(ThrowingCard);
        }
    }

    // To Be Changed / Updated
    public static DbVector3 RaycastFromCamera(ReducerContext ctx, Magician Sender, Raycast Raycast)
    {

        DbVector3 Closest = Add(Raycast.Position, Mul(Raycast.Forward, Raycast.MaxDistance));
        float ClosestScalar = Raycast.MaxDistance;
        const float EPS = 1e-4f;

        foreach (var Magician in ctx.Db.magician.Iter()) // Switch to a generalized table that stores all objects, but only their position and collider data, to make raycasting all necessary objects easier at some point
        {
            if (Magician.MatchId != Sender.MatchId || Magician.Id == Sender.Id) continue;

            if (TryRaycastOverlap(GetColliderShape(Magician.Collider), Magician.Collider, Raycast, out DbVector3 Hit))
            {
                float Scalar = Dot(Sub(Hit, Raycast.Position), Raycast.Forward);
                if (Scalar <= ClosestScalar && Scalar >= EPS)
                {
                    ClosestScalar = Scalar;
                    Closest = Hit;
                }
            }
        }

        return Closest;
    }

    // public static void AdjustCollider(ReducerContext ctx, ref Magician Magician)
    // {
    //     KinematicInformation KinematicInformation = Magician.KinematicInformation;

    //     if (KinematicInformation.Grounded is true) {
    //         Magician.GjkCollider = KinematicInformation.Crouched is true ? MagicianCrouchingCollider : MagicianIdleCollider;
    //     }

    //     else {
    //         Magician.GjkCollider = KinematicInformation.Falling is true ? MagicianFallingCollider : MagicianJumpingCollider;
    //     }
    // }
}



    
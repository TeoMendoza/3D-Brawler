using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
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

    public static void AdjustCollider(ReducerContext ctx, Magician Magician)
    {
        KinematicInformation KinematicInformation = Magician.KinematicInformation;
        if (KinematicInformation.Grounded is true)
        {
            Magician.GjkCollider = KinematicInformation.Crouched is true ? MagicianCrouchingCollider : MagicianIdleCollider;
        }

        else
        {
            Magician.GjkCollider = KinematicInformation.Falling is true ? MagicianFallingCollider : MagicianJumpingCollider;
        }
    }

}

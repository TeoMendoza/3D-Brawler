using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public static DbVector3 RaycastFromCamera(ReducerContext ctx, Magician Sender, Raycast Raycast)
    {

        DbVector3 Closest = Add(Raycast.Position, Mul(Raycast.Forward, Raycast.MaxDistance));
        Log.Info($"Raycast Forward: {Raycast.Forward}");
        float ClosestScalar = Raycast.MaxDistance;
        const float EPS = 1e-4f;

        foreach (var Magician in ctx.Db.magician.Iter()) // Switch to a generalized table that stores all objects, but only their position and collider data, to make raycasting all necessary objects easier at some point
        {
            if (Magician.MatchId != Sender.MatchId || Magician.Id == Sender.Id) continue;

            if (TryRaycastOverlap(GetColliderShape(Magician.Collider), Magician.Collider, Raycast, out DbVector3 Hit))
            {
                Log.Info("Raycast Hit");
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

    static float WrapDeg(float d)
    {
        d = (d + 180f) % 360f;
        if (d < 0f) d += 360f;
        return d - 180f;
    }

}

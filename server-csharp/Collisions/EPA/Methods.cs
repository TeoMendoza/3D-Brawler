using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    static DbVector3 ComputeContactNormal(DbVector3 RawNormal, DbVector3 CenterA, DbVector3 CenterB)
    {
        DbVector3 Normal = RawNormal;
        if (Dot(Normal, Normal) < 1e-6f) return new DbVector3(0f, 1f, 0f);
        Normal = Normalize(Normal);

        DbVector3 CenterDelta = Sub(CenterA, CenterB);
        float CenterDeltaSq = Dot(CenterDelta, CenterDelta);

        if (CenterDeltaSq > 1e-8f)
        {
            if (Dot(Normal, CenterDelta) < 0f)
                Normal = Negate(Normal);
        }

        DbVector3 WorldUp = new(0f, 1f, 0f);
        float UpDot = Dot(Normal, WorldUp);

        float FloorSnapDot = 0.98f;   // ~11 degrees of up
        float CeilingSnapDot = 0.98f; // ~11 degrees of down
        float WallSnapAbsDot = 0.05f; // ~87-93 degrees of up (horizontal)

        // FLOOR SNAP (nearly flat)
        if (UpDot >= FloorSnapDot)
        {
            return WorldUp;
        }

        // CEILING SNAP (nearly flat but inverted)
        if (UpDot <= -CeilingSnapDot)
        {
            return new DbVector3(0f, -1f, 0f);
        }

        // WALL SNAP (nearly vertical surface contact)
        if (MathF.Abs(UpDot) <= WallSnapAbsDot)
        {
            Normal.y = 0f;
            if (Dot(Normal, Normal) < 1e-6f) return new DbVector3(0f, 1f, 0f);
            return Normalize(Normal);
        }

        // SLOPE / RAMP (keep true normal)
        return Normal;
    }

}
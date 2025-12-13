using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    static DbVector3 ComputeContactNormal(DbVector3 RawNormal, DbVector3 CenterA, DbVector3 CenterB)
    {
        DbVector3 Normal = RawNormal;
        if (Dot(Normal, Normal) < 1e-6f) return new DbVector3(0, 1, 0);
        Normal = Normalize(Normal);

        DbVector3 CenterDelta = Sub(CenterA, CenterB);
        float CenterDeltaSq = Dot(CenterDelta, CenterDelta);

        if (CenterDeltaSq > 1e-8f)
        {
            if (Dot(Normal, CenterDelta) < 0f)
                Normal = Negate(Normal);
        }

        DbVector3 WorldUp = new(0f, 1f, 0f);
        float MinGroundDot = 0.7f; // 45 degrees
        float UpDot = Dot(Normal, WorldUp);
        
        // FLOOR CASE
        if (UpDot > MinGroundDot)
        {
            Normal = WorldUp;
        }
        // CEILING CASE
        else if (UpDot < -MinGroundDot)
        {
            Normal = new DbVector3(0f, -1f, 0f);
        }
        // WALL CASE (Vertical)
        else if (MathF.Abs(UpDot) < 0.05f)
        {
            Normal.y = 0f;
            Normal = Normalize(Normal);
        }

        // SLOPE CASE (Default)
        // Returns RawNormal

        return Normal;
    }
}
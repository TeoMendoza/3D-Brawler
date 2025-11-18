using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    static DbVector3 Add(in DbVector3 x, in DbVector3 y) => new(x.x + y.x, x.y + y.y, x.z + y.z);
    static DbVector3 Sub(in DbVector3 x, in DbVector3 y) => new(x.x - y.x, x.y - y.y, x.z - y.z);
    static DbVector3 Mul(in DbVector3 x, float s) => new(x.x * s, x.y * s, x.z * s);
    static float Dot(in DbVector3 x, in DbVector3 y) => x.x * y.x + x.y * y.y + x.z * y.z;
    static float LenSq(in DbVector3 x) => Dot(x, x);
    static float Sqrt(float v) => (float)Math.Sqrt(v);
    static float Magnitude(in DbVector3 x) => Sqrt(Dot(x, x));
    static float Clamp01(float t) => t < 0f ? 0f : (t > 1f ? 1f : t);
    static float Clamp(float x, float a, float b) => x < a ? a : (x > b ? b : x);
    static float ToRadians(float Degrees) => Degrees * (MathF.PI / 180f);
    static DbVector3 Cross(in DbVector3 a, in DbVector3 b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    private static Vector3 ToVector3(DbVector3 v) => new(v.x, v.y, v.z);
    private static DbVector3 ToDbVector3(Vector3 v) => new(v.X, v.Y, v.Z);
    private static DbVector3 Rotate(DbVector3 v, Quaternion q) => ToDbVector3(Vector3.Transform(ToVector3(v), q));
    static DbVector3 Negate(DbVector3 Vector) => new(-Vector.x, -Vector.y, -Vector.z);
    static DbVector3 TripleCross(DbVector3 VectorA, DbVector3 VectorB, DbVector3 VectorC) => Cross(Cross(VectorA, VectorB), VectorC);
    static DbVector3 NormalizeSmallVector(in DbVector3 v, in DbVector3 fallback)
    {
        float magSq = LenSq(v);
        if (magSq <= 1e-12f) return fallback;
        float invMag = 1f / Sqrt(magSq);
        return new DbVector3(v.x * invMag, v.y * invMag, v.z * invMag);
    }

    static DbVector3 AnyPerpendicularUnit(in DbVector3 unitAxis)
    {
        var refVec = Math.Abs(unitAxis.y) < 0.99f ? new DbVector3(0, 1, 0) : new DbVector3(1, 0, 0);
        var perp = Cross(unitAxis, refVec);
        return NormalizeSmallVector(perp, new DbVector3(1, 0, 0));
    }

    private static DbVector3 Normalize(DbVector3 v)
    {
        float Magnitude = Module.Magnitude(v);
        if (Magnitude <= 1e-6f) return new DbVector3(0f, 0f, 0f);
        return new DbVector3(v.x / Magnitude, v.y / Magnitude, v.z / Magnitude);
    }

    static bool NearZero(DbVector3 Vector)
    {
        float MagnitudeSquared = Dot(Vector, Vector);
        return MagnitudeSquared <= 1e-12f;
    }

    static DbVector3 Perp(DbVector3 Vector)
    {
        if (MathF.Abs(Vector.x) > MathF.Abs(Vector.z))
        {
            return new DbVector3(-Vector.y, Vector.x, 0f);
        }

        return new DbVector3(0f, -Vector.z, Vector.y);
    }
}
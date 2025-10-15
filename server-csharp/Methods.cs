using System.Numerics;
using SpacetimeDB;

public static partial class Module
{

    public static Shape GetColliderShape(object collider)
    {
        return collider switch
        {
            SphereCollider => Shape.Sphere,
            CapsuleCollider => Shape.Capsule,
            BoxCollider => Shape.Box,
            _ => throw new ArgumentOutOfRangeException(nameof(collider), collider, "Unknown collider type")
        };
    }

    public delegate bool OverlapFn(object a, object b, out Contact contact);

    static readonly Dictionary<(Shape, Shape), OverlapFn> Overlap = new()
    {
        { (Shape.Capsule, Shape.Capsule), (object a, object b, out Contact c) => OverlapCapsuleCapsule((CapsuleCollider)a, (CapsuleCollider)b, out c) },
    };

    static bool TryOverlap(Shape sa, object ca, Shape sb, object cb, out Contact contact)
    {
        if (Overlap.TryGetValue((sa, sb), out var fn))
            return fn(ca, cb, out contact);

        contact = default;
        return false;
    }

    static bool OverlapCapsuleCapsule(CapsuleCollider a, CapsuleCollider b, out Contact contact)
    {
        static float Length(DbVector3 v) => Sqrt(LenSq(v));

        var aDir = a.Direction;
        var bDir = b.Direction;

        ComputeSegmentEndpoints(a, out var aBottom, out var aTop);
        ComputeSegmentEndpoints(b, out var bBottom, out var bTop);
        ClosestPointsOnSegments(aBottom, aTop, bBottom, bTop, out var pA, out var pB);

        var bToAAtClosest = Sub(pA, pB);
        float distanceSq = LenSq(bToAAtClosest);
        float distance = Sqrt(distanceSq);
        float combinedR = a.Radius + b.Radius;


        if (distanceSq > combinedR * combinedR)
        {
            contact = default;
            return false;
        }

        DbVector3 contactNormal;
        if (distance > 1e-6f)
        {
            contactNormal = Mul(bToAAtClosest, 1f / distance);
        }
        else
        {
            contactNormal = NormalizeSmallVector(Sub(aDir, bDir), AnyPerpendicularUnit(aDir));
        }

        contact = new Contact
        {
            Normal = contactNormal, // B → A
            Depth  = combinedR - distance
        };

        return true;
    
    }

    static void ComputeSegmentEndpoints(in CapsuleCollider capsule, out DbVector3 bottom, out DbVector3 top)
    {
        var axisUnit = capsule.Direction;
        float cylinderLength = Math.Max(0f, capsule.HeightEndToEnd - 2f * capsule.Radius);
        float halfSegment = 0.5f * cylinderLength;

        var offset = Mul(axisUnit, halfSegment);
        bottom = Sub(capsule.Center, offset);
        top = Add(capsule.Center, offset);
    }

    static void ClosestPointsOnSegments(in DbVector3 a0, in DbVector3 a1, in DbVector3 b0, in DbVector3 b1, out DbVector3 closestOnA, out DbVector3 closestOnB)
    {
        var aDir = Sub(a1, a0);
        var bDir = Sub(b1, b0);
        var a0ToB0 = Sub(a0, b0);

        float aa = Dot(aDir, aDir);
        float ab = Dot(aDir, bDir);
        float bb = Dot(bDir, bDir);
        float ad = Dot(aDir, a0ToB0);
        float bd = Dot(bDir, a0ToB0);
        float D  = aa * bb - ab * ab;

        const float EPS = 1e-8f;

        float sN, sD = D, tN, tD = D;

        if (D < EPS)
        {
            sN = 0f; sD = 1f;
            tN = bd; tD = bb;
        }
        else
        {
            sN = ab * bd - bb * ad;
            tN = aa * bd - ab * ad;

            if (sN < 0f)
            {
                sN = 0f;
                tN = bd;
                tD = bb;
            }
            else if (sN > sD)
            {
                sN = sD;
                tN = bd + ab;
                tD = bb;
            }
        }

        if (tN < 0f)
        {
            tN = 0f;
            if (-ad < 0f) { sN = 0f;  sD = 1f; }
            else if (-ad > aa) { sN = sD; }
            else { sN = -ad; sD = aa; }
        }
        else if (tN > tD)
        {
            tN = tD;
            if (-ad + ab < 0f) { sN = 0f;  sD = 1f; }
            else if (-ad + ab > aa) { sN = sD; }
            else { sN = -ad + ab; sD = aa; }
        }

        float s = MathF.Abs(sN) < EPS ? 0f : (sN / sD);
        float t = MathF.Abs(tN) < EPS ? 0f : (tN / tD);

        s = Clamp01(s);
        t = Clamp01(t);

        closestOnA = Add(a0, Mul(aDir, s));
        closestOnB = Add(b0, Mul(bDir, t));
    }

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

    static DbVector3 Add(in DbVector3 x, in DbVector3 y) => new DbVector3(x.x + y.x, x.y + y.y, x.z + y.z);
    static DbVector3 Sub(in DbVector3 x, in DbVector3 y) => new DbVector3(x.x - y.x, x.y - y.y, x.z - y.z);
    static DbVector3 Mul(in DbVector3 x, float s) => new DbVector3(x.x * s, x.y * s, x.z * s);
    static float Dot(in DbVector3 x, in DbVector3 y) => x.x * y.x + x.y * y.y + x.z * y.z;
    static float LenSq(in DbVector3 x) => Dot(x, x);
    static float Sqrt(float v) => (float)Math.Sqrt(v);
    static float Clamp01(float t) => t < 0f ? 0f : (t > 1f ? 1f : t);
    static DbVector3 Cross(in DbVector3 a, in DbVector3 b) => new(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    static bool IsZero(DbVector3 v) => (v.x * v.x + v.y * v.y + v.z * v.z) < 1e-10f;

    static void AddSubscriberUnique(List<string> subscribers, string reason)
    {
        if (subscribers.Contains(reason)) return;
        subscribers.Add(reason);
    }

    static void RemoveSubscriber(List<string> subscribers, string reason)
    {
        for (int i = subscribers.Count - 1; i >= 0; i--)
            if (subscribers[i] == reason) { subscribers.RemoveAt(i); break; }
    }

    private static PermissionEntry GetPermissionEntry(List<PermissionEntry> entries, string key)
    {
        foreach (var entry in entries)
        {
            if (entry.Key == key)
                return entry;
        }
        return entries[0];
    }

}
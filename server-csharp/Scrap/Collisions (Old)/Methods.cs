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

    public delegate bool RaycastOverlapFn(object a, Raycast2 Raycast, out DbVector3 Position);

    static readonly Dictionary<Shape, RaycastOverlapFn> RaycastOverlap = new()
    {
        {  Shape.Capsule, (object a, Raycast2 Raycast, out DbVector3 p) => OverlapRaycastCapsule((CapsuleCollider)a, Raycast, out p) },
    };

    static bool TryRaycastOverlap(Shape sa, object ca, Raycast2 Raycast, out DbVector3 Position)
    {
        if (RaycastOverlap.TryGetValue(sa, out var fn))
            return fn(ca, Raycast, out Position);

        Position = default;
        return false;
    }
    
    static bool OverlapRaycastCapsule(CapsuleCollider Capsule, Raycast2 Raycast, out DbVector3 Position)
    {
        Position = default;

        DbVector3 ro = Raycast.Position;
        DbVector3 rd = Normalize(Raycast.Forward);
        float tMax = Raycast.MaxDistance;   

        DbVector3 c = Capsule.Center;
        DbVector3 ax = Capsule.Direction;
        float r = Capsule.Radius;
        float halfCylLen = Math.Max(0f, Capsule.HeightEndToEnd * 0.5f - r);

        DbVector3 pa = Sub(c, Mul(ax, halfCylLen));
        DbVector3 pb = Add(c, Mul(ax, halfCylLen));

        DbVector3 ba = Sub(pb, pa);
        DbVector3 oa = Sub(ro, pa);

        float baba = Dot(ba, ba);
        float bard = Dot(ba, rd);
        float baoa = Dot(ba, oa);
        float rdoa = Dot(rd, oa);
        float oaoa = Dot(oa, oa);

        float k2 = baba - bard * bard;
        float k1 = baba * rdoa - baoa * bard;
        float k0 = baba * oaoa - baoa * baoa - r * r * baba;

        float tHit = float.PositiveInfinity;

        const float EPS = 1e-8f;
        if (k2 > EPS)
        {
            float h = k1 * k1 - k2 * k0;
            if (h >= 0f)
            {
                float t = (-k1 - (float)Math.Sqrt(h)) / k2;
                if (t >= 0f && t <= tMax)
                {
                    float y = baoa + t * bard;
                    if (y > 0f && y < baba) tHit = t;
                }
            }
        }

        if (float.IsPositiveInfinity(tHit))
        {
            float yCap = Clamp(baoa, 0f, baba);
            DbVector3 oc = yCap <= 0f ? oa : Sub(ro, pb);
            float b = Dot(rd, oc);
            float c2 = Dot(oc, oc) - r * r;
            float h = b * b - c2;
            if (h >= 0f)
            {
                float t = -b - (float)Math.Sqrt(h);
                if (t >= 0f && t <= tMax) tHit = t;
            }
        }

        if (float.IsPositiveInfinity(tHit)) return false;

        Position = Add(ro, Mul(rd, tHit));
        return true;
    }

    static bool OverlapCapsuleCapsule(CapsuleCollider a, CapsuleCollider b, out Contact contact)
    {
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
            Depth = combinedR - distance
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
}
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    
    public interface ICollider
    {
        DbVector3 SupportLocal(DbVector3 Direction);
    }

    public abstract class ColliderBase : ICollider
    {
        public float Margin { get; protected set; }
        public abstract DbVector3 SupportLocal(DbVector3 Direction);
    }

    public class ConvexHullCollider : ColliderBase
    {
        public List<DbVector3> VerticesLocal { get; }
        public ConvexHullCollider(List<DbVector3> verticesLocal, float margin)
        {
            VerticesLocal = verticesLocal;
            Margin = margin;
        }

        public override DbVector3 SupportLocal(DbVector3 Direction)
        {
            int Best = 0;
            float BestDot = Dot(VerticesLocal[0], Direction);
            for (int i = 1; i < VerticesLocal.Count; i++)
            {
                float D = Dot(VerticesLocal[i], Direction);
                if (D > BestDot) { BestDot = D; Best = i; }
            }
            var P = VerticesLocal[Best];
            if (Margin > 0f)
            {
                var N = Normalize(Direction);
                return new DbVector3(P.x + N.x * Margin, P.y + N.y * Margin, P.z + N.z * Margin);
            }
            return P;
        }
    }

    public class CompoundCollider : ColliderBase
    {
        public List<CompoundChildBase> Children { get; }

        public CompoundCollider(List<CompoundChildBase> children, float margin = 0)
        {
            Children = children;
            Margin = margin;
        }

        public override DbVector3 SupportLocal(DbVector3 Direction)
        {
            float Best = float.NegativeInfinity;
            DbVector3 BestPoint = default;
            for (int i = 0; i < Children.Count; i++)
            {
                var P = Children[i].SupportInParent(Direction);
                float D = Dot(P, Direction);
                if (D > Best) { Best = D; BestPoint = P; }
            }
            if (Margin > 0f)
            {
                var N = Normalize(Direction);
                return new DbVector3(BestPoint.x + N.x * Margin, BestPoint.y + N.y * Margin, BestPoint.z + N.z * Margin);
            }
            return BestPoint;
        }
    }


    public interface ICompoundChild
    {
        public DbVector3 SupportInParent(DbVector3 DirectionParent);
    }

    public abstract class CompoundChildBase : ICompoundChild
    {
        public DbVector3 LocalPosition { get; protected set; }
        protected abstract ColliderBase Collider { get; }

        public DbVector3 SupportInParent(DbVector3 DirectionParent)
        {
            var PChild = Collider.SupportLocal(DirectionParent); 
            return Add(PChild, LocalPosition);
        }

    }

    public class ConvexHullChild : CompoundChildBase
    {
        protected override ConvexHullCollider Collider { get; }
        public ConvexHullChild(ConvexHullCollider collider, DbVector3 localPosition)
        {
            Collider = collider;
            LocalPosition = localPosition;
        }
    }

    

    public struct GjkVertex(DbVector3 PA, Module.DbVector3 PB)
    {
        public DbVector3 PA = PA;
        public DbVector3 PB = PB;
        public DbVector3 W = Sub(PA, PB);
    }

    public struct GjkResult
    {
        public bool Intersects;
        public List<GjkVertex> Simplex;
        public DbVector3 LastDirection;
    }

    public static bool SolveGjk(CompoundCollider A, CompoundCollider B, out GjkResult Result, int MaxIterations = 32)
    {
        var S = new List<GjkVertex>(4);
        DbVector3 D = new(1f, 0f, 0f);

        var V0 = SupportPair(A, B, D);
        if (Dot(V0.W, D) <= 0f) { Result = new GjkResult { Intersects = false, Simplex = S, LastDirection = D }; return false; }
        S.Add(V0);
        D = Negate(V0.W);

        for (int I = 0; I < MaxIterations; I++)
        {
            var V = SupportPair(A, B, D);
            if (Dot(V.W, D) <= 0f) { Result = new GjkResult { Intersects = false, Simplex = S, LastDirection = D }; return false; }
            S.Add(V);
            if (UpdateSimplex(ref S, ref D)) { Result = new GjkResult { Intersects = true, Simplex = S, LastDirection = D }; return true; }
        }

        Result = new GjkResult { Intersects = false, Simplex = S, LastDirection = D };
        return false;
    }


    static GjkVertex SupportPair(CompoundCollider A, CompoundCollider B, DbVector3 D)
    {
        var PA = A.SupportLocal(D);
        var PB = B.SupportLocal(Negate(D));
        return new GjkVertex(PA, PB);
    }

    static bool UpdateSimplex(ref List<GjkVertex> S, ref DbVector3 D)
    {
        if (S.Count == 2)
        {
            var A = S[1].W;
            var B = S[0].W;
            var AB = Sub(B, A);
            var AO = Negate(A);
            var Dir = TripleCross(AB, AO, AB);
            if (NearZero(Dir)) Dir = Perp(AB);
            D = Dir;
            return false;
        }

        if (S.Count == 3)
        {
            var A = S[2].W;
            var B = S[1].W;
            var C = S[0].W;

            var AB = Sub(B, A);
            var AC = Sub(C, A);
            var AO = Negate(A);
            var ABC = Cross(AB, AC);

            var ABPerp = Cross(ABC, AB);
            if (Dot(ABPerp, AO) > 0f)
            {
                S.RemoveAt(0);
                var Dir = TripleCross(AB, AO, AB);
                if (NearZero(Dir)) Dir = Perp(AB);
                D = Dir;
                return false;
            }

            var ACPerp = Cross(AC, ABC);
            if (Dot(ACPerp, AO) > 0f)
            {
                S.RemoveAt(1);
                var Dir = TripleCross(AC, AO, AC);
                if (NearZero(Dir)) Dir = Perp(AC);
                D = Dir;
                return false;
            }

            D = ABC;
            if (Dot(D, AO) < 0f) D = Negate(D);
            return false;
        }

        if (S.Count == 4)
        {
            var A = S[3].W;
            var B = S[2].W;
            var C = S[1].W;
            var D0 = S[0].W;

            var AO = Negate(A);
            var AB = Sub(B, A);
            var AC = Sub(C, A);
            var AD = Sub(D0, A);

            var ABC = Cross(AB, AC);
            var ACD = Cross(AC, AD);
            var ADB = Cross(AD, AB);

            if (Dot(ABC, AO) > 0f) { S.RemoveAt(0); D = ABC; return false; }
            if (Dot(ACD, AO) > 0f) { S.RemoveAt(2); D = ACD; return false; }
            if (Dot(ADB, AO) > 0f) { S.RemoveAt(1); D = ADB; return false; }

            return true;
        }

        D = Negate(S[0].W);
        return false;
    }

    static DbVector3 Negate(DbVector3 A) => new DbVector3(-A.x, -A.y, -A.z);
    static DbVector3 TripleCross(DbVector3 A, DbVector3 B, DbVector3 C) => Cross(Cross(A, B), C);
    static bool NearZero(DbVector3 V) { float m2 = Dot(V, V); return m2 <= 1e-12f; }
    static DbVector3 Perp(DbVector3 V) { if (MathF.Abs(V.x) > MathF.Abs(V.z)) return new DbVector3(-V.y, V.x, 0f); return new DbVector3(0f, -V.z, V.y); }

}
using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public partial struct ComplexCollider(List<ConvexHullCollider> convexHulls)
    {
        public List<ConvexHullCollider> ConvexHulls = convexHulls;
    }

    [SpacetimeDB.Type]
    public partial struct ConvexHullCollider(List<DbVector3> verticesLocal, float margin)
    {
        public List<DbVector3> VerticesLocal = verticesLocal;
        public float Margin = margin;
    }

    public struct GjkVertex(DbVector3 SupportPointA, DbVector3 SupportPointB)
    {
        public DbVector3 SupportPointA = SupportPointA;
        public DbVector3 SupportPointB = SupportPointB;
        public DbVector3 MinkowskiPoint = Sub(SupportPointA, SupportPointB);
    }

    public struct GjkResult(bool Intersects, List<GjkVertex> Simplex, DbVector3 LastDirection)
    {
        public bool Intersects = Intersects;
        public List<GjkVertex> Simplex = Simplex;
        public DbVector3 LastDirection = LastDirection;
    }

    [SpacetimeDB.Type]
    public partial struct CollisionEntry(CollisionEntryType type, uint id)
    {
        public CollisionEntryType Type = type;
        public uint Id = id;

    }

    [SpacetimeDB.Type]
    public enum CollisionEntryType
    {
        Magician,
        ThrowingCard,
        Map,
    }
}
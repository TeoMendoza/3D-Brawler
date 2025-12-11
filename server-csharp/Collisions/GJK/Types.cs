using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [SpacetimeDB.Type]
    public partial struct ComplexCollider(List<ConvexHullCollider> convexHulls, DbVector3 centerPoint)
    {
        public List<ConvexHullCollider> ConvexHulls = convexHulls;
        public DbVector3 CenterPoint = centerPoint;
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

    public struct GjkResult(bool Intersects, List<GjkVertex> Simplex, DbVector3 LastDirection, DbVector3 ClosestPointA, DbVector3 ClosestPointB)
    {
        public bool Intersects = Intersects;
        public List<GjkVertex> Simplex = Simplex;
        public DbVector3 LastDirection = LastDirection;
        public DbVector3 ClosestPointA;
        public DbVector3 ClosestPointB;
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
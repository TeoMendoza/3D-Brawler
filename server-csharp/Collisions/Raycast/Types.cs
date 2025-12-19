using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public enum RaycastHitType
    {
        None = 0,
        Magician = 1,
        MapPiece = 2
    }

    public partial struct Raycast(bool hit, float hitDistance, DbVector3 hitPoint, RaycastHitType hitType, Identity hitIdentity, long hitEntityId)
    {
        public bool Hit = hit;
        public float HitDistance = hitDistance;
        public DbVector3 HitPoint = hitPoint;
        public RaycastHitType HitType = hitType;
        public Identity HitIdentity = hitIdentity;
        public long HitEntityId = hitEntityId;
    }
}


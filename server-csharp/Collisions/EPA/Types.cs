using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public struct CollisionContact(DbVector3 Normal, float PenetrationDepth, CollisionEntryType collisionType)
    {
        public DbVector3 Normal = Normal; // Object B -> A
        public float PenetrationDepth = PenetrationDepth;
        public CollisionEntryType CollisionType = collisionType;
    }

    public struct EpaFace
    {
        public int IndexA;
        public int IndexB;
        public int IndexC;
        public DbVector3 Normal;
        public float Distance;
        public bool Obsolete;
    }

    public struct EpaEdge
    {
        public int IndexA;
        public int IndexB;
        public bool Obsolete;
    }
}
using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    public struct GjkDistanceResult
    {
        public bool Intersects;
        public float Distance;
        public DbVector3 SeparationDirection;
        public DbVector3 PointOnA;
        public DbVector3 PointOnB;
        public List<GjkVertex> Simplex;
        public DbVector3 LastDirection;
    }
}
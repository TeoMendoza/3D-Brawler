using System.Diagnostics.Contracts;
using System.Numerics;
using SpacetimeDB;

public static partial class Module
{
    [Table(Name = "Map", Public = true)]
    public partial struct Map
    {
        [PrimaryKey, AutoInc]
        public uint Id;
        public ComplexCollider GjkCollider;
    }
}
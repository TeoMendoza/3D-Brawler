using SpacetimeDB.Types;
using UnityEngine;

namespace SpacetimeDB.Types
{
    public partial class DbVector3
    {
        public static implicit operator Vector3(DbVector3 vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        public static implicit operator DbVector3(Vector3 vec)
        {
            return new DbVector3(vec.x, vec.y, vec.z);
        }
    }

    public partial class DbRotation2
    {
        public static implicit operator Vector3(DbRotation2 vec)
        {
            return new Vector3(vec.Pitch, vec.Yaw, 0);
        }

        public static implicit operator DbRotation2(Vector3 vec)
        {
            return new DbRotation2(vec.x, vec.y);
        }

        public Quaternion ToQuaternion()
        {
            return Quaternion.Euler(Pitch, Yaw, 0f);
        }
    
        public static DbRotation2 FromQuaternion(Quaternion q)
        {
            var e = q.eulerAngles;  // degrees
            return new DbRotation2(e.x, e.y);
        }
    
    }
    
    
}
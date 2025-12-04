using System.Collections.Generic;
using UnityEngine;
using SpacetimeDB.Types;

[RequireComponent(typeof(MagicianController))]
public class PlayerHullGizmo : MonoBehaviour
{
    public float VertexRadius = 0.03f;
    public bool DrawLinesFromCenter = true;
    public Color HullColor = Color.green;

    MagicianController Magician;

    void Awake()
    {
        Magician = GetComponent<MagicianController>();
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
        {
            return;
        }

        if (Magician == null)
        {
            Magician = GetComponent<MagicianController>();
            if (Magician == null)
            {
                return;
            }
        }

        Vector3 WorldCenter = Magician.TargetPosition;
        float YawDegrees = Magician.TargetRotation.Yaw;
        Quaternion YawRotation = Quaternion.Euler(0f, YawDegrees, 0f);

        Gizmos.color = HullColor;

        foreach (List<DbVector3> HullVertices in PlayerConvexHullVerticesLocalByHull)
        {
            if (HullVertices == null)
            {
                continue;
            }

            foreach (DbVector3 VertexDb in HullVertices)
            {
                Vector3 LocalVertex = new Vector3((float)VertexDb.X, (float)VertexDb.Y, (float)VertexDb.Z);
                Vector3 RotatedVertex = YawRotation * LocalVertex;
                Vector3 WorldVertex = WorldCenter + RotatedVertex;

                Gizmos.DrawSphere(WorldVertex, VertexRadius);

                if (DrawLinesFromCenter)
                {
                    Gizmos.DrawLine(WorldCenter, WorldVertex);
                }
            }
        }
    }

    

    public static readonly List<DbVector3> IdleConvexHull0Vertices = new List<DbVector3>
{
    new DbVector3( 0.300000f,  0.000000f,  0.000000f),
    new DbVector3( 0.277164f,  0.000000f,  0.114805f),
    new DbVector3( 0.212132f,  0.000000f,  0.212132f),
    new DbVector3( 0.114805f,  0.000000f,  0.277164f),
    new DbVector3( 0.000000f,  0.000000f,  0.300000f),
    new DbVector3(-0.114805f,  0.000000f,  0.277164f),
    new DbVector3(-0.212132f,  0.000000f,  0.212132f),
    new DbVector3(-0.277164f,  0.000000f,  0.114805f),
    new DbVector3(-0.300000f,  0.000000f,  0.000000f),
    new DbVector3(-0.277164f,  0.000000f, -0.114805f),
    new DbVector3(-0.212132f,  0.000000f, -0.212132f),
    new DbVector3(-0.114805f,  0.000000f, -0.277164f),
    new DbVector3(-0.000000f,  0.000000f, -0.300000f),
    new DbVector3( 0.114805f,  0.000000f, -0.277164f),
    new DbVector3( 0.212132f,  0.000000f, -0.212132f),
    new DbVector3( 0.277164f,  0.000000f, -0.114805f),

    new DbVector3( 0.350000f,  0.850000f,  0.000000f),
    new DbVector3( 0.323358f,  0.850000f,  0.133939f),
    new DbVector3( 0.247487f,  0.850000f,  0.247487f),
    new DbVector3( 0.133939f,  0.850000f,  0.323358f),
    new DbVector3( 0.000000f,  0.850000f,  0.350000f),
    new DbVector3(-0.133939f,  0.850000f,  0.323358f),
    new DbVector3(-0.247487f,  0.850000f,  0.247487f),
    new DbVector3(-0.323358f,  0.850000f,  0.133939f),
    new DbVector3(-0.350000f,  0.850000f,  0.000000f),
    new DbVector3(-0.323358f,  0.850000f, -0.133939f),
    new DbVector3(-0.247487f,  0.850000f, -0.247487f),
    new DbVector3(-0.133939f,  0.850000f, -0.323358f),
    new DbVector3(-0.000000f,  0.850000f, -0.350000f),
    new DbVector3( 0.133939f,  0.850000f, -0.323358f),
    new DbVector3( 0.247487f,  0.850000f, -0.247487f),
    new DbVector3( 0.323358f,  0.850000f, -0.133939f),

    new DbVector3( 0.300000f,  1.700000f,  0.000000f),
    new DbVector3( 0.277164f,  1.700000f,  0.114805f),
    new DbVector3( 0.212132f,  1.700000f,  0.212132f),
    new DbVector3( 0.114805f,  1.700000f,  0.277164f),
    new DbVector3( 0.000000f,  1.700000f,  0.300000f),
    new DbVector3(-0.114805f,  1.700000f,  0.277164f),
    new DbVector3(-0.212132f,  1.700000f,  0.212132f),
    new DbVector3(-0.277164f,  1.700000f,  0.114805f),
    new DbVector3(-0.300000f,  1.700000f,  0.000000f),
    new DbVector3(-0.277164f,  1.700000f, -0.114805f),
    new DbVector3(-0.212132f,  1.700000f, -0.212132f),
    new DbVector3(-0.114805f,  1.700000f, -0.277164f),
    new DbVector3(-0.000000f,  1.700000f, -0.300000f),
    new DbVector3( 0.114805f,  1.700000f, -0.277164f),
    new DbVector3( 0.212132f,  1.700000f, -0.212132f),
    new DbVector3( 0.277164f,  1.700000f, -0.114805f),
};


    
    // Master list of all convex hull vertex lists
    public static readonly List<List<DbVector3>> PlayerConvexHullVerticesLocalByHull = new List<List<DbVector3>>
    {
        IdleConvexHull0Vertices,
    };
}

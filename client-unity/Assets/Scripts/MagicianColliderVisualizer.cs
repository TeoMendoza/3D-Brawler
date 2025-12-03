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
        new DbVector3( 0.35f,    0f,      0f),
        new DbVector3( 0.2475f,  0f,  0.2475f),
        new DbVector3( 0f,       0f,   0.35f),
        new DbVector3(-0.2475f,  0f,  0.2475f),
        new DbVector3(-0.35f,    0f,      0f),
        new DbVector3(-0.2475f,  0f, -0.2475f),
        new DbVector3( 0f,       0f,  -0.35f),
        new DbVector3( 0.2475f,  0f, -0.2475f),

        new DbVector3( 0.35f,    1.7f,      0f),
        new DbVector3( 0.2475f,  1.7f,  0.2475f),
        new DbVector3( 0f,       1.7f,   0.35f),
        new DbVector3(-0.2475f,  1.7f,  0.2475f),
        new DbVector3(-0.35f,    1.7f,      0f),
        new DbVector3(-0.2475f,  1.7f, -0.2475f),
        new DbVector3( 0f,       1.7f,  -0.35f),
        new DbVector3( 0.2475f,  1.7f, -0.2475f),
    };

    
    // Master list of all convex hull vertex lists
    public static readonly List<List<DbVector3>> PlayerConvexHullVerticesLocalByHull = new List<List<DbVector3>>
    {
        IdleConvexHull0Vertices,
    };
}

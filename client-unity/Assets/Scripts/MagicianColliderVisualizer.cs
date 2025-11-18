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

        foreach (DbVector3 VertexDb in PlayerConvexHullVerticesLocal)
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

    public static readonly List<DbVector3> PlayerConvexHullVerticesLocal = new List<DbVector3>
    {
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(4.664304E-06f, 1.017722f, -0.1605133f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.9430344f, 1.435602f, -0.1180731f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(-0.1531489f, 0.004603939f, 0.1399166f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(-0.08126362f, 0.010119f, 0.2013561f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(0.01826309f, 1.728066f, -0.1125759f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(-0.004421317f, 1.800004f, -0.04191095f),
        new DbVector3(-0.943046f, 1.435681f, -0.1180799f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(-0.9746667f, 1.439709f, -0.068678f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(0.8737144f, 1.360841f, 0.02754814f),
        new DbVector3(-0.003069865f, 1.74116f, 0.1152613f),
        new DbVector3(0.03253203f, 1.795121f, 0.06259078f),
        new DbVector3(-0.02967339f, 1.799247f, 0.05032304f),
        new DbVector3(-0.9653891f, 1.438058f, -0.02841866f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(0.9653536f, 1.437939f, -0.02829308f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.974641f, 1.439772f, -0.06878822f),
        new DbVector3(-0.08126362f, 0.010119f, 0.2013561f),
        new DbVector3(-0.8736752f, 1.360868f, 0.02756588f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(-0.0001176918f, 1.628559f, 0.1350315f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
        new DbVector3(0.1400456f, 0.00484989f, 0.1718591f),
        new DbVector3(0.09768067f, 0.01106594f, 0.1990863f),
        new DbVector3(-0.08126362f, 0.010119f, 0.2013561f),
        new DbVector3(-0.1120597f, 0.009630211f, -0.08812804f),
        new DbVector3(0.1123706f, 0.009947245f, -0.08824297f),
        new DbVector3(0.1525205f, 0.003930383f, 0.1374778f),
        new DbVector3(-0.1340702f, 0.005429628f, 0.1797838f),
    };
}

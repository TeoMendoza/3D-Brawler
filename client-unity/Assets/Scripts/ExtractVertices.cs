using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
public static class ColliderVertexExtractorTool
{
    [MenuItem("Tools/Collider/Export Convex Mesh Vertices")]
    public static void ExportConvexMeshVertices()
    {
    GameObject SelectedObject = Selection.activeGameObject;
    if (SelectedObject == null)
    {
    Debug.LogError("ColliderVertexExtractorTool ExportConvexMeshVertices requires a selected GameObject.");
    return;
    }
        MeshCollider MeshColliderComponent = SelectedObject.GetComponent<MeshCollider>();
        if (MeshColliderComponent == null)
        {
            Debug.LogError("ColliderVertexExtractorTool ExportConvexMeshVertices requires a MeshCollider on the selected GameObject.");
            return;
        }

        Mesh SourceMesh = MeshColliderComponent.sharedMesh;
        if (SourceMesh == null)
        {
            Debug.LogError("ColliderVertexExtractorTool ExportConvexMeshVertices found no mesh on the MeshCollider.");
            return;
        }

        Vector3[] LocalVertices = SourceMesh.vertices;
        Transform RootTransform = SelectedObject.transform;

        List<Vector3> RootLocalVertices = new List<Vector3>(LocalVertices.Length);
        for (int Index = 0; Index < LocalVertices.Length; Index++)
        {
            Vector3 LocalColliderVertex = LocalVertices[Index];
            Vector3 WorldVertex = RootTransform.TransformPoint(LocalColliderVertex);
            Vector3 RootLocalVertex = RootTransform.InverseTransformPoint(WorldVertex);
            RootLocalVertices.Add(RootLocalVertex);
        }

        StringBuilder Builder = new StringBuilder();
        Builder.AppendLine("ConvexHullVerticesLocal:");
        for (int Index = 0; Index < RootLocalVertices.Count; Index++)
        {
            Vector3 Vertex = RootLocalVertices[Index];
            Builder.AppendLine("new DbVector3(" + Vertex.x + "f, " + Vertex.y + "f, " + Vertex.z + "f),");
        }

        string DefaultFileName = SelectedObject.name + "_ConvexHullVertices.txt";
        string FilePath = EditorUtility.SaveFilePanel("Save Convex Hull Vertices", "", DefaultFileName, "txt");

        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        File.WriteAllText(FilePath, Builder.ToString());
        Debug.Log("ColliderVertexExtractorTool ExportConvexMeshVertices wrote vertices to file: " + FilePath);
    }
}
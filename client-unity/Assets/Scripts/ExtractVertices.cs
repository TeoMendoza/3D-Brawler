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

        Transform RootTransform = SelectedObject.transform;

        MeshFilter[] MeshFilters = SelectedObject.GetComponentsInChildren<MeshFilter>(true);
        if (MeshFilters == null || MeshFilters.Length == 0)
        {
            Debug.LogError("ColliderVertexExtractorTool ExportConvexMeshVertices requires at least one MeshFilter on children of the selected GameObject.");
            return;
        }

        StringBuilder Builder = new StringBuilder();
        Builder.AppendLine("ConvexHullVerticesLocalByHull:");

        int HullIndex = 0;

        for (int FilterIndex = 0; FilterIndex < MeshFilters.Length; FilterIndex++)
        {
            MeshFilter MeshFilterComponent = MeshFilters[FilterIndex];
            if (MeshFilterComponent == null)
            {
                continue;
            }

            if (MeshFilterComponent.gameObject == SelectedObject)
            {
                continue;
            }

            Mesh SourceMesh = MeshFilterComponent.sharedMesh;
            if (SourceMesh == null)
            {
                Debug.LogWarning("ColliderVertexExtractorTool ExportConvexMeshVertices found no mesh on MeshFilter: " + MeshFilterComponent.gameObject.name);
                continue;
            }

            Vector3[] LocalVertices = SourceMesh.vertices;
            if (LocalVertices == null || LocalVertices.Length == 0)
            {
                continue;
            }

            Transform HullTransform = MeshFilterComponent.transform;

            HashSet<Vector3> UniqueRootLocalVertices = new HashSet<Vector3>();

            for (int Index = 0; Index < LocalVertices.Length; Index++)
            {
                Vector3 LocalVertex = LocalVertices[Index];
                Vector3 WorldVertex = HullTransform.TransformPoint(LocalVertex);
                Vector3 RootLocalVertex = RootTransform.InverseTransformPoint(WorldVertex);
                UniqueRootLocalVertices.Add(RootLocalVertex);
            }

            Builder.AppendLine();
            Builder.AppendLine("Hull " + HullIndex + " (" + MeshFilterComponent.gameObject.name + "):");

            foreach (Vector3 Vertex in UniqueRootLocalVertices)
            {
                Builder.AppendLine("new DbVector3(" + Vertex.x + "f, " + Vertex.y + "f, " + Vertex.z + "f),");
            }

            Debug.Log("Hull " + HullIndex + " unique vertex count: " + UniqueRootLocalVertices.Count);
            HullIndex++;
        }

        if (HullIndex == 0)
        {
            Debug.LogError("ColliderVertexExtractorTool ExportConvexMeshVertices found no valid child hull meshes to export.");
            return;
        }

        string DefaultFileName = SelectedObject.name + "_ConvexHullVertices_AllHulls.txt";
        string FilePath = EditorUtility.SaveFilePanel("Save Convex Hull Vertices For All Child Hulls", "", DefaultFileName, "txt");

        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        File.WriteAllText(FilePath, Builder.ToString());
        Debug.Log("ColliderVertexExtractorTool ExportConvexMeshVertices wrote vertices for " + HullIndex + " hulls to file: " + FilePath);
    }
}

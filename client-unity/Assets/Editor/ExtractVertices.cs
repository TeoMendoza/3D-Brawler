using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

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

        Builder.AppendLine("using System.Collections.Generic;");
        Builder.AppendLine("using System.Numerics;");
        Builder.AppendLine("using SpacetimeDB;");
        Builder.AppendLine();
        Builder.AppendLine("public static partial class Module");
        Builder.AppendLine("{");

        int HullIndex = 0;
        List<int> ExportedHullIndices = new List<int>();

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

            if (UniqueRootLocalVertices.Count == 0)
            {
                continue;
            }

            Debug.Log("Hull " + HullIndex + " unique vertex count: " + UniqueRootLocalVertices.Count);
            ExportedHullIndices.Add(HullIndex);

            Builder.AppendLine();
            Builder.AppendLine("    public static readonly List<DbVector3> ConvexHull" + HullIndex + "Vertices = new List<DbVector3>");
            Builder.AppendLine("    {");

            foreach (Vector3 Vertex in UniqueRootLocalVertices)
            {
                string X = Vertex.x.ToString("0.######", CultureInfo.InvariantCulture);
                string Y = Vertex.y.ToString("0.######", CultureInfo.InvariantCulture);
                string Z = Vertex.z.ToString("0.######", CultureInfo.InvariantCulture);

                Builder.AppendLine("        new DbVector3(" + X + "f, " + Y + "f, " + Z + "f),");
            }

            Builder.AppendLine("    };");
            Builder.AppendLine();
            Builder.AppendLine("    public static readonly ConvexHullCollider ConvexHull" + HullIndex + " = new ConvexHullCollider");
            Builder.AppendLine("    {");
            Builder.AppendLine("        VerticesLocal = ConvexHull" + HullIndex + "Vertices");
            Builder.AppendLine("    };");

            HullIndex++;
        }

        if (ExportedHullIndices.Count == 0)
        {
            Debug.LogError("ColliderVertexExtractorTool ExportConvexMeshVertices found no valid child hull meshes to export.");
            return;
        }

        Builder.AppendLine();
        string PrefixName = SelectedObject.name;
        Builder.AppendLine("    public static readonly List<ConvexHullCollider> " + PrefixName + "ConvexHulls = new List<ConvexHullCollider>");
        Builder.AppendLine("    {");
        for (int Index = 0; Index < ExportedHullIndices.Count; Index++)
        {
            int HullId = ExportedHullIndices[Index];
            Builder.AppendLine("        ConvexHull" + HullId + ",");
        }
        Builder.AppendLine("    };");
        Builder.AppendLine();
        Builder.AppendLine("    public static readonly ComplexCollider " + PrefixName + "Collider = new ComplexCollider");
        Builder.AppendLine("    {");
        Builder.AppendLine("        ConvexHulls = " + PrefixName + "ConvexHulls");
        Builder.AppendLine("    };");
        Builder.AppendLine("}");

        string DefaultFileName = SelectedObject.name + "_ConvexHulls.cs";
        string FilePath = EditorUtility.SaveFilePanel("Save Convex Hull Collider C# File", "", DefaultFileName, "cs");

        if (string.IsNullOrEmpty(FilePath))
        {
            return;
        }

        File.WriteAllText(FilePath, Builder.ToString());
        Debug.Log("ColliderVertexExtractorTool ExportConvexMeshVertices wrote hulls C# file for " + ExportedHullIndices.Count + " hulls to: " + FilePath);
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

public static class ColliderVertexExtractorTool2
{
    [MenuItem("Tools/Collider/Export All Collider Vertices")]
    public static void ExportColliderVertices()
    {
        GameObject SelectedObject = Selection.activeGameObject;
        if (SelectedObject == null)
        {
            Debug.LogError("ColliderVertexExtractorTool ExportColliderVertices requires a selected GameObject.");
            return;
        }

        Transform RootTransform = SelectedObject.transform;

        Collider[] Colliders = SelectedObject.GetComponentsInChildren<Collider>(true);
        MeshFilter[] MeshFilters = SelectedObject.GetComponentsInChildren<MeshFilter>(true);

        if ((Colliders == null || Colliders.Length == 0) && (MeshFilters == null || MeshFilters.Length == 0))
        {
            Debug.LogError("ColliderVertexExtractorTool ExportColliderVertices requires at least one Collider or MeshFilter on the selected GameObject or its children.");
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

        for (int ColliderIndex = 0; ColliderIndex < Colliders.Length; ColliderIndex++)
        {
            Collider CurrentCollider = Colliders[ColliderIndex];
            if (CurrentCollider == null)
            {
                continue;
            }

            Transform ColliderTransform = CurrentCollider.transform;
            HashSet<Vector3> UniqueRootLocalVertices = new HashSet<Vector3>();

            if (CurrentCollider is MeshCollider)
            {
                MeshCollider MeshColliderComponent = CurrentCollider as MeshCollider;
                Mesh SourceMesh = MeshColliderComponent.sharedMesh;
                if (SourceMesh == null)
                {
                    Debug.LogWarning("ColliderVertexExtractorTool ExportColliderVertices found no mesh on MeshCollider: " + MeshColliderComponent.gameObject.name);
                    continue;
                }

                Vector3[] LocalVertices = SourceMesh.vertices;
                if (LocalVertices == null || LocalVertices.Length == 0)
                {
                    continue;
                }

                for (int Index = 0; Index < LocalVertices.Length; Index++)
                {
                    Vector3 LocalVertex = LocalVertices[Index];
                    Vector3 WorldVertex = ColliderTransform.TransformPoint(LocalVertex);
                    Vector3 RootLocalVertex = RootTransform.InverseTransformPoint(WorldVertex);
                    UniqueRootLocalVertices.Add(RootLocalVertex);
                }
            }
            else if (CurrentCollider is BoxCollider)
            {
                BoxCollider BoxColliderComponent = CurrentCollider as BoxCollider;
                Vector3 CenterLocal = BoxColliderComponent.center;
                Vector3 SizeLocal = BoxColliderComponent.size;
                Vector3 HalfSize = SizeLocal * 0.5f;

                for (int XSign = -1; XSign <= 1; XSign += 2)
                {
                    for (int YSign = -1; YSign <= 1; YSign += 2)
                    {
                        for (int ZSign = -1; ZSign <= 1; ZSign += 2)
                        {
                            Vector3 OffsetLocal = new Vector3(
                                HalfSize.x * XSign,
                                HalfSize.y * YSign,
                                HalfSize.z * ZSign
                            );

                            Vector3 LocalVertex = CenterLocal + OffsetLocal;
                            Vector3 WorldVertex = ColliderTransform.TransformPoint(LocalVertex);
                            Vector3 RootLocalVertex = RootTransform.InverseTransformPoint(WorldVertex);
                            UniqueRootLocalVertices.Add(RootLocalVertex);
                        }
                    }
                }
            }
            else if (CurrentCollider is SphereCollider)
            {
                SphereCollider SphereColliderComponent = CurrentCollider as SphereCollider;
                Vector3 CenterLocal = SphereColliderComponent.center;
                float Radius = SphereColliderComponent.radius;

                Vector3[] Directions = new Vector3[]
                {
                    Vector3.right,
                    Vector3.left,
                    Vector3.up,
                    Vector3.down,
                    Vector3.forward,
                    Vector3.back
                };

                for (int Index = 0; Index < Directions.Length; Index++)
                {
                    Vector3 LocalVertex = CenterLocal + Directions[Index] * Radius;
                    Vector3 WorldVertex = ColliderTransform.TransformPoint(LocalVertex);
                    Vector3 RootLocalVertex = RootTransform.InverseTransformPoint(WorldVertex);
                    UniqueRootLocalVertices.Add(RootLocalVertex);
                }
            }
            else if (CurrentCollider is CapsuleCollider)
            {
                CapsuleCollider CapsuleColliderComponent = CurrentCollider as CapsuleCollider;
                Vector3 CenterLocal = CapsuleColliderComponent.center;
                float Radius = CapsuleColliderComponent.radius;
                float Height = CapsuleColliderComponent.height;

                float HalfHeight = Mathf.Max(Height * 0.5f - Radius, 0f);

                Vector3 AxisLocal;
                if (CapsuleColliderComponent.direction == 0)
                {
                    AxisLocal = Vector3.right;
                }
                else if (CapsuleColliderComponent.direction == 1)
                {
                    AxisLocal = Vector3.up;
                }
                else
                {
                    AxisLocal = Vector3.forward;
                }

                Vector3 TopCenterLocal = CenterLocal + AxisLocal * HalfHeight;
                Vector3 BottomCenterLocal = CenterLocal - AxisLocal * HalfHeight;

                Vector3[] Directions = new Vector3[]
                {
                    Vector3.right,
                    Vector3.left,
                    Vector3.up,
                    Vector3.down,
                    Vector3.forward,
                    Vector3.back
                };

                for (int Index = 0; Index < Directions.Length; Index++)
                {
                    Vector3 Direction = Directions[Index];

                    Vector3 LocalVertexTop = TopCenterLocal + Direction * Radius;
                    Vector3 WorldVertexTop = ColliderTransform.TransformPoint(LocalVertexTop);
                    Vector3 RootLocalVertexTop = RootTransform.InverseTransformPoint(WorldVertexTop);
                    UniqueRootLocalVertices.Add(RootLocalVertexTop);

                    Vector3 LocalVertexBottom = BottomCenterLocal + Direction * Radius;
                    Vector3 WorldVertexBottom = ColliderTransform.TransformPoint(LocalVertexBottom);
                    Vector3 RootLocalVertexBottom = RootTransform.InverseTransformPoint(WorldVertexBottom);
                    UniqueRootLocalVertices.Add(RootLocalVertexBottom);
                }
            }
            else
            {
                Debug.LogWarning("ColliderVertexExtractorTool ExportColliderVertices encountered unsupported collider type on object: " + CurrentCollider.gameObject.name);
                continue;
            }

            if (UniqueRootLocalVertices.Count == 0)
            {
                continue;
            }

            Debug.Log("Hull " + HullIndex + " unique vertex count (collider): " + UniqueRootLocalVertices.Count);
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

        for (int FilterIndex = 0; FilterIndex < MeshFilters.Length; FilterIndex++)
        {
            MeshFilter MeshFilterComponent = MeshFilters[FilterIndex];
            if (MeshFilterComponent == null)
            {
                continue;
            }

            if (MeshFilterComponent.GetComponent<Collider>() != null)
            {
                continue;
            }

            Mesh SourceMesh = MeshFilterComponent.sharedMesh;
            if (SourceMesh == null)
            {
                Debug.LogWarning("ColliderVertexExtractorTool ExportColliderVertices found no mesh on MeshFilter: " + MeshFilterComponent.gameObject.name);
                continue;
            }

            Vector3[] LocalVertices = SourceMesh.vertices;
            if (LocalVertices == null || LocalVertices.Length == 0)
            {
                continue;
            }

            Transform MeshTransform = MeshFilterComponent.transform;
            HashSet<Vector3> UniqueRootLocalVertices = new HashSet<Vector3>();

            for (int Index = 0; Index < LocalVertices.Length; Index++)
            {
                Vector3 LocalVertex = LocalVertices[Index];
                Vector3 WorldVertex = MeshTransform.TransformPoint(LocalVertex);
                Vector3 RootLocalVertex = RootTransform.InverseTransformPoint(WorldVertex);
                UniqueRootLocalVertices.Add(RootLocalVertex);
            }

            if (UniqueRootLocalVertices.Count == 0)
            {
                continue;
            }

            Debug.Log("Hull " + HullIndex + " unique vertex count (mesh): " + UniqueRootLocalVertices.Count);
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
            Debug.LogError("ColliderVertexExtractorTool ExportColliderVertices found no valid collider or mesh vertices to export.");
            Builder.AppendLine("}");
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
        Debug.Log("ColliderVertexExtractorTool ExportColliderVertices wrote hulls C# file for " + ExportedHullIndices.Count + " hulls to: " + FilePath);
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public sealed class QuickHullToolWindow : EditorWindow
{
    [MenuItem("Tools/QuickHull Tool")]
    public static void Open()
    {
        QuickHullToolWindow Window = GetWindow<QuickHullToolWindow>();
        Window.titleContent = new GUIContent("QuickHull Tool");
        Window.Show();
    }

    string VerticesText = "";
    float DuplicateEpsilon = 1e-4f;
    float CoplanarEpsilon = 1e-5f;
    bool BuildPreviewMesh = true;
    Vector2 ScrollPosition;

    List<Vector3> LastVertices = new List<Vector3>();
    List<int> LastTriangles = new List<int>();
    Mesh LastMesh;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Input Vertices", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Paste vertices as lines like: new DbVector3(0.1f, 0.2f, -0.3f), OR as: 0.1,0.2,-0.3", MessageType.Info);

        ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(220));
        VerticesText = EditorGUILayout.TextArea(VerticesText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        DuplicateEpsilon = EditorGUILayout.FloatField("Duplicate Epsilon", DuplicateEpsilon);
        CoplanarEpsilon = EditorGUILayout.FloatField("Coplanar Epsilon", CoplanarEpsilon);
        BuildPreviewMesh = EditorGUILayout.Toggle("Build Preview Mesh", BuildPreviewMesh);

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Compute Hull Triangles"))
        {
            try
            {
                List<Vector3> ParsedVertices = ParseVertices(VerticesText);
                List<Vector3> UniqueVertices = DeduplicateVertices(ParsedVertices, DuplicateEpsilon);
                LastVertices = UniqueVertices;

                List<int> Triangles = QuickHull3D.BuildHullTriangles(UniqueVertices, CoplanarEpsilon);
                LastTriangles = Triangles;

                if (BuildPreviewMesh)
                {
                    LastMesh = BuildMesh(UniqueVertices, Triangles);
                    CreatePreviewObject(LastMesh);
                }
            }
            catch (Exception Exception)
            {
                Debug.LogError(Exception);
            }
        }

        EditorGUILayout.Space(8);

        if (LastVertices.Count > 0)
        {
            EditorGUILayout.LabelField($"Vertices: {LastVertices.Count}", EditorStyles.boldLabel);
        }

        if (LastTriangles.Count > 0)
        {
            EditorGUILayout.LabelField($"Triangles: {LastTriangles.Count / 3}", EditorStyles.boldLabel);

            if (GUILayout.Button("Copy Triangle Indices As List<int>"))
            {
                string Output = FormatTrianglesAsListInt(LastTriangles);
                EditorGUIUtility.systemCopyBuffer = Output;
            }

            if (GUILayout.Button("Copy Triangle Indices As Lines"))
            {
                string Output = FormatTrianglesAsLines(LastTriangles);
                EditorGUIUtility.systemCopyBuffer = Output;
            }
            if (GUILayout.Button("Copy Triangle Indices As One-Line List<int>"))
            {
                string Output = FormatTrianglesAsListIntOneLine(LastTriangles);
                EditorGUIUtility.systemCopyBuffer = Output;
            }

        }

        if (LastMesh != null)
        {
            if (GUILayout.Button("Select Preview Object"))
            {
                SelectPreviewObject();
            }
        }
    }

    static string FormatTrianglesAsListIntOneLine(List<int> Triangles)
    {
        StringBuilder Builder = new StringBuilder();
        Builder.Append("new List<int> { ");

        for (int Index = 0; Index < Triangles.Count; Index++)
        {
            if (Index != 0) Builder.Append(", ");
            Builder.Append(Triangles[Index]);
        }

        Builder.Append(" }");
        return Builder.ToString();
    }
    
    static List<Vector3> ParseVertices(string Text)
    {
        List<Vector3> Result = new List<Vector3>();
        if (string.IsNullOrWhiteSpace(Text)) throw new Exception("No vertices provided.");

        string[] Lines = Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        Regex DbVectorPattern = new Regex(@"DbVector3\s*\(\s*([-\d\.eE]+)\s*f?\s*,\s*([-\d\.eE]+)\s*f?\s*,\s*([-\d\.eE]+)\s*f?\s*\)", RegexOptions.Compiled);
        Regex PlainPattern = new Regex(@"^\s*([-\d\.eE]+)\s*,\s*([-\d\.eE]+)\s*,\s*([-\d\.eE]+)\s*$", RegexOptions.Compiled);

        foreach (string Line in Lines)
        {
            Match MatchDb = DbVectorPattern.Match(Line);
            if (MatchDb.Success)
            {
                float X = ParseFloat(MatchDb.Groups[1].Value);
                float Y = ParseFloat(MatchDb.Groups[2].Value);
                float Z = ParseFloat(MatchDb.Groups[3].Value);
                Result.Add(new Vector3(X, Y, Z));
                continue;
            }

            Match MatchPlain = PlainPattern.Match(Line);
            if (MatchPlain.Success)
            {
                float X = ParseFloat(MatchPlain.Groups[1].Value);
                float Y = ParseFloat(MatchPlain.Groups[2].Value);
                float Z = ParseFloat(MatchPlain.Groups[3].Value);
                Result.Add(new Vector3(X, Y, Z));
                continue;
            }
        }

        if (Result.Count < 4) throw new Exception("Need at least 4 vertices to build a 3D hull.");
        return Result;
    }

    static float ParseFloat(string Text)
    {
        return float.Parse(Text, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    static List<Vector3> DeduplicateVertices(List<Vector3> Vertices, float Epsilon)
    {
        List<Vector3> Result = new List<Vector3>();
        float EpsilonSq = Epsilon * Epsilon;

        for (int IndexA = 0; IndexA < Vertices.Count; IndexA++)
        {
            Vector3 Candidate = Vertices[IndexA];
            bool IsDuplicate = false;

            for (int IndexB = 0; IndexB < Result.Count; IndexB++)
            {
                Vector3 Existing = Result[IndexB];
                if ((Candidate - Existing).sqrMagnitude <= EpsilonSq)
                {
                    IsDuplicate = true;
                    break;
                }
            }

            if (IsDuplicate == false) Result.Add(Candidate);
        }

        if (Result.Count < 4) throw new Exception("After deduplication, not enough vertices remain.");
        return Result;
    }

    static Mesh BuildMesh(List<Vector3> Vertices, List<int> Triangles)
    {
        Mesh Mesh = new Mesh();
        Mesh.indexFormat = Vertices.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        Mesh.SetVertices(Vertices);
        Mesh.SetTriangles(Triangles, 0);
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();
        return Mesh;
    }

    static string FormatTrianglesAsListInt(List<int> Triangles)
    {
        StringBuilder Builder = new StringBuilder();
        Builder.Append("new List<int>\n{\n");
        for (int Index = 0; Index < Triangles.Count; Index++)
        {
            Builder.Append("    ");
            Builder.Append(Triangles[Index]);
            Builder.Append(",");
            Builder.Append("\n");
        }
        Builder.Append("}");
        return Builder.ToString();
    }

    static string FormatTrianglesAsLines(List<int> Triangles)
    {
        StringBuilder Builder = new StringBuilder();
        for (int Index = 0; Index < Triangles.Count; Index += 3)
        {
            Builder.Append(Triangles[Index]);
            Builder.Append(", ");
            Builder.Append(Triangles[Index + 1]);
            Builder.Append(", ");
            Builder.Append(Triangles[Index + 2]);
            Builder.Append("\n");
        }
        return Builder.ToString();
    }

    static readonly string PreviewObjectName = "QuickHull Preview Mesh";

    static void CreatePreviewObject(Mesh Mesh)
    {
        GameObject Existing = GameObject.Find(PreviewObjectName);
        if (Existing != null) DestroyImmediate(Existing);

        GameObject Preview = new GameObject(PreviewObjectName);
        MeshFilter Filter = Preview.AddComponent<MeshFilter>();
        MeshRenderer Renderer = Preview.AddComponent<MeshRenderer>();
        Filter.sharedMesh = Mesh;
        Renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        Selection.activeGameObject = Preview;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    static void SelectPreviewObject()
    {
        GameObject Existing = GameObject.Find(PreviewObjectName);
        if (Existing != null) Selection.activeGameObject = Existing;
    }
}

public static class QuickHull3D
{
    sealed class Face
    {
        public int A;
        public int B;
        public int C;
        public Vector3 Normal;
        public float Offset;
        public bool IsDisabled;
        public HashSet<int> OutsidePoints = new HashSet<int>();

        public Face(int a, int b, int c, List<Vector3> Vertices, Vector3 InteriorPoint)
        {
            A = a;
            B = b;
            C = c;
            RecomputePlane(Vertices);
            if (DistanceToPlane(InteriorPoint) > 0f) Flip(Vertices);
        }

        public void RecomputePlane(List<Vector3> Vertices)
        {
            Vector3 Va = Vertices[A];
            Vector3 Vb = Vertices[B];
            Vector3 Vc = Vertices[C];
            Vector3 Ab = Vb - Va;
            Vector3 Ac = Vc - Va;
            Vector3 N = Vector3.Cross(Ab, Ac);
            float Len = N.magnitude;
            if (Len <= 1e-12f)
            {
                Normal = Vector3.zero;
                Offset = 0f;
                return;
            }
            Normal = N / Len;
            Offset = -Vector3.Dot(Normal, Va);
        }

        public void Flip(List<Vector3> Vertices)
        {
            int Temp = B;
            B = C;
            C = Temp;
            RecomputePlane(Vertices);
        }

        public float DistanceToPlane(Vector3 Point)
        {
            return Vector3.Dot(Normal, Point) + Offset;
        }
    }

    struct EdgeKey : IEquatable<EdgeKey>
    {
        public int From;
        public int To;

        public EdgeKey(int from, int to)
        {
            From = from;
            To = to;
        }

        public bool Equals(EdgeKey Other)
        {
            return From == Other.From && To == Other.To;
        }

        public override bool Equals(object Obj)
        {
            if (Obj is EdgeKey Other) return Equals(Other);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From * 397) ^ To;
            }
        }
    }

    public static List<int> BuildHullTriangles(List<Vector3> Vertices, float Epsilon)
    {
        if (Vertices.Count < 4) throw new Exception("Need at least 4 points.");
        List<Face> Faces = new List<Face>();

        int IndexMinX = 0;
        int IndexMaxX = 0;
        for (int Index = 1; Index < Vertices.Count; Index++)
        {
            if (Vertices[Index].x < Vertices[IndexMinX].x) IndexMinX = Index;
            if (Vertices[Index].x > Vertices[IndexMaxX].x) IndexMaxX = Index;
        }

        if (IndexMinX == IndexMaxX) throw new Exception("Degenerate point set.");

        int IndexMaxDist = FindFarthestPointFromLine(Vertices, IndexMinX, IndexMaxX, Epsilon);
        if (IndexMaxDist < 0) throw new Exception("All points appear collinear.");

        int IndexMaxDistFromPlane = FindFarthestPointFromPlane(Vertices, IndexMinX, IndexMaxX, IndexMaxDist, Epsilon);
        if (IndexMaxDistFromPlane < 0) throw new Exception("All points appear coplanar.");

        Vector3 InteriorPoint = (Vertices[IndexMinX] + Vertices[IndexMaxX] + Vertices[IndexMaxDist] + Vertices[IndexMaxDistFromPlane]) * 0.25f;

        CreateInitialTetrahedron(Faces, Vertices, InteriorPoint, IndexMinX, IndexMaxX, IndexMaxDist, IndexMaxDistFromPlane);

        HashSet<int> TetraIndices = new HashSet<int>();
        TetraIndices.Add(IndexMinX);
        TetraIndices.Add(IndexMaxX);
        TetraIndices.Add(IndexMaxDist);
        TetraIndices.Add(IndexMaxDistFromPlane);

        AssignOutsidePoints(Faces, Vertices, TetraIndices, Epsilon);

        while (true)
        {
            Face FaceToExpand = FindFaceWithOutsidePoints(Faces);
            if (FaceToExpand == null) break;

            int ApexIndex = FindFarthestOutsidePoint(FaceToExpand, Vertices);
            if (ApexIndex < 0)
            {
                FaceToExpand.OutsidePoints.Clear();
                continue;
            }

            List<Face> VisibleFaces = CollectVisibleFaces(Faces, Vertices, ApexIndex, Epsilon);
            if (VisibleFaces.Count == 0)
            {
                FaceToExpand.OutsidePoints.Remove(ApexIndex);
                continue;
            }

            List<(int From, int To)> HorizonEdges = BuildHorizon(VisibleFaces);

            HashSet<int> ReassignPoints = new HashSet<int>();
            foreach (Face Visible in VisibleFaces)
            {
                foreach (int P in Visible.OutsidePoints) ReassignPoints.Add(P);
                Visible.OutsidePoints.Clear();
                Visible.IsDisabled = true;
            }

            List<Face> NewFaces = new List<Face>();
            for (int Index = 0; Index < HorizonEdges.Count; Index++)
            {
                (int From, int To) Edge = HorizonEdges[Index];
                Face NewFace = new Face(Edge.From, Edge.To, ApexIndex, Vertices, InteriorPoint);
                if (NewFace.Normal == Vector3.zero) continue;
                NewFaces.Add(NewFace);
            }

            Faces.AddRange(NewFaces);

            ReassignPoints.Remove(ApexIndex);
            ReassignOutsidePoints(NewFaces, Vertices, ReassignPoints, Epsilon);

            RemoveDisabledFaces(Faces);
        }

        List<int> Triangles = new List<int>();
        for (int Index = 0; Index < Faces.Count; Index++)
        {
            Face Face = Faces[Index];
            if (Face.IsDisabled) continue;
            Triangles.Add(Face.A);
            Triangles.Add(Face.B);
            Triangles.Add(Face.C);
        }

        return Triangles;
    }

    static int FindFarthestPointFromLine(List<Vector3> Vertices, int A, int B, float Epsilon)
    {
        Vector3 Va = Vertices[A];
        Vector3 Vb = Vertices[B];
        Vector3 Ab = Vb - Va;
        float AbLenSq = Ab.sqrMagnitude;
        if (AbLenSq <= 1e-12f) return -1;

        int BestIndex = -1;
        float BestDistSq = 0f;

        for (int Index = 0; Index < Vertices.Count; Index++)
        {
            if (Index == A || Index == B) continue;
            Vector3 Ap = Vertices[Index] - Va;
            Vector3 Cross = Vector3.Cross(Ab, Ap);
            float DistSq = Cross.sqrMagnitude / AbLenSq;
            if (DistSq > BestDistSq + Epsilon * Epsilon)
            {
                BestDistSq = DistSq;
                BestIndex = Index;
            }
        }

        return BestIndex;
    }

    static int FindFarthestPointFromPlane(List<Vector3> Vertices, int A, int B, int C, float Epsilon)
    {
        Vector3 Va = Vertices[A];
        Vector3 Vb = Vertices[B];
        Vector3 Vc = Vertices[C];

        Vector3 Normal = Vector3.Cross(Vb - Va, Vc - Va);
        float Len = Normal.magnitude;
        if (Len <= 1e-12f) return -1;
        Normal /= Len;

        float Offset = -Vector3.Dot(Normal, Va);

        int BestIndex = -1;
        float BestAbsDist = 0f;

        for (int Index = 0; Index < Vertices.Count; Index++)
        {
            if (Index == A || Index == B || Index == C) continue;
            float Dist = Vector3.Dot(Normal, Vertices[Index]) + Offset;
            float AbsDist = Mathf.Abs(Dist);
            if (AbsDist > BestAbsDist + Epsilon)
            {
                BestAbsDist = AbsDist;
                BestIndex = Index;
            }
        }

        return BestIndex;
    }

    static void CreateInitialTetrahedron(List<Face> Faces, List<Vector3> Vertices, Vector3 InteriorPoint, int A, int B, int C, int D)
    {
        Faces.Add(new Face(A, B, C, Vertices, InteriorPoint));
        Faces.Add(new Face(A, C, D, Vertices, InteriorPoint));
        Faces.Add(new Face(A, D, B, Vertices, InteriorPoint));
        Faces.Add(new Face(B, D, C, Vertices, InteriorPoint));
    }

    static void AssignOutsidePoints(List<Face> Faces, List<Vector3> Vertices, HashSet<int> SeedIndices, float Epsilon)
    {
        for (int PointIndex = 0; PointIndex < Vertices.Count; PointIndex++)
        {
            if (SeedIndices.Contains(PointIndex)) continue;

            int BestFaceIndex = -1;
            float BestDistance = Epsilon;

            for (int FaceIndex = 0; FaceIndex < Faces.Count; FaceIndex++)
            {
                Face Face = Faces[FaceIndex];
                float Distance = Face.DistanceToPlane(Vertices[PointIndex]);
                if (Distance > BestDistance)
                {
                    BestDistance = Distance;
                    BestFaceIndex = FaceIndex;
                }
            }

            if (BestFaceIndex >= 0) Faces[BestFaceIndex].OutsidePoints.Add(PointIndex);
        }
    }

    static Face FindFaceWithOutsidePoints(List<Face> Faces)
    {
        for (int Index = 0; Index < Faces.Count; Index++)
        {
            Face Face = Faces[Index];
            if (Face.IsDisabled) continue;
            if (Face.OutsidePoints.Count > 0) return Face;
        }
        return null;
    }

    static int FindFarthestOutsidePoint(Face Face, List<Vector3> Vertices)
    {
        int BestIndex = -1;
        float BestDistance = 0f;
        foreach (int PointIndex in Face.OutsidePoints)
        {
            float Distance = Face.DistanceToPlane(Vertices[PointIndex]);
            if (Distance > BestDistance)
            {
                BestDistance = Distance;
                BestIndex = PointIndex;
            }
        }
        return BestIndex;
    }

    static List<Face> CollectVisibleFaces(List<Face> Faces, List<Vector3> Vertices, int ApexIndex, float Epsilon)
    {
        List<Face> Visible = new List<Face>();
        Vector3 Apex = Vertices[ApexIndex];

        for (int Index = 0; Index < Faces.Count; Index++)
        {
            Face Face = Faces[Index];
            if (Face.IsDisabled) continue;
            float Distance = Face.DistanceToPlane(Apex);
            if (Distance > Epsilon) Visible.Add(Face);
        }

        return Visible;
    }

    static List<(int From, int To)> BuildHorizon(List<Face> VisibleFaces)
    {
        Dictionary<EdgeKey, int> EdgeCounts = new Dictionary<EdgeKey, int>();

        void AddEdge(int From, int To)
        {
            EdgeKey Forward = new EdgeKey(From, To);
            EdgeKey Reverse = new EdgeKey(To, From);

            if (EdgeCounts.ContainsKey(Reverse))
            {
                EdgeCounts[Reverse] = EdgeCounts[Reverse] + 1;
            }
            else if (EdgeCounts.ContainsKey(Forward))
            {
                EdgeCounts[Forward] = EdgeCounts[Forward] + 1;
            }
            else
            {
                EdgeCounts[Forward] = 1;
            }
        }

        for (int Index = 0; Index < VisibleFaces.Count; Index++)
        {
            Face Face = VisibleFaces[Index];
            AddEdge(Face.A, Face.B);
            AddEdge(Face.B, Face.C);
            AddEdge(Face.C, Face.A);
        }

        List<(int From, int To)> Horizon = new List<(int From, int To)>();
        foreach (var Pair in EdgeCounts)
        {
            EdgeKey Key = Pair.Key;
            int Count = Pair.Value;

            if (Count == 1)
            {
                bool FoundReverseInVisible = false;
                for (int Index = 0; Index < VisibleFaces.Count; Index++)
                {
                    Face Face = VisibleFaces[Index];
                    if ((Face.A == Key.To && Face.B == Key.From) || (Face.B == Key.To && Face.C == Key.From) || (Face.C == Key.To && Face.A == Key.From))
                    {
                        FoundReverseInVisible = true;
                        break;
                    }
                }

                if (FoundReverseInVisible == false)
                {
                    Horizon.Add((Key.From, Key.To));
                }
                else
                {
                    Horizon.Add((Key.To, Key.From));
                }
            }
        }

        return OrderHorizonEdges(Horizon);
    }

    static List<(int From, int To)> OrderHorizonEdges(List<(int From, int To)> HorizonEdges)
    {
        if (HorizonEdges.Count == 0) return HorizonEdges;

        Dictionary<int, int> NextByFrom = new Dictionary<int, int>();
        for (int Index = 0; Index < HorizonEdges.Count; Index++)
        {
            (int From, int To) Edge = HorizonEdges[Index];
            if (NextByFrom.ContainsKey(Edge.From) == false) NextByFrom[Edge.From] = Edge.To;
        }

        List<(int From, int To)> Ordered = new List<(int From, int To)>();
        (int From, int To) Start = HorizonEdges[0];

        int CurrentFrom = Start.From;
        int CurrentTo = Start.To;
        Ordered.Add((CurrentFrom, CurrentTo));

        int Safety = 0;
        while (Safety < HorizonEdges.Count + 5)
        {
            Safety += 1;
            if (NextByFrom.TryGetValue(CurrentTo, out int NextTo) == false) break;
            if (CurrentTo == Start.From) break;
            Ordered.Add((CurrentTo, NextTo));
            CurrentTo = NextTo;
        }

        return Ordered;
    }

    static void ReassignOutsidePoints(List<Face> NewFaces, List<Vector3> Vertices, HashSet<int> Points, float Epsilon)
    {
        foreach (int PointIndex in Points)
        {
            int BestFaceIndex = -1;
            float BestDistance = Epsilon;

            for (int FaceIndex = 0; FaceIndex < NewFaces.Count; FaceIndex++)
            {
                Face Face = NewFaces[FaceIndex];
                float Distance = Face.DistanceToPlane(Vertices[PointIndex]);
                if (Distance > BestDistance)
                {
                    BestDistance = Distance;
                    BestFaceIndex = FaceIndex;
                }
            }

            if (BestFaceIndex >= 0) NewFaces[BestFaceIndex].OutsidePoints.Add(PointIndex);
        }
    }

    static void RemoveDisabledFaces(List<Face> Faces)
    {
        for (int Index = Faces.Count - 1; Index >= 0; Index--)
        {
            if (Faces[Index].IsDisabled) Faces.RemoveAt(Index);
        }
    }

    
}


using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;

public static class MeshSimplifierTool
{
    [MenuItem("Tools/Simplify Selected Mesh Filter")]
    public static void SimplifySelectedMeshFilter()
    {
        GameObject SelectedObject = Selection.activeGameObject;
        if (SelectedObject == null) return;

        MeshFilter SelectedMeshFilter = SelectedObject.GetComponent<MeshFilter>();
        if (SelectedMeshFilter == null) return;

        Mesh OriginalMesh = SelectedMeshFilter.sharedMesh;
        if (OriginalMesh == null) return;

        MeshSimplifier MeshSimplifierInstance = new MeshSimplifier();
        MeshSimplifierInstance.Initialize(OriginalMesh);

        float TargetQuality = 0.0f; // 0.0 to 1.0, lower means more aggressive simplification
        MeshSimplifierInstance.SimplifyMesh(TargetQuality);

        Mesh SimplifiedMesh = MeshSimplifierInstance.ToMesh();
        SimplifiedMesh.name = OriginalMesh.name + "_Simplified";

        string AssetPath = "Assets/" + SimplifiedMesh.name + ".asset";
        AssetDatabase.CreateAsset(SimplifiedMesh, AssetPath);
        AssetDatabase.SaveAssets();

        SelectedMeshFilter.sharedMesh = SimplifiedMesh;
        EditorUtility.SetDirty(SelectedMeshFilter);
    }
}

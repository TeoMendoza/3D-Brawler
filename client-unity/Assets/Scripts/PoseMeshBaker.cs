using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PoseMeshBaker : MonoBehaviour
{
    public SkinnedMeshRenderer SourceRenderer;
    public string MeshAssetName = "BakedPoseMesh";

#if UNITY_EDITOR
    [ContextMenu("BakePoseToAsset")]
    void BakePoseToAsset()
    {
        if (SourceRenderer == null)
        {
            SourceRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (SourceRenderer == null)
        {
            Debug.LogError("PoseMeshBaker could not find a SkinnedMeshRenderer.");
            return;
        }

        Mesh bakedMesh = new Mesh();
        SourceRenderer.BakeMesh(bakedMesh);

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Baked Mesh",
            MeshAssetName,
            "asset",
            "Choose a location for the baked mesh asset."
        );

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        AssetDatabase.CreateAsset(bakedMesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Baked mesh saved to " + path);
    }
#endif
}

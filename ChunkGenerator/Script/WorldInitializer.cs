using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldInitializer : MonoBehaviour
{
    [SerializeField] private ChunkRenderer chunkRenderer;
    [SerializeField] private ChunkConfig chunkConfig;

    private void Start()
    {
        chunkRenderer.Initialize(chunkConfig);
        chunkRenderer.GenerateVisuals();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WorldInitializer))]
public class WorldInitializerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        var chunkRendererProp = serializedObject.FindProperty("chunkRenderer");
        var chunkConfigProp   = serializedObject.FindProperty("chunkConfig");

        var renderer = chunkRendererProp.objectReferenceValue as ChunkRenderer;
        var config = chunkConfigProp.objectReferenceValue as ChunkConfig;

        if (renderer != null && config != null && GUILayout.Button("Generate Chunk Visuals"))
        {
            renderer.Initialize(config);
            renderer.GenerateVisuals();
            EditorUtility.SetDirty(renderer);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

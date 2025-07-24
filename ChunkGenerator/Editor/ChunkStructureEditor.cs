using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StructureConfig))]
public class ChunkStructureEditor : Editor
{
    private int currentLayer = 0;
    private StructureConfig config;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        config = (StructureConfig)target;

        SerializedProperty sizeProp = serializedObject.FindProperty("sizeRaw");
        SerializedProperty anchorProp = serializedObject.FindProperty("anchorRaw");
        SerializedProperty blocksProp = serializedObject.FindProperty("blockTemplates");
        SerializedProperty matrixProp = serializedObject.FindProperty("serializedMatrix");
        SerializedProperty flattenProp = serializedObject.FindProperty("flattenGroundUnderStructure");

        EditorGUILayout.LabelField("Structure Settings", EditorStyles.boldLabel);
        DrawVector3Int(sizeProp, "Size (X,Y,Z)");
        DrawVector3Int(anchorProp, "Anchor (X,Y,Z)");
        EditorGUILayout.PropertyField(blocksProp, true);
        EditorGUILayout.PropertyField(flattenProp);

        Vector3Int sz = new Vector3Int(
            sizeProp.FindPropertyRelative("x").intValue,
            sizeProp.FindPropertyRelative("y").intValue,
            sizeProp.FindPropertyRelative("z").intValue
        );

        sz.x = Mathf.Max(1, sz.x);
        sz.y = Mathf.Max(1, sz.y);
        sz.z = Mathf.Max(1, sz.z);

        sizeProp.FindPropertyRelative("x").intValue = sz.x;
        sizeProp.FindPropertyRelative("y").intValue = sz.y;
        sizeProp.FindPropertyRelative("z").intValue = sz.z;

        int expectedSize = sz.x * sz.y * sz.z;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Matrix Controls", EditorStyles.boldLabel);

        if (matrixProp.arraySize != expectedSize)
        {
            if (GUILayout.Button("Resize Matrix to Fit Size"))
            {
                matrixProp.arraySize = expectedSize;
                for (int i = 0; i < expectedSize; i++)
                    matrixProp.GetArrayElementAtIndex(i).intValue = 0;

                EditorUtility.SetDirty(config);
            }
        }

        if (GUILayout.Button("Clear Matrix"))
        {
            matrixProp.arraySize = expectedSize;
            for (int i = 0; i < expectedSize; i++)
                matrixProp.GetArrayElementAtIndex(i).intValue = 0;

            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Matrix Editor (Z layers)", EditorStyles.boldLabel);
        currentLayer = EditorGUILayout.IntSlider("Layer (Z)", currentLayer, 0, Mathf.Max(0, sz.z - 1));

        for (int y = sz.y - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < sz.x; x++)
            {
                int idx = x + sz.x * (y + sz.y * currentLayer);
                if (idx < matrixProp.arraySize)
                {
                    SerializedProperty elem = matrixProp.GetArrayElementAtIndex(idx);
                    elem.intValue = EditorGUILayout.IntField(elem.intValue, GUILayout.Width(30));
                }
                else
                {
                    EditorGUILayout.LabelField("?", GUILayout.Width(30));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawVector3Int(SerializedProperty prop, string label)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(150));
        prop.FindPropertyRelative("x").intValue = EditorGUILayout.IntField(prop.FindPropertyRelative("x").intValue);
        prop.FindPropertyRelative("y").intValue = EditorGUILayout.IntField(prop.FindPropertyRelative("y").intValue);
        prop.FindPropertyRelative("z").intValue = EditorGUILayout.IntField(prop.FindPropertyRelative("z").intValue);
        EditorGUILayout.EndHorizontal();
    }
}

#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AlphamapProceduralEditor))]
public class AlphamapProceduralEditorButtons : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AlphamapProceduralEditor myScript = (AlphamapProceduralEditor)target;
        
        if (GUILayout.Button("AssignSplatMap"))
        {
            myScript.AssignSplatMap();
        }

        if (GUILayout.Button("Save alphamap"))
        {
            myScript.SaveAlphamap();
        }
    }
}
#endif
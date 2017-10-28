#if (UNITY_EDITOR)
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneryProceduralEditor))]
public class SceneryProceduralEditorButtons : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneryProceduralEditor myScript = (SceneryProceduralEditor)target;

        if (GUILayout.Button("ClearStonesAndVegetation"))
        {
            myScript.ClearTrees();
        }
    }
}
#endif

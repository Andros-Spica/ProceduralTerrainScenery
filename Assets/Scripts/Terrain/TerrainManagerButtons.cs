#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainManager))]
public class TerrainManagerButtons : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainManager myScript = (TerrainManager)target;
        if (GUILayout.Button("Load heightmap"))
        {
            myScript.LoadHeightmap();
        }

        if (GUILayout.Button("Load alphamap"))
        {
            myScript.LoadAlphamap();
        }
    }
}
#endif
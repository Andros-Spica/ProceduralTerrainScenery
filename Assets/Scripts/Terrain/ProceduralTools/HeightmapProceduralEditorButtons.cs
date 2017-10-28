#if (UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(HeightmapProceduralEditor))]
public class HeightmapProceduralEditorButtons : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

        HeightmapProceduralEditor myScript = (HeightmapProceduralEditor)target;
        
        if (GUILayout.Button("Add noise"))
		{
			myScript.AddNoise();
		}
		if (GUILayout.Button("Smooth"))
		{
			myScript.Smooth();
		}

		if (GUILayout.Button("ResetTerrain"))
		{
			myScript.ResetTerrain();
		}

		if (GUILayout.Button ("AddStreams")) {
			myScript.AddStreams ();
		}
        if (GUILayout.Button("Save heightmap"))
        {
            myScript.SaveHeightmap();
        }
    }
}
#endif

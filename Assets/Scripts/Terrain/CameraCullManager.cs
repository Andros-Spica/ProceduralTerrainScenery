using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraCullManager : MonoBehaviour
{
	public float[] layerCullDistances;

	void Start()
	{
		Camera camera = GetComponent<Camera>();
		float[] distances = new float[32];
		for (int i = 0; i < 32; i++) 
		{
			if (i < layerCullDistances.Length)
				distances[i] = layerCullDistances[i];
		}
		camera.layerCullDistances = distances;
	}
}

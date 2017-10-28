using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowControl : MonoBehaviour {

    TerrainManager terrainManager;

    void Awake ()
    {
        terrainManager = FindObjectOfType<TerrainManager>();
    }

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(terrainManager.SetUp());
	}
}

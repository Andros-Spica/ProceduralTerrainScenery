using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintTerrainColliderInfo : MonoBehaviour {

    TerrainCollider terrainCollider;

	// Use this for initialization
	void Start () {
        terrainCollider = GetComponent<TerrainCollider>();
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(terrainCollider.bounds);
	}
}

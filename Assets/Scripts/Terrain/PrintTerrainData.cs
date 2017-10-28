using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintTerrainData : MonoBehaviour {

    public Text display;
    TerrainManager terrainManager;
    TerrainData terrainData;

    void Start()
    {
        terrainManager = FindObjectOfType<TerrainManager>();
        terrainData = Terrain.activeTerrain.terrainData;
    }

    void Update()
    {
        display.text =
            "position: " + transform.position +
            "\nsector: " + terrainManager.sectorLoaded.ToString() +
            "\nheight: " + terrainData.GetInterpolatedHeight(
                (transform.position.x - terrainManager.sectorLoaded.x) / terrainManager.loadedTerrainSize.x,
                (transform.position.z - terrainManager.sectorLoaded.y) / terrainManager.loadedTerrainSize.z) +
            "\nslope: " + terrainManager.GetSlopeAtPoint(transform.position.x, transform.position.z);
    }

}

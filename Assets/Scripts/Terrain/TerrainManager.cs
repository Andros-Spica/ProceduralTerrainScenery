using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TestSimpleRNG;
using VectorExtension;
using System.Text.RegularExpressions;
using System.Linq;

public class TerrainManager : MonoBehaviour
{
    // size of whole terrain
    public Vector3 terrainSize = new Vector3(12000f, 1000f, 12000f);
    
    public IntVector2 centralPatch = new IntVector2(16, 80);
    [HideInInspector]
    public Vector3 centralPosition;
    
    public enum ValidHeightmapResolution { x513, x1025, x2049 }

    public ValidHeightmapResolution heightmapResolution = ValidHeightmapResolution.x2049;
    public ValidHeightmapResolution loadedHeightmapResolution = ValidHeightmapResolution.x513;

    public int worldPatchesX = 100;
    public int worldPatchesY = 100;

    // limits of rectangular grid, given squared heighmap and to better fit spatial data
    public Range posXRange = new Range(2450f, 10000f);
    public Range posZRange = new Range(0f, 12000f);
    
    [HideInInspector]
    public Vector2 patchSize { get; set; } // patch width/height

    [HideInInspector]
    public Vector3 loadedTerrainSize;

    [HideInInspector]
    public Rect sectorLoaded; // terrain sector to be loaded in world coordinates
    
    int heightmapRes, alphamapRes, loadedHeightmapRes, loadedAlphamapRes;

    [HideInInspector]
    public float[,] streamDepressions;
    
    // other scene objects
    Transform player;
    //CharacterControls FPController;
    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController FPController;
    LoadSceneryAndLore loadSceneryAndLore;
    
    Terrain actTerrain;
    TerrainData terrainData;

    void Awake()
    {
        //FPController = FindObjectOfType<CharacterControls>();
        loadSceneryAndLore = FindObjectOfType<LoadSceneryAndLore>();
        player = FPController.GetComponent<Transform>();

        // get terrain
        actTerrain = Terrain.activeTerrain;
        terrainData = actTerrain.terrainData;

        StartCoroutine(EnterLoadingMode());
    }


    public IEnumerator SetUp()
    {
        
        Debug.Log("set up Terrain Manager...");
        Debug.Log("initializing variables...");

        SetUpTerrainDimensions();

        // check if given posXRange fits the terrainData.size.x
        if (posXRange.min < 0f || posXRange.max > terrainSize.x) Debug.LogWarning("posXRange must define values between 0 and terrainSize.x");
        if (posZRange.min < 0f || posZRange.max > terrainSize.z) Debug.LogWarning("posZRange must define values between 0 and terrainSize.x");

        // calculate patch size in world units
        patchSize = new Vector2(posXRange.range / worldPatchesX, posZRange.range / worldPatchesY);

        // Get central patch
        centralPatch = new IntVector2(50, 60);

        // convert initial position in patch coordinates to world coordinates
        centralPosition = GetPositionFromPatchCoordinates(centralPatch);

        // get sector to load around initial position
        sectorLoaded = GetTerrainSectorToLoad(centralPosition);

        // move terrain to loaded sector position
        actTerrain.transform.position = new Vector3(sectorLoaded.x, 0, sectorLoaded.y);

        Debug.Log("done");

        // load heightmap and alphamap in sector from file
        Debug.Log("loading heightmap...");
        LoadHeightmap();
        Debug.Log("done");

        Debug.Log("loading alphamap...");
        LoadAlphamap();
        Debug.Log("done");

        // place FPSController two units above the ground
        player.position = new Vector3(
            centralPosition.x,
            2 + GetHeightAtPoint(centralPosition.x, centralPosition.z),
            centralPosition.z);

        yield return StartCoroutine(LoadTerrainObjects());
    }

    private IEnumerator LoadTerrainObjects()
    {
        Debug.Log("loading terrain objects...");

        // Generate scenery and lore
        loadSceneryAndLore.enabled = true;
        Debug.Log("generating scenery and lore...");
        yield return new WaitUntil(() => loadSceneryAndLore.CountPatchesLoaded() >= 1);
        Debug.Log("done");

        StartCoroutine(ExitLoadingMode());
    }

    private IEnumerator EnterLoadingMode()
    {
        Debug.Log("entering loading mode...");
        
        // freeze player
        FPController.enabled = false;
        
        // deactivate load scenery and lore
        loadSceneryAndLore.enabled = false;
        
        Debug.Log("done");

        yield return null;
    }

    private IEnumerator ExitLoadingMode()
    {
        Debug.Log("exiting loading mode...");
        
        // unfreeze player
        FPController.enabled = true;
        
        yield return null;
    }

    #region terrain procedures


    void SetUpTerrainDimensions()
    {
        switch (heightmapResolution)
        {
            case ValidHeightmapResolution.x513:
                heightmapRes = 513;
                alphamapRes = 512;
                break;
            case ValidHeightmapResolution.x1025:
                heightmapRes = 1025;
                alphamapRes = 1024;
                break;
            case ValidHeightmapResolution.x2049:
                heightmapRes = 2049;
                alphamapRes = 2048;
                break;
        }
        switch (loadedHeightmapResolution)
        {
            case ValidHeightmapResolution.x513:
                loadedHeightmapRes = 513;
                loadedAlphamapRes = 512;
                break;
            case ValidHeightmapResolution.x1025:
                loadedHeightmapRes = 1025;
                loadedAlphamapRes = 1024;
                break;
            case ValidHeightmapResolution.x2049:
                loadedHeightmapRes = 2049;
                loadedAlphamapRes = 2048;
                break;
        }
        
        loadedTerrainSize = new Vector3(
            loadedHeightmapRes * terrainSize.x / heightmapRes,
            terrainSize.y,
            loadedHeightmapRes * terrainSize.z / heightmapRes
            );

        terrainData.heightmapResolution = loadedHeightmapRes;
        terrainData.alphamapResolution = loadedAlphamapRes;
        terrainData.size = loadedTerrainSize;
    }

    public float GetDistanceFromCenterToSide()
    {
        return loadedTerrainSize.x / 2;
    }

    //float GetMaxHeightInSectorLoaded()
    //{
    //    float maxHeight = 0f;
    //    for (int x = 0; x < loadedHeightmapRes; x++)
    //    {
    //        for (int y = 0; y < loadedHeightmapRes; y++)
    //        {
    //            float height = terrainData.GetHeight(x, y);
    //            return height;
    //            if (height > maxHeight)
    //                maxHeight = height;
    //        }
    //    }
    //    return maxHeight;
    //}

    public void LoadStreams()
    {
        //Serialization.LoadStreams(streamDepressions, GetHeightmapPointsToLoad(sectorLoaded));
    }

    public void LoadAlphamap()
    {
        //Serialization.LoadAlphamap(terrainData, GetAlphamapPointsToLoad(sectorLoaded));
        float[,,] alphamapSector;
        Serialization.LoadRaster3D("alphamap", GetAlphamapPointsToLoad(sectorLoaded), terrainData.splatPrototypes.Length, out alphamapSector);
        terrainData.SetAlphamaps(0, 0, alphamapSector);
    }

    Rect GetAlphamapPointsToLoad(Rect terrainSectorToLoad)
    {
        return new Rect(
            Mathf.FloorToInt((terrainSectorToLoad.x / terrainSize.x) * alphamapRes),
            Mathf.FloorToInt((terrainSectorToLoad.y / terrainSize.z) * alphamapRes),
            loadedAlphamapRes,
            loadedAlphamapRes
            );
    }

    public void LoadHeightmap()
    {
        float[,] heightmapSector;
        Serialization.LoadRaster2D("heightmap", GetHeightmapPointsToLoad(sectorLoaded), out heightmapSector);
        terrainData.SetHeights(0, 0, heightmapSector);
    }

    Rect GetHeightmapPointsToLoad(Rect terrainSectorToLoad)
    {
        return new Rect(
            Mathf.FloorToInt((terrainSectorToLoad.x / terrainSize.x) * heightmapRes),
            Mathf.FloorToInt((terrainSectorToLoad.y / terrainSize.z) * heightmapRes),
            loadedHeightmapRes,
            loadedHeightmapRes
            );
    }

    Rect GetTerrainSectorToLoad(Vector3 pos)
    {
        return new Rect(
            Mathf.Clamp(pos.x - loadedTerrainSize.x / 2, 0f, terrainSize.x - loadedTerrainSize.x),
            Mathf.Clamp(pos.z - loadedTerrainSize.z / 2, 0f, terrainSize.z - loadedTerrainSize.z),
            loadedTerrainSize.x, loadedTerrainSize.z);
    }


    #endregion


    #region getters


    public Vector3 GetPositionFromPatchCoordinates(IntVector2 patchCoord)
    {
        return GetPositionFromPatchCoordinates(patchCoord.x, patchCoord.y);
    }
    public Vector3 GetPositionFromPatchCoordinates(int x, int z)
    {
        float posX = posXRange.min + x * patchSize.x;
        float posZ = posZRange.min + z * patchSize.y;
        return new Vector3(posX, 0f, posZ);
    }

    //<summary>Get the patch that contains the position</summary>
    public IntVector2 GetPatchCoordFromPosition(Vector3 pos)
    {
        return GetPatchCoordFromPosition(pos.x, pos.z);
    }
    public IntVector2 GetPatchCoordFromPosition(float posX, float posZ)
    {
        int x = Mathf.FloorToInt(worldPatchesX * (posX - posXRange.min) / (posXRange.range));
        int y = Mathf.FloorToInt(worldPatchesY * (posZ - posZRange.min) / (posZRange.range));
        return new IntVector2(x, y);
    }
    
    //<summary>Get slope at point. 
    // Steepness is given as an angle, 0..90 degrees. scaleToRatio will divide
    // by 90 to get a value in the range 0..1.</summary>
    public float GetSlopeAtPoint(float pointX, float pointZ, bool scaleToRatio = true)
    {
        float factor = (scaleToRatio) ? 90f : 1f;
        return terrainData.GetSteepness(
            (pointX - sectorLoaded.x) / loadedTerrainSize.x, // x and z coordinates must always be scaled here
            (pointZ - sectorLoaded.y) / loadedTerrainSize.z
            ) / factor; 
    }

    //<summary> Get normal at point</summary>
    public Vector3 GetNormalAtPoint(float pointX, float pointZ)
    {
        RaycastHit hit;
        Vector3 castPos = new Vector3(pointX, GetHeightAtPoint(pointX, pointZ) + 20, pointZ);
        if (Physics.Raycast(castPos, Vector3.down, out hit))
        {
            return hit.normal;
        }
        return Vector3.zero;
    }

    //<summary> Get height at point</summary>
    public float GetHeightAtPoint(float pointX, float pointZ)
    {
        return actTerrain.SampleHeight(new Vector3(pointX, 0f, pointZ));
    }

    //<summary>Get a random position within patch and over the surface</summary>
    public Vector3 GetRandomPointInPatchSurface(IntVector2 patchCoord, System.Random RNG, bool scaleToTerrain = false)
    {
    //    Rect patchRect = GetPatchRect(patchCoord.x, patchCoord.y);
    //    return GetRandomPointInPatchSurface(patchRect, scaleToTerrain);
    //}
    //public Vector3 GetRandomPointInPatchSurface(Rect patchRect, bool scaleToTerrain = false)
    //{
        
        Rect patchRect = GetPatchRect(patchCoord.x, patchCoord.y);
        float pointX = patchRect.x + (float)RNG.NextDouble() * patchRect.width;
        float pointZ = patchRect.y + (float)RNG.NextDouble() * patchRect.height;
        float pointHeight = GetHeightAtPoint(pointX, pointZ);
        if (scaleToTerrain)
        {
            return new Vector3(
                (pointX - sectorLoaded.x) / loadedTerrainSize.x,
                pointHeight,
                (pointZ - sectorLoaded.y) / loadedTerrainSize.z
                );
        }
        return new Vector3(pointX, pointHeight, pointZ);
    }

    //<summary>Get the rectangle corresponding to the patch surface</summary>
    public Rect GetPatchRect(IntVector2 patchCoord)
    {
        return GetPatchRect(patchCoord.x, patchCoord.y);
    }
    public Rect GetPatchRect(int x, int y)
    {
        return new Rect( // must account for the margin in the x and z axes (e.g. because of the rectangular shape of the grid map)
            posXRange.min + posXRange.range * x / worldPatchesX,
            posZRange.min + posZRange.range * y / worldPatchesY,
            patchSize.x,
            patchSize.y
        );
    }

    public IntVector2 WorldToLoadedHeightmap(Vector3 point)
    {
        return new IntVector2(
            Mathf.FloorToInt((point.x - sectorLoaded.x) * loadedHeightmapRes / loadedTerrainSize.x),
            Mathf.FloorToInt((point.z - sectorLoaded.y) * loadedHeightmapRes / loadedTerrainSize.z)
            );
    }

    public IntVector2 WorldToWholeHeightmap(Vector3 point)
    {
        return new IntVector2(
            Mathf.FloorToInt(point.x * loadedHeightmapRes / terrainSize.x),
            Mathf.FloorToInt(point.z * loadedHeightmapRes / terrainSize.z)
            );
    }

    public Vector2 LoadedHeightmapToWorld(IntVector2 point)
    {
        return new Vector2(
            (loadedTerrainSize.x * point.x / loadedHeightmapRes) + sectorLoaded.x,
            (loadedTerrainSize.z * point.y / loadedHeightmapRes) + sectorLoaded.y
            );
    }

    public Vector2 WholeHeightmapToWorld(IntVector2 point)
    {
        return new Vector2(
            (point.x * terrainSize.x) / heightmapRes,
            (point.y * terrainSize.x) / heightmapRes
            );
    }


    #endregion
}

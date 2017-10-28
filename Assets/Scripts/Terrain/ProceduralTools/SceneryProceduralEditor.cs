using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VectorExtension;
using TestSimpleRNG;

public class SceneryProceduralEditor : MonoBehaviour {

    //Tree settings
    public int treeNoiseSeed = 54;
    public float treeNoiseFrequency = 12f;
    public float treeNoiseScale = 0.001f;
    public float treeDensity = 1f;
    public float treeDensityStdev = 1f;
    public Range treeScale = new Range(.01f, .1f);

    public int cactusNoiseSeed = 678;
    public float cactusNoiseFrequency = 12f;
    public float cactusNoiseScale = 0.001f;
    public float cactusDensity = 200f;
    public float cactusDensityStdev = 20f;
    // max scale variation for cactus
    public Range cactusScale = new Range(1f, 5f);

    //The distance at which trees will no longer be drawn
    public float m_treeDistance = 2000.0f;
    //The distance at which trees meshes will turn into tree billboards
    public float m_treeBillboardDistance = 400.0f;
    //As trees turn to billboards there transform is rotated to match the meshes, a higher number will make this transition smoother
    public float m_treeCrossFadeLength = 20.0f;
    //The maximum number of trees that will be drawn in a certain area.
    public int m_treeMaximumFullLODCount = 400;

    // stone parameters
    public int stoneSeed = 392;
    public Range stonePlainDensity = new Range(20f, 50f);
    public Range stonePlainScale = new Range(0.5f, 5f);
    public Vector3 stonePlainOffset = Vector3.down * 0.6f;
    public Range stonePlainSlope = new Range(0.3f, 0.8f);
    public Range stoneCliffDensity = new Range(200f, 500f);
    public Range stoneCliffScale = new Range(0.5f, 20f);
    public Vector3 stoneCliffOffset = Vector3.down * 1f;
    public Range stoneCliffSlope = new Range(0.5f, 1f);

    public GameObject[] treePrefabs;
    public GameObject[] cactusPrefabs;
    public GameObject[] stonePlainPrefabs;
    public GameObject[] stoneCliffPrefabs;

    TreePrototype[] m_treeProtoTypes;

    public TerrainManager terrainManager;
    public LoadSceneryAndLore loadSceneryAndLore;

    Terrain actTerrain;
    TerrainData terrainData;

    PerlinNoise vegetationNoiseOutsideModel;
    System.Random treeRNG;
    System.Random cactusRNG;
    System.Random stoneRNG;

    void Awake()
    {
        actTerrain = Terrain.activeTerrain;
        terrainData = actTerrain.terrainData;

        // set up terrain general properties
        actTerrain.treeDistance = m_treeDistance;
        actTerrain.treeBillboardDistance = m_treeBillboardDistance;
        actTerrain.treeCrossFadeLength = m_treeCrossFadeLength;
        actTerrain.treeMaximumFullLODCount = m_treeMaximumFullLODCount;
        vegetationNoiseOutsideModel = new PerlinNoise(treeNoiseSeed);

        // clear trees
        List<TreeInstance> newTrees = new List<TreeInstance>(0);
        terrainData.treeInstances = newTrees.ToArray();

        FillPrototypes();

    }

    void FillPrototypes()
    {
        m_treeProtoTypes = new TreePrototype[treePrefabs.Length + cactusPrefabs.Length];

        // vegetation
        for (int i = 0; i < treePrefabs.Length; i++)
        {
            m_treeProtoTypes[i] = new TreePrototype();
            m_treeProtoTypes[i].prefab = treePrefabs[i];
        }
        for (int i = treePrefabs.Length; i < treePrefabs.Length + cactusPrefabs.Length; i++)
        {
            m_treeProtoTypes[i] = new TreePrototype();
            m_treeProtoTypes[i].prefab = cactusPrefabs[i - treePrefabs.Length];
        }

        terrainData.treePrototypes = m_treeProtoTypes;
    }

    public void UpdateVegetationAtPatch(IntVector2 patchCoord, Transform patchHolder)
    {
        // reset SimpleRNG using tree seed
        SimpleRNG.SetSeed((uint)(treeNoiseSeed + patchCoord.x + patchCoord.y * terrainManager.worldPatchesX));
        // set RNG seed depending on tree seed, world seed, and patch coordinates
        treeRNG = new System.Random(treeNoiseSeed + patchCoord.x + patchCoord.y * terrainManager.worldPatchesX);

        if (treePrefabs.Length > 0)
        {
            // get the number of trees in the patch (vegetation density)
            int numberOftrees = 0;
            if (true)
                numberOftrees = Mathf.RoundToInt(Mathf.Max(0f,(float)SimpleRNG.GetNormal(treeDensity, treeDensityStdev)));
            else
                numberOftrees = Mathf.RoundToInt(vegetationNoiseOutsideModel.FractalNoise2D(patchCoord.x, patchCoord.y, 4, treeNoiseFrequency, treeNoiseScale) * treeDensity);
            
            // place trees
            for (int i = 0; i < numberOftrees; i++)
            {

                // scale x and z coordinates for placing prefab prototypes in terrain. Note that this is not needed when placing GameObjects
                //Vector3 point = terrainManager.GetRandomPointInPatchSurface(patchCoord, treeRNG, scaleToTerrain: true);
                Vector3 point = loadSceneryAndLore.GetValidRandomPositionInPatch(patchCoord, 1f, treeRNG);

                float slope = terrainManager.GetSlopeAtPoint(point.x, point.z);

                if (slope < 0.7f)
                { //make sure tree are not on cliffs
                    GameObject temp = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], point, Quaternion.identity);
                    //temp.transform.Translate(Vector3.down * 1f);
                    temp.transform.localScale = Vector3.one * (treeScale.min + (float)treeRNG.NextDouble() * (treeScale.max - treeScale.min));
                    temp.transform.Rotate(Vector3.up * Random.Range(0f, 360f));
                    
                    temp.transform.parent = patchHolder.transform;
                    //					TreeInstance temp = new TreeInstance ();
                    //					temp.position = point;
                    //					temp.rotation = Random.Range (0f, 360f) * Mathf.Deg2Rad;
                    //					temp.prototypeIndex = Random.Range (0, treePrefabs.Length);
                    //					temp.color = Color.white;
                    //					temp.lightmapColor = Color.white;
                    //
                    //					actTerrain.AddTreeInstance (temp);
                }
            }
        }
        //Debug.Log (terrainData.treeInstanceCount);

        // reset SimpleRNG using cactus seed
        SimpleRNG.SetSeed((uint)(cactusNoiseSeed + patchCoord.x + patchCoord.y * terrainManager.worldPatchesX));
        // set RNG seed depending on cactus seed, world seed, and patch coordinates
        cactusRNG = new System.Random(cactusNoiseSeed + patchCoord.x + patchCoord.y * terrainManager.worldPatchesX);

        if (cactusPrefabs.Length > 0)
        {
            // get the number of cactus in the patch (vegetation density)
            int numberOfCactus = 0;
            if (true)
                numberOfCactus = Mathf.RoundToInt(Mathf.Max((float)SimpleRNG.GetNormal(cactusDensity, cactusDensityStdev)));
            else
                numberOfCactus = Mathf.RoundToInt(vegetationNoiseOutsideModel.FractalNoise2D(patchCoord.x, patchCoord.y, 4, cactusNoiseFrequency, cactusNoiseScale) * cactusDensity);
            
            // place trees
            for (int i = 0; i < numberOfCactus; i++)
            {
                //Debug.Log(patchCoord.ToString());
                // scale x and z coordinates for placing prefab prototypes in terrain. Note that this is not needed when placing GameObjects
                //Vector3 point = terrainManager.GetRandomPointInPatchSurface(patchCoord, cactusRNG, scaleToTerrain: true);
                Vector3 point = loadSceneryAndLore.GetValidRandomPositionInPatch(patchCoord, 1f, cactusRNG);

                // normal diffusion
                //point = new Vector3(point.x + (float)SimpleRNG.GetNormal(0, .01f), 0f, point.z + (float)SimpleRNG.GetNormal(0, 0.01f));

                float slope = terrainManager.GetSlopeAtPoint(point.x, point.z);

                if (slope < 0.7f)
                { //make sure cactus are not on cliffs

                    GameObject temp = Instantiate(cactusPrefabs[cactusRNG.Next(0, cactusPrefabs.Length)], point, Quaternion.identity);
                    //temp.transform.Translate(Vector3.down * 0.2f);
                    temp.transform.localScale = Vector3.one * (cactusScale.min + (float)cactusRNG.NextDouble() * (cactusScale.max - cactusScale.min));
                    temp.transform.Rotate(Vector3.up * Random.Range(0f, 360f));
                    
                    temp.transform.parent = patchHolder.transform;
                    //TreeInstance temp = new TreeInstance();
                    //temp.position = point;
                    //temp.rotation = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    //temp.prototypeIndex = Random.Range(treePrefabs.Length, treePrefabs.Length + cactusPrefabs.Length);
                    //float randomScale = Random.Range(1f, cactusMaxScale);
                    //temp.widthScale = randomScale;
                    //temp.heightScale = randomScale;
                    //temp.color = Color.white;
                    //temp.lightmapColor = Color.white;
                    //actTerrain.AddTreeInstance(temp);
                }
            }
        }
    }

    public void UpdateStonesAtPatch(IntVector2 patchCoord, Transform patchHolder)
    {
        // reset SimpleRNG using cactus seed
        SimpleRNG.SetSeed((uint)(stoneSeed + patchCoord.x + patchCoord.y * terrainManager.worldPatchesX));
        // set RNG seed depending on stone seed, world seed, and patch coordinates
        stoneRNG = new System.Random(stoneSeed + patchCoord.x + patchCoord.y * terrainManager.worldPatchesX);

        // in plain terrain
        float stonePlainDensityHere = stonePlainDensity.min + (float)stoneRNG.NextDouble() * (stonePlainDensity.max - stonePlainDensity.min);
        
        for (int i = 0; i < stonePlainDensityHere; i++)
        {
            Vector3 point = loadSceneryAndLore.GetValidRandomPositionInPatch(patchCoord, 1f, stoneRNG);
            
            float slope = terrainManager.GetSlopeAtPoint(point.x, point.z);
            //Debug.Log(point + ", " + slope);

            if (slope > stonePlainSlope.min && slope < stonePlainSlope.max)
            {
                GameObject temp = Instantiate(stonePlainPrefabs[stoneRNG.Next(stonePlainPrefabs.Length)], point, Quaternion.identity);
                temp.transform.localScale = Vector3.one * (stonePlainScale.min + (float)stoneRNG.NextDouble() * (stonePlainScale.max - stonePlainScale.min));
                temp.transform.Translate(stonePlainOffset * temp.transform.localScale.x);
                temp.transform.Rotate(Vector3.up * (float)stoneRNG.NextDouble() * 360f);
                
                temp.transform.parent = patchHolder.transform;
            }
        }

        // in cliffs
        float stoneCliffDensityHere = stoneCliffDensity.min + (float)stoneRNG.NextDouble() * (stoneCliffDensity.max - stoneCliffDensity.min);

        for (int i = 0; i < stoneCliffDensityHere; i++)
        {
            Vector3 point = loadSceneryAndLore.GetValidRandomPositionInPatch(patchCoord, 1f, stoneRNG);

            float slope = terrainManager.GetSlopeAtPoint(point.x, point.z);

            if (slope > stoneCliffSlope.min && slope < stoneCliffScale.max)
            {
                GameObject temp = Instantiate(stoneCliffPrefabs[stoneRNG.Next(stoneCliffPrefabs.Length)], point, Quaternion.identity);
                temp.transform.localScale = Vector3.one * (stoneCliffScale.min + (float)stoneRNG.NextDouble() * (stoneCliffScale.max - stoneCliffScale.min)); // re-scale
                temp.transform.Translate(stoneCliffOffset * temp.transform.localScale.x); // apply downwards offset
                temp.transform.Rotate(Vector3.up * (float)stoneRNG.NextDouble() * 360f);
                
                temp.transform.parent = patchHolder.transform;
            }
        }

    }

    public void ClearTrees()
    {
        terrainData.treeInstances = new TreeInstance[0];
    }

}

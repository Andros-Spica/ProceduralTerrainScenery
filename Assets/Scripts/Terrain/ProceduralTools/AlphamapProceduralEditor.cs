using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VectorExtension;

public class AlphamapProceduralEditor : MonoBehaviour
{
    public int alphaNoiseSeed = 0;
    public float alphaFrq = 8f;
    public float alphaNoiseScale = 0.001f;
    public float lowlandThreshold = 0.15f;
    public float midStrataLowerThreshold = 0.18f;
    public float midStrataMiddleThreshold = 0.2f;
    public float midStrataUpperThreshold = 0.23f;
    public float highlandThreshold = 0.25f;
    public float steepTerrainThreshold = 0.3f;
    public float plainTextureWeight = 1.0f;
    public float steepTextureWeight = 2.0f;
    public float highlandTextureWeight = 1.5f; // apply 75% only to "Mesa" uplands (NOTE: the first two textures sum 1, so 1.5 corresponds to 80%)
    public float midStrataTextureWeight = 1f;
    public float lowlandTextureWeight = 1f; // apply 50% only to valley floor

    public float[] m_splatTileSize = { 10.0f, 5.0f, 15.0f, 10.0f, 10.0f };
    public Texture2D[] terrainTextures;
    public Texture2D[] terrainNormals;
    
    SplatPrototype[] m_splatPrototypes;

    public TerrainManager terrainManager;
    TerrainData terrainData;
    PerlinNoise alphaNoise;

    float[,] streamDepressions;

    void Awake()
    {
        terrainData = Terrain.activeTerrain.terrainData;
        alphaNoise = new PerlinNoise(alphaNoiseSeed);

        CreateSplatProtoTypes();
        terrainData.splatPrototypes = m_splatPrototypes;
    }

    public void SaveAlphamap()
    {
        if (terrainData.alphamapResolution != 2048)
        {
            Debug.LogWarning("Please only save heightmaps corresponding to a 2048x2048 resolution.");
            return;
        }
        Serialization.SaveRaster3D("alphamap", terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution));
    }

    void CreateSplatProtoTypes()
    {
        m_splatPrototypes = new SplatPrototype[terrainTextures.Length];

        for (int i = 0; i < terrainTextures.Length; i++)
        {
            m_splatPrototypes[i] = new SplatPrototype();
            m_splatPrototypes[i].texture = terrainTextures[i];
            m_splatPrototypes[i].normalMap = terrainNormals[i];
            m_splatPrototypes[i].tileSize = new Vector2(m_splatTileSize[i], m_splatTileSize[i]);
        }
    }

    // thanks to https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/
    public void AssignSplatMap()
    {
        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        float terrainMaxHeight = terrainData.size.y;

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                // Assign this point to the splatmap array
                float[] alphaWeights = GetWeightsForPoint(x, y, terrainMaxHeight);

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    splatmapData[y, x, i] = alphaWeights[i]; // NOTE: Alpha map array dimensions are shifted in relation to heightmap and world space (y is x and x is y or z)
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);

        //for (int i = 0; i < 5; i++)
        //{
        //    Debug.Log(terrainData.GetAlphamaps(250, 250, 1, 1)[0, 0, i]);
        //}
        //Debug.Log(terrainData.size.x);
        //Debug.Log(highlandThreshold * terrainMaxHeight);
    }

    public void UpdateAlphamapArea(Rect area)
    {
        IntVector2 bottomLeft = terrainManager.WorldToLoadedHeightmap(new Vector3(area.x, 0f, area.y));
        //Debug.Log(bottomLeft);
        IntVector2 topRight = terrainManager.WorldToLoadedHeightmap(new Vector3(area.xMax, 0f, area.yMax));
        //Debug.Log(topRight);

        float[,,] areaAlphas = terrainData.GetAlphamaps(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);

        float terrainMaxHeight = terrainData.size.y;

        for (int x = 0; x < areaAlphas.GetLength(1); x++)
        {
            for (int y = 0; y < areaAlphas.GetLength(0); y++)
            {
                // Assign this point to the splatmap array
                float[] alphaWeights = GetWeightsForPoint(x, y, terrainMaxHeight);

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    areaAlphas[y, x, i] = alphaWeights[i]; // NOTE: Alpha map array dimensions are shifted in relation to heightmap and world space (y is x and x is y or z)
                }
            }
        }

        terrainData.SetAlphamaps(bottomLeft.x, bottomLeft.y, areaAlphas);
    }

    private float[] GetWeightsForPoint(int x, int y, float terrainMaxHeight)
    {
        // Normalise x/y coordinates to range 0-1 
        float y_01 = (float)y / (float)terrainData.alphamapHeight;
        float x_01 = (float)x / (float)terrainData.alphamapWidth;

        // Setup an array to record the mix of texture weights at this point
        float[] splatWeights = new float[terrainData.alphamapLayers];

        // get height and slope at corresponding point
        float height = terrainData.GetInterpolatedHeight(x_01, y_01);
        float slope = terrainData.GetSteepness(x_01, y_01) / 90.0f;

        float noise = alphaNoise.FractalNoise2D(x_01 * terrainData.size.x, y_01 * terrainData.size.z, 4, alphaFrq, alphaNoiseScale);
        //if (x == 250 && y == 250) Debug.Log(slope + " " + height + " " + noise);
        splatWeights[0] = Mathf.Max(0f, plainTextureWeight - (slope * steepTextureWeight)); // decreases with slope (ground texture)

        splatWeights[1] = slope * steepTextureWeight; // increases with slope (rocky texture)

        // uplands terrain
        splatWeights[2] = (
        height > (highlandThreshold * terrainMaxHeight) && // higher than threshold
        slope < steepTerrainThreshold) // plain terrain
            ? (highlandTextureWeight + noise) : 0f;

        // mid strata terrain
        splatWeights[3] = (
                             height < (midStrataUpperThreshold * terrainMaxHeight) && // lower than upper threshold
                             height > (midStrataMiddleThreshold * terrainMaxHeight) && // higher than middle threshold
                             slope > steepTerrainThreshold) // steep terrain
                             ? (midStrataTextureWeight + noise) : 0f;

        splatWeights[4] = (
                             height < (midStrataMiddleThreshold * terrainMaxHeight) && // lower than middle threshold
                             height > (midStrataLowerThreshold * terrainMaxHeight) && // higher than lower threshold
                             slope > steepTerrainThreshold) // steep terrain
                             ? (midStrataTextureWeight + noise) : 0f;

        // lowlands terrain
        splatWeights[5] = (
            height < (lowlandThreshold * terrainMaxHeight) && // lower than threshold
            slope < steepTerrainThreshold) // plain terrain
            ? (lowlandTextureWeight + noise) : 0f;

        // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
        float z = splatWeights.Sum();

        // Loop through each terrain texture
        for (int i = 0; i < terrainData.alphamapLayers; i++)
        {

            // Normalize so that sum of all texture weights = 1
            splatWeights[i] /= z;
        }

        return splatWeights;
    }
}

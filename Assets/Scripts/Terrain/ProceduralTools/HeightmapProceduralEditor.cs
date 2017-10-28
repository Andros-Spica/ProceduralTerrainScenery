using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TestSimpleRNG;
using VectorExtension;
using System.Text.RegularExpressions;
using System.Linq;

public class HeightmapProceduralEditor : MonoBehaviour
{
    public int streamSEED = 99;
    private System.Random streamsRNG;
    public IntRange streamSourcesPerPatch = new IntRange(1, 5);
    public float streamDepth = 0.0005f;
    public int streamLength = 100;
    public bool displayStreams = false;
    List<List<Vector3>> streamPaths;
    float[,] streamDepressions;

    public int groundNoiseSeed = 0;
    public float groundFrq = 12f;
    public float groundNoiseScale = 0.001f;
    public float smoothIntensity = 0.2f;
    public int heightMapSize;
    
    Vector3 terrainSize;
    public float[,] originalHeights;

    public TerrainManager terrainManager;
    public AlphamapProceduralEditor alphamapProceduralEditor;
    Terrain actTerrain;
    TerrainData terrainData;
    TerrainCollider terrainCollider;
    PerlinNoise groundNoise;

    void Awake()
    {
        actTerrain = Terrain.activeTerrain;
        terrainData = actTerrain.terrainData;
        terrainCollider = actTerrain.GetComponent<TerrainCollider>();

        heightMapSize = terrainData.heightmapWidth;
        terrainSize = terrainData.size;

        streamPaths = new List<List<Vector3>>();

        originalHeights = terrainData.GetHeights(0, 0, heightMapSize, heightMapSize);
        streamDepressions = new float[originalHeights.GetLength(0),originalHeights.GetLength(1)];
    }
    
    public void SaveHeightmap()
    {
        if (terrainData.heightmapResolution != 2049)
        {
            Debug.LogWarning("Please only save heightmaps corresponding to a 2049x2049 resolution.");
            return;
        }
        Serialization.SaveRaster2D("heightmap", terrainData.GetHeights(0, 0, heightMapSize, heightMapSize));
        Serialization.SaveRaster2D("streams", streamDepressions);
    }

    #region heightmap modifications

    public void AddStreams()
    {
        //ResetTerrain();
        Vector2 patchSize = terrainManager.patchSize;

        // set up a random number generator dependent on the global RNG and the patch coordinates
        streamsRNG = new System.Random(streamSEED);

        // iterate for every patch (c. 100x100), including patches outside the grid
        for (int x = 0; x < (int)(terrainSize.x / patchSize.x); x++)
        {
            for (int z = 0; z < (int)(terrainSize.z / patchSize.y); z++)
            {
                
                // get random number of stream sources
                //int streamSources = Random.Range(streamSourcesPerPatch.min, streamSourcesPerPatch.max);
                // get number of stream sources depending on hydro (values between 0-8.6)
                int streamSources = 
                    streamSourcesPerPatch.min + 
                    Mathf.RoundToInt(streamSourcesPerPatch.range * streamsRNG.Next());

                for (int i = 0; i < streamSources; i++)
                {
                    // get a random point within a the patch
                    Vector3 sourcePosition = terrainManager.GetRandomPointInPatchSurface(new IntVector2(x, z), streamsRNG);
                    // Create a stream
                    AddStream(sourcePosition);
                }
            }
        }

        //Debug.Log(originalHeights.Cast<float>().SequenceEqual<float>(terrainData.GetHeights(0, 0, heightMapSize, heightMapSize).Cast<float>()));
    }

    void AddStream(Vector3 sourcePosition)
    {
        // initialize path
        List<Vector3> streamPath = new List<Vector3>();
        
        // add point to the path order list
        streamPath.Add(sourcePosition);

        int i = 0;
        while (i < streamLength)
        {
            Vector3 lastPoint = streamPath[streamPath.Count - 1];
            Vector3 nextPoint = GetDownstreamPoint(lastPoint);

            // check if there is no neighbour lower than this point
            if (nextPoint != lastPoint)
            {
                // add point to the path order list
                streamPath.Add(nextPoint);
            }
            else
            {
                break;
            }
            i++;
        }

        if (streamPath.Count > 1) // discard fail attempts (one-point dead ends)
        {
            streamPaths.Add(streamPath);
            DrawPathOnTerrain(streamPath);
        }
    }

    private Vector3 GetDownstreamPoint(Vector3 point)
    {
        Vector3 currentPoint = point;
        // sample 4 neighboring points
        List<IntVector2> neighbors = new List<IntVector2>
        {
            IntVector2.left, IntVector2.right, IntVector2.up, IntVector2.down
        };
        // shuffle list
        neighbors.Shuffle(streamsRNG);

        foreach (IntVector2 neighbor in neighbors)
        {
            //Vector2 unit = terrainManager.unit;
            // Get the neighbouring point
            float neighborX = point.x + (neighbor.x * terrainManager.loadedTerrainSize.x / heightMapSize);//(neighbor.x / unit.x) / heightMapSize;
            float neighborZ = point.z + (neighbor.y * terrainManager.loadedTerrainSize.z / heightMapSize);//(neighbor.y / unit.y) / heightMapSize;

            // check if the position is within limits
            if (neighborX < 0 || neighborX >= terrainData.size.x ||
                neighborZ < 0 || neighborZ >= terrainData.size.z)
            {
                continue;
            }

            // get height
            //float neighborHeight = terrainManager.GetHeightAtPoint(neighborX, neighborZ);
            float neighborHeight = Terrain.activeTerrain.SampleHeight(new Vector3(neighborX, 0f, neighborZ));

            //Debug.Log("current: " + currentPoint.y + " vs candidate: " + candidate.y);
            // Compare heights
            if (neighborHeight < currentPoint.y)
            {
                currentPoint = new Vector3(neighborX, neighborHeight, neighborZ);
            }
        }
        return currentPoint;
    }

    public void DrawPathOnTerrain(List<Vector3> path)
    {
        foreach (Vector3 point in path)
        {
            CreateCrater(point);
            //DeformTexture(point, modifiedAlphas, out modifiedAlphas);
        }
    }

    void CreateCrater(Vector3 craterWorldPosition)
    {
        // inspired by 
        // TerrainDeformer - Demonstrating a method modifying terrain in real-time. Changing height and texture
        // released under MIT License
        // http://www.opensource.org/licenses/mit-license.php
        //@author		Devin Reimer
        //@website 		http://blog.almostlogical.com
        //Copyright (c) 2010 Devin Reimer

        float craterSizeInMeters = terrainManager.WholeHeightmapToWorld(IntVector2.one).x;

        //get the heights only once keep it and reuse, precalculate as much as possible
        int heightMapCraterWidth = 1;//(int)(craterSizeInMeters * (heightMapSize / terrainSize.x)); 
        int heightMapCraterLength = 1;//(int)(craterSizeInMeters * (heightMapSize / terrainSize.z));
        float deformationDepth = streamDepth;//(craterSizeInMeters / 2.0f) / terrainSize.y;// deeper in highlands

        // bottom-lef position of the crater
        IntVector2 heightMapStartPos = terrainManager.WorldToWholeHeightmap(craterWorldPosition);
        heightMapStartPos.x = (int)(heightMapStartPos.x - (heightMapCraterWidth / 2));
        heightMapStartPos.y = (int)(heightMapStartPos.y - (heightMapCraterLength / 2));

        if (heightMapStartPos.x < 0 || heightMapStartPos.x + heightMapCraterWidth >= heightMapSize ||
            heightMapStartPos.y < 0 || heightMapStartPos.y + heightMapCraterLength >= heightMapSize)
        {
            return;
        }

        float[,] craterHeights = terrainData.GetHeights(heightMapStartPos.x, heightMapStartPos.y, heightMapCraterWidth, heightMapCraterLength);
        float circlePosX;
        float circlePosY;
        float distanceFromCenter;
        float depthMultiplier;

        for (int i = 0; i < heightMapCraterLength; i++)
        {
            for (int j = 0; j < heightMapCraterWidth; j++)
            {
                circlePosX = (j - (heightMapCraterWidth / 2)) / (heightMapSize / terrainSize.x);
                circlePosY = (i - (heightMapCraterLength / 2)) / (heightMapSize / terrainSize.z);

                //convert back to values without skew
                distanceFromCenter = Mathf.Abs(Mathf.Sqrt(circlePosX * circlePosX + circlePosY * circlePosY));

                if (distanceFromCenter < (craterSizeInMeters / 2.0f))
                {
                    depthMultiplier = ((craterSizeInMeters / 2.0f - distanceFromCenter) / (craterSizeInMeters / 2.0f));

                    depthMultiplier += 0.1f;
                    depthMultiplier += (float)streamsRNG.NextDouble() * .1f;

                    depthMultiplier = Mathf.Clamp(depthMultiplier, 0, 1);

                    float depression = deformationDepth * depthMultiplier;

                    craterHeights[i, j] = Mathf.Clamp(craterHeights[i, j] - depression, 0, 1);

                    streamDepressions[heightMapStartPos.x + i, heightMapStartPos.y + j] = depression;
                }
            }
        }
        terrainData.SetHeights(heightMapStartPos.x, heightMapStartPos.y, craterHeights);
    }

    public void LevelTerrain(Rect area, float height)
    {
        IntVector2 bottomLeft = terrainManager.WorldToLoadedHeightmap(new Vector3(area.x, 0f, area.y));
        //Debug.Log(bottomLeft);
        IntVector2 topRight = terrainManager.WorldToLoadedHeightmap(new Vector3(area.xMax, 0f, area.yMax));
        //Debug.Log(topRight);

        float[,] areaHeights = terrainData.GetHeights(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        
        for (int x = 0; x < areaHeights.GetLength(0); x++)
        {
            for (int y = 0; y < areaHeights.GetLength(1); y++)
            {
                //Debug.Log(areaHeights[x, y]);
                areaHeights[x, y] = height / terrainData.size.y;
            }
        }

        terrainData.SetHeights(bottomLeft.x, bottomLeft.y, areaHeights);

        // update textures
        alphamapProceduralEditor.UpdateAlphamapArea(area);

        terrainCollider.terrainData = terrainData;
    }

    public void AddNoise()
    {
        // set noise seed
        groundNoise = new PerlinNoise(groundNoiseSeed);

        // get the heights of the terrain
        float[,] heights = terrainData.GetHeights(0, 0, heightMapSize, heightMapSize);

        // we set each sample of the terrain in the size to the desired height
        for (int x = 0; x < heightMapSize; x++)
        {
            for (int z = 0; z < heightMapSize; z++)
            {
                heights[z, x] += groundNoise.FractalNoise2D(x, z, 4, groundFrq, groundNoiseScale) + // + 0.1f; (float)SimpleRNG.GetNormal () * groundNoiseScale;/
                groundNoise.FractalNoise2D(x, z, 4, Mathf.RoundToInt(0.8f * groundFrq), 0.8f * groundNoiseScale) +
                groundNoise.FractalNoise2D(x, z, 4, Mathf.RoundToInt(0.5f * groundFrq), 0.5f * groundNoiseScale) +
                groundNoise.FractalNoise2D(x, z, 4, Mathf.RoundToInt(0.2f * groundFrq), 0.2f * groundNoiseScale);
            }
            // set the new height
            terrainData.SetHeights(0, 0, heights);
        }
    }

    public void Smooth()
    {
        float[,] startHeights = terrainData.GetHeights(0, 0, heightMapSize, heightMapSize); // in case there were previous modifications on the original heights
        float[,] heights = terrainData.GetHeights(0, 0, heightMapSize, heightMapSize);
        for (int x = 0; x < heightMapSize; x++)
        {
            for (int z = 0; z < heightMapSize; z++)
            {
                float avHeight = 0;
                int count = 0;
                for (int xNeighbor = -1; xNeighbor <= 1; xNeighbor++)
                {
                    for (int zNeighbor = -1; zNeighbor <= 1; zNeighbor++)
                    {
                        if (z + zNeighbor >= 0 && z + zNeighbor < heightMapSize && x + xNeighbor >= 0 && x + xNeighbor < heightMapSize)
                        {
                            float neighborHeight = startHeights[z + zNeighbor, x + xNeighbor];
                            avHeight += neighborHeight;
                            count++;
                        }
                    }
                }
                avHeight /= count;
                heights[z, x] += (avHeight - startHeights[z, x]) * smoothIntensity;
            }
            // set the new height
            terrainData.SetHeights(0, 0, heights);
        }
    }

    public void ResetTerrain()
    {
        terrainData.SetHeights(0, 0, originalHeights);
        streamPaths = new List<List<Vector3>>();
    }
    #endregion

    private void ReApplyTerrain()
    {
        float[,] heights = terrainData.GetHeights(0, 0, heightMapSize, heightMapSize);
        terrainData.SetHeights(0, 0, heights);
    }

    //void OnApplicationQuit()
    //{
    //    ResetTerrain();
    //}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (displayStreams && streamPaths != null && streamPaths.Count > 0)
        {
            foreach (List<Vector3> streamPath in streamPaths)
            {
                foreach (Vector3 point in streamPath)
                {
                    Gizmos.DrawSphere(point, 10);
                    if (point != streamPath[0])
                    {
                        int index = streamPath.IndexOf(point);
                        Vector3 lastWorldPoint = streamPath[index - 1];
                        Gizmos.DrawLine(point, lastWorldPoint);
                    }
                }
            }
        }
    }
}

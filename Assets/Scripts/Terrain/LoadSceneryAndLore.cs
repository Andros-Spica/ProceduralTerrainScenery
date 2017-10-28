using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VectorExtension;

public class LoadSceneryAndLore : MonoBehaviour
{
    public float maxLoadingDistance = 250f;
    public float minDeletingDistance = 500f;
    public bool loadingLimits = false;
    public bool deletingLimits = false;
    
    int patchNeighborhood;

    IntVector2[] patchRelativePositions;

    Transform terrainObjectHolder;

    public TerrainManager terrainManager;
    SceneryProceduralEditor sceneryProceduralEditor;

    List<IntVector2> buildList = new List<IntVector2>();
    List<IntVector2> buildTreeList = new List<IntVector2>();
    
    int timer = 0;

	void Awake() 
	{
        terrainObjectHolder = (new GameObject("Terrain object holder")).transform;

        sceneryProceduralEditor = FindObjectOfType<SceneryProceduralEditor>();
    }

    void Start()
    {
        // calculate neighborhood
        patchNeighborhood = Mathf.Min(
            Mathf.FloorToInt(maxLoadingDistance / terrainManager.patchSize.x),
            Mathf.FloorToInt(maxLoadingDistance / terrainManager.patchSize.y)
            );
        
        FillNeighborhood();
    }

	void FillNeighborhood () 
	{
        List<IntVector2> patchPositionsList = new List<IntVector2>();
        
        // add relative patch positions forming a circle, including the central position
        for (int x = - patchNeighborhood; x <= patchNeighborhood; x++)
        {
            for (int z = - patchNeighborhood; z <= patchNeighborhood; z++)
            {
                if (Vector2.Distance(new Vector2(x * terrainManager.patchSize.x, z * terrainManager.patchSize.y), Vector2.zero) < maxLoadingDistance)
                {
                    patchPositionsList.Add(new IntVector2(x, z));
                }
                    
            }
        }
        // order in radial fashion
        patchPositionsList.Sort((i, j) => (Mathf.Abs(i.x) + Mathf.Abs(i.y)).CompareTo(Mathf.Abs(j.x) + Mathf.Abs(j.y)));

        patchRelativePositions = patchPositionsList.ToArray();
    }

    public int CountPatchesLoaded()
    {
        return terrainObjectHolder.childCount;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (DeletePatchHolder()) //Check to see if a deletion happened
           return;  //and if so return early

        FindPatchHoldersToLoad();
        LoadNextPatchHolder();
    }

    void FindPatchHoldersToLoad()
    {
        // Get the patch position of this game object to generate around
        IntVector2 currentPatchCoord = terrainManager.GetPatchCoordFromPosition(transform.position);
        
        // If there aren't already patch holders to generate
        if (buildList.Count == 0)
        {
            // Cycle through the array of positions
            for (int i = 0; i < patchRelativePositions.Length; i++)
            {
                // translate the player position and array position into patch coordinates
                IntVector2 newPatchCoord = new IntVector2(
                    patchRelativePositions[i].x + currentPatchCoord.x,
                    patchRelativePositions[i].y + currentPatchCoord.y
                    );
                
                // get patch holder name
                string patchName = "Patch (" + newPatchCoord.x + ", " + newPatchCoord.y + ")";

                // Get the patch holder in the defined position (which is may be a child of scenaryHolder)
                Transform patchHolder = terrainObjectHolder.Find(patchName);
                
                // If the patch holder already exists and it's already rendered or in queue to be rendered continue
                if (patchHolder != null)
                    continue;

                // add chunk to lists
                buildList.Add(newPatchCoord);
                return;
            }
        }
    }

    void LoadNextPatchHolder()
    {
        if (buildList.Count != 0)
        {
            for (int i = 0; i < buildList.Count && i < 8; i++)
            {
                BuildPatchHolder(buildList[0]);
                buildList.RemoveAt(0);
            }

            //If patchHolders were built return early
            return;
        }

        //if (updateList.Count != 0) // This applies to elements that must be updated during game play
        //{
        //    Chunk chunk = terrainManager.GetChunk(updateList[0].x, updateList[0].y);
        //    if (chunk != null)
        //        chunk.update = true;
        //    updateList.RemoveAt(0);
        //}
    }

    bool DeletePatchHolder()
    {
        if (timer == 10)
        {
            List<Transform> patchHoldersToDelete = new List<Transform>();
            foreach (Transform patchHolder in terrainObjectHolder)
            {
                //Debug.Log(new Vector2(patchHolder.position.x + (terrainManager.patchSize.x / 2), patchHolder.position.z + (terrainManager.patchSize.y / 2)) + " to " +
                //    new Vector2(transform.position.x, transform.position.z));
                float distance = Vector2.Distance(
                    // from the patch center
                    new Vector2(patchHolder.position.x + (terrainManager.patchSize.x / 2), patchHolder.position.z + (terrainManager.patchSize.y / 2)),
                    // to the (player) current position
                    new Vector2(transform.position.x, transform.position.z));
                //Debug.Log(distance + " > " + minDeletingDistance);
                if (distance > minDeletingDistance)
                    patchHoldersToDelete.Add(patchHolder);
            }

            foreach (Transform patchHolder in patchHoldersToDelete)
            {
                Destroy(patchHolder.gameObject);
            }
                
            timer = 0;
            return true;
        }

        timer++;
        return false;
    }

    void BuildPatchHolder(IntVector2 patchCoord)
    {
        // get patch and patch holder name
        string patchName = "Patch (" + patchCoord.x + ", " + patchCoord.y + ")";

        // create patch holder
        GameObject patchHolderGO = new GameObject(patchName);
        patchHolderGO.transform.parent = terrainObjectHolder;
        patchHolderGO.transform.position = terrainManager.GetPositionFromPatchCoordinates(patchCoord);
        Transform patchHolder = patchHolderGO.transform;

        // create a scenery holder
        Transform sceneryHolder = new GameObject("Scenery").transform;
        sceneryHolder.position = patchHolder.position;
        sceneryHolder.parent = patchHolder;

        // create a settlement holder
        Transform settlementHolder = new GameObject("Settlement").transform;
        settlementHolder.position = patchHolder.position;
        settlementHolder.parent = patchHolder;

        // add vegetation
        sceneryProceduralEditor.UpdateVegetationAtPatch(patchCoord, sceneryHolder);
        //if (!buildTreeList.Contains(patchCoord)) // this version applies whenever tree instances are added to the terrain object
        //{
        //    sceneryProceduralEditor.UpdateVegetationAtPatch(patchCoord, patch, patchHolder);
        //    buildTreeList.Add(patchCoord);
        //}
        // add stones
        sceneryProceduralEditor.UpdateStonesAtPatch(patchCoord, sceneryHolder);
        
    }

    
    public Vector3 GetValidRandomPositionInPatch(IntVector2 patchCoord, float radius, System.Random RNG)
    {
        Vector3 validPos = terrainManager.GetRandomPointInPatchSurface(patchCoord, RNG);
        // check for overlap
        if (Physics.CheckSphere(validPos, radius, 11)) // detect terrain objects
        {
            // sample a position again
            validPos = terrainManager.GetRandomPointInPatchSurface(patchCoord, RNG);
            //return GetValidRandomPositionInPatch(patchCoord, radius, RNG); // ALERT! POSSIBLE STACK OVERFLOW
        }
        return validPos;
    }

    public void ClearSceneryInArea(Rect area, IntVector2 patch)
    {
        Transform sceneryHolder = terrainObjectHolder.Find("Patch (" + patch.x + ", " + patch.y + ")").Find("Scenery");
        if (sceneryHolder)
        {
            List<Transform> objectsToDelete = new List<Transform>();
            foreach (Transform sceneryObject in sceneryHolder)
            {
                if (area.Contains(sceneryObject.position))
                    objectsToDelete.Add(sceneryObject);
            }
            foreach (Transform obj in objectsToDelete)
            {
                Destroy(obj);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (loadingLimits)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxLoadingDistance);
        }
        if (deletingLimits)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, minDeletingDistance);
        }
    }
}

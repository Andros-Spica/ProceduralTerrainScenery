using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitPlayerPosition : MonoBehaviour {

    public float playerPosLimitMargins = 0.1f;
    public bool displayLimits = false;

    Vector3 initialPosition;
    float maxDistanceToInitialPosition;
   
    void Start ()
    {
        TerrainManager terrainManager = FindObjectOfType<TerrainManager>();
        initialPosition = terrainManager.centralPosition;
        maxDistanceToInitialPosition = terrainManager.GetDistanceFromCenterToSide() * (1 - playerPosLimitMargins);
    }

	public bool IsOutsideLimit (float x, float z)
    {
        if (Vector3.Distance(new Vector3(x, initialPosition.y, z), initialPosition) > maxDistanceToInitialPosition)
        {
            return true;
        }
        return false;
	}

    void OnDrawGizmos()
    {
        if (displayLimits)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(initialPosition, maxDistanceToInitialPosition);
        }
    }
}

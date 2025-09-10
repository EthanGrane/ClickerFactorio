using System;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerHarvest : MonoBehaviour
{
    public float harvestDamage = 1;
    public float harvestDistance = 5;
    public LayerMask ResourceLayerMask;

    ResourceMaterial lastResourceMaterial = null;

    public void HandleHarvest()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, harvestDistance))
            {
                if (hit.transform.GetComponent<ResourceMaterial>())
                {
                    hit.transform.GetComponent<ResourceMaterial>().HarvestMaterial(Mathf.FloorToInt(harvestDamage));

                    lastResourceMaterial = hit.transform.GetComponent<ResourceMaterial>();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (lastResourceMaterial)
            {
                lastResourceMaterial.BounceObject();

                lastResourceMaterial = null;
            }
        }

    }
}

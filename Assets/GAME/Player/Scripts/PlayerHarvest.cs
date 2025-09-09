using System;
using Mono.Cecil;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerHarvest : MonoBehaviour
{
    public float harvestDamage = 1;
    public float harvestDistance = 5;
    public LayerMask ResourceLayerMask;
    
    public AudioClip harvestSound;
    public AudioClip harvestSoundUp;

    ResourceMaterial lastResourceMaterial = null;

    public void HandleHarvest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AudioManager.Instance.PlayOneShot2D(harvestSound).Volume(0.1f).PitchVariation(0.05f).Play();

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
            AudioManager.Instance.PlayOneShot2D(harvestSoundUp).Volume(0.1f).PitchVariation(0.05f).Play();
            if (lastResourceMaterial)
            {
                lastResourceMaterial.BounceObject();

                lastResourceMaterial = null;
            }
        }

    }
}

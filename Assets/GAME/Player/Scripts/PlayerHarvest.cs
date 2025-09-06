using System;
using Mono.Cecil;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerHarvest : MonoBehaviour
{
    public float harvestDamage = 1;
    public float harvestDistance = 5;
    public LayerMask ResourceLayerMask;
    
    private AudioSource harvestAudioSource;
    public AudioClip harvestSound;
    public AudioClip harvestSoundUp;

    ResourceMaterial lastResourceMaterial = null;
    
    private void Awake()
    {
        harvestAudioSource = gameObject.AddComponent<AudioSource>();
    }

    public void HandleHarvest()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, harvestDistance))
            {
                if (hit.transform.GetComponent<ResourceMaterial>())
                {
                    hit.transform.GetComponent<ResourceMaterial>().HarvestMaterial(Mathf.FloorToInt(harvestDamage));
                    
                    harvestAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    harvestAudioSource.PlayOneShot(harvestSound);
                    lastResourceMaterial = hit.transform.GetComponent<ResourceMaterial>();
                }
            }
        }

        if (lastResourceMaterial)
        {
            if (Input.GetMouseButtonUp(0))
            {
                lastResourceMaterial.BounceObject();

                harvestAudioSource.PlayOneShot(harvestSoundUp);
                lastResourceMaterial = null;
            }
        }

    }
}

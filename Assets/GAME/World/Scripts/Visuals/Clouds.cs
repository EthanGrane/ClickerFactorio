using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Clouds : MonoBehaviour
{
    public float cloudsYStart = 100;
    public int cloudsResolution = 16;
    public int raysResolution = 32;
    public float raysSpacing = 2.5f;
    
    public GameObject cloudPrefab;
    public Material cloudMaterial;
    
    GameObject[] clouds;

    private void Start()
    {
        StartCoroutine(DensityRoutine());
    }

    private IEnumerator DensityRoutine()
    {
        while (true)
        {
            float wait1 = Random.Range(30, 60);
            // Espera 3 minutos
            yield return new WaitForSeconds(wait1);

            float currentDensity = cloudMaterial.GetFloat("_Density");
            float targetDensity = Random.Range(0.25f,0.75f);
            
            float elapsed = 0f;
            float transitionTime = Random.Range(30, 60);
            while (elapsed < transitionTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / transitionTime);
                float density = Mathf.Lerp(currentDensity, targetDensity, t);
                cloudMaterial.SetFloat("_Density", density);
                yield return null;
            }
            
        }
    }
    
    [ContextMenu("SpawnClouds")]
    public void Generate()
    {
        cloudMaterial.SetFloat("_StartY", cloudsYStart - 0.5f);
        cloudMaterial.SetFloat("_Count", cloudsResolution);
        
        for (int i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        clouds = new GameObject[cloudsResolution + raysResolution];
        for (int i = 0; i < cloudsResolution; i++)
        {
            clouds[i] = Instantiate(cloudPrefab, transform);
            clouds[i].transform.position = Vector3.up * (i + cloudsYStart);
            clouds[i].name = "Cloud" + i.ToString();
        }

        for (int i = 0; i < raysResolution; i++)
        {
            clouds[i + cloudsResolution] = Instantiate(cloudPrefab, transform);
            clouds[i + cloudsResolution].transform.position = Vector3.up * (cloudsYStart - (i * raysSpacing));
            clouds[i + cloudsResolution].name = "Ray" + i.ToString();
        }
    }
    
}

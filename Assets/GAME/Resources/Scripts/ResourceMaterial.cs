using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class ResourceMaterial : MonoBehaviour
{
    public int resourceHealth = 100;
    public ResourceItem resourceResourceItem;
    public string hitSound;
    public AudioMixerGroup audioMixerGroup;
    [Space]
    public ParticleSystem hitParticleSystem;
    public ParticleSystem breakParticleSystem;
    
    public Action onResourceDestroyed;
    
    Material material;
    Sequence bounceTweener = null;
    Sequence shineTweener = null;
    bool canHarvest = true;
    
    private void Awake()
    {
        material = GetComponent<MeshRenderer>().materials[1];
        material.SetTexture("_Texture2D",GetComponent<MeshRenderer>().materials[0].GetTexture("_BaseMap"));
    }

    public void HarvestMaterial(int damage)
    {
        if(canHarvest == false)
            return;
        
        resourceHealth -= damage;

        if (resourceHealth <= 0)
        {
            onResourceDestroyed?.Invoke();
            canHarvest = false;
            
            bounceTweener.Kill();
            breakParticleSystem.Play();
            breakParticleSystem.transform.parent = null;
            transform.DOShakeScale(0.1f, Vector3.up).onComplete += () =>
            {
                transform.DOScale(0, 0.1f).onComplete += () =>
                {
                    Destroy(hitParticleSystem, 5f);
                    WorldManager.Instance.DestroyCellObject(transform.position);
                };
            };
        }
        
        AudioManager.Instance.PlayOneShot3D(hitSound,transform.position)
            .Volume(1f)
            .PitchVariation(0.25f)
            .MaxDistance(100f)
            .AudioMixerGroup(audioMixerGroup)
            .Play();
        
        if (hitParticleSystem != null)
        {
            hitParticleSystem.Play();
        }
        
        if (shineTweener != null)
        {
            shineTweener.Kill();
            shineTweener = null;
        }

        shineTweener = DOTween.Sequence()
            .Append(material.DOFloat(1, "_Shine", 0.05f).SetEase(Ease.InQuad))
            .Append(material.DOFloat(0, "_Shine", 0.05f).SetEase(Ease.Linear))
            .SetAutoKill(false)
            .OnComplete(() => shineTweener = null);

        shineTweener.Restart();
        shineTweener.Restart(); 
    }

    public void BounceObject()
    {
        if(!canHarvest)
            return;
        
        if (bounceTweener == null)
        {
            bounceTweener = DOTween.Sequence();

            // Shake de escala
            bounceTweener.Join(transform.DOShakeScale(
                0.5f,
                new Vector3(0.25f, 1, 0.25f) * 0.05f
            ).SetEase(Ease.InOutQuad));

            bounceTweener.SetAutoKill(false);
            bounceTweener.Pause();
            bounceTweener.onComplete = () => { bounceTweener = null; };
        }

        bounceTweener.Restart(); 
    }
}

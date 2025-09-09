using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ResourceMaterial : MonoBehaviour
{
    public int resourceHealth = 100;
    public int resourceValue = 1;
    public string hitSound;
    
    ParticleSystem ps;
    Material material;
    Sequence bounceTweener = null;
    Sequence shineTweener = null;
    
    private void Awake()
    {
        material = GetComponent<MeshRenderer>().materials[1];
        material.SetTexture("_Texture2D",GetComponent<MeshRenderer>().materials[0].GetTexture("_BaseMap"));
        
        ps = GetComponentInChildren<ParticleSystem>();
    }

    public void HarvestMaterial(int damage)
    {
        resourceHealth -= damage;
        GameManager.Instance.AddMoney(resourceValue * damage);

        AudioManager.Instance.PlayOneShot3D(hitSound,transform.position)
            .Volume(1f)
            .PitchVariation(0.25f)
            .MaxDistance(100f)
            .Play();
        
        if (ps)
        {
            ps.Play();
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

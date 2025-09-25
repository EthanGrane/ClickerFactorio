using DG.Tweening;
using UnityEngine;

public class RectTransformAnimation : MonoBehaviour
{
    public float animationSpeed = 1;
    [Space]
    public float rotationVariation = 0;
    public float rotationMultiplier = 0;
    [Space]
    public float positionVariation = 0;

    Vector3 iPos;
    Quaternion iRot;

    private float seed = 0;
    
    private void Awake()
    {
        seed = Random.Range(0.00f,1.00f);
        
        iPos = transform.position;
        iRot = transform.rotation;
    }

    public void Update()
    {
        float rotation = Mathf.Sin(seed + Time.unscaledTime * rotationMultiplier) * rotationVariation;
        transform.rotation = Quaternion.Euler(0, 0, iRot.eulerAngles.z + rotation);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float targetY = iPos.y + Random.Range(0f, positionVariation);

            Sequence jumpSeq = DOTween.Sequence();
            jumpSeq.Append(transform.DOMoveY(targetY, 0.8f).SetEase(Ease.Linear))
                .Append(transform.DOMoveY(iPos.y, 0.4f).SetEase(Ease.OutBounce));
        }

    }
}

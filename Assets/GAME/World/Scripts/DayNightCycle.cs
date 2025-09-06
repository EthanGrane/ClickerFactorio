using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Transform sunTransform;
    public float daySpeed = 1f;
    public float nightSpeed = 3f;

    public float sunsetAngle = 180f;   // Ángulo donde empieza la puesta
    public float nightAngle = 270f;    // Ángulo donde empieza la noche

    public float minIntensity = 0.2f;
    public float maxIntensity = 1.5f;

    private void Update()
    {
        float sunAngle = sunTransform.eulerAngles.x;

        // Determinamos si es día o noche según los thresholds
        float speed = (sunAngle < sunsetAngle || sunAngle > nightAngle) ? daySpeed : nightSpeed;

        // Rotamos el sol
        sunTransform.Rotate(Vector3.right, Time.deltaTime * speed);

        // Ajustamos intensidad: máximo al mediodía, mínimo en la noche
        float intensity = Mathf.Clamp01(Mathf.Cos((sunTransform.eulerAngles.x - 90f) * Mathf.Deg2Rad));
        intensity = Mathf.Lerp(minIntensity, maxIntensity, intensity);
        RenderSettings.ambientIntensity = intensity;
    }
}
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public Vector3 offset = new Vector3(0f, -0.1f, 0.5f); // posición base
    public float springStrength = 50f;
    public float damping = 8f;
    public float maxSway = 0.25f; // máximo movimiento por MouseX/MouseY
    public float moveSway = 0.1f; // máximo movimiento por WASD/Space

    [Header("Sound")]
    public AudioClip mouseDownClip;
    public AudioClip mouseUpClip;
    public AudioMixerGroup audioMixerGroup;
    
    [Header("Click Push")]
    public float pushStrength = 0.3f;

    private Vector3 velocity;
    private Transform cam;
    Sequence scaleTween;

    private void Awake()
    {
        cam = Camera.main.transform;
        transform.localPosition = offset;
        transform.localRotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 sway = new Vector3(
            Mathf.Clamp(-mouseX * maxSway, -maxSway, maxSway),
            Mathf.Clamp(-mouseY * maxSway, -maxSway, maxSway),
            0f
        );

        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S
        float moveY = Input.GetKey(KeyCode.Space) ? 1f : 0f; // salto

        Vector3 moveSwayOffset = new Vector3(
            -moveX * moveSway,       // lateral inverso al movimiento
            -moveY * moveSway * 0.5f, // arriba para salto, abajo ligero al caminar
            -moveZ * moveSway
        );

        // --- TARGET FINAL ---
        Vector3 targetLocalPos = offset + sway + moveSwayOffset;

        // --- SPRING PHYSICS ---
        Vector3 displacement = targetLocalPos - transform.localPosition;
        Vector3 springForce = displacement * springStrength;
        Vector3 dampingForce = -velocity * damping;
        Vector3 acceleration = springForce + dampingForce;

        velocity += acceleration * Time.deltaTime;
        transform.localPosition += velocity * Time.deltaTime;

        // --- CLICK PUSH ---
        if (Input.GetMouseButtonDown(0))
        {
            AudioManager.Instance.PlayOneShot2D(mouseDownClip).Volume(0.1f).PitchVariation(0.05f).AudioMixerGroup(audioMixerGroup).Play();

            velocity += transform.InverseTransformDirection(cam.forward) * pushStrength;
            if (scaleTween == null)
            {
                scaleTween = DOTween.Sequence();

                // Shake de escala
                scaleTween.Join(transform.DOShakeScale(
                    0.25f,
                    new Vector3(0.25f, 1, 0.25f) * 0.1f
                ).SetEase(Ease.InOutQuad));

                scaleTween.SetAutoKill(false);
                scaleTween.Pause();
                scaleTween.onComplete = () => { scaleTween = null; };
            }
            scaleTween.Restart();
        }
        
        if(Input.GetMouseButtonUp(0))
            AudioManager.Instance.PlayOneShot2D(mouseUpClip).Volume(0.1f).PitchVariation(0.05f).AudioMixerGroup(audioMixerGroup).Play();

        // --- ROTACIÓN SUAVE ---
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
    }
}

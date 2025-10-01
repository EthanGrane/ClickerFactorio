using System;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashDecay = 5f; // qué tan rápido pierde inercia

    [Header("Camara")]
    public float mouseSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Ground Check")]
    public Transform groundCheck;      
    public float groundRadius = 0.3f;  
    public LayerMask groundMask;       

    CharacterController controller;
    Vector3 verticalVelocity;
    Vector3 dashVelocity;
    float cameraPitch = 0f;
    bool isGrounded;
    bool doubleJump = false;
    bool isDashing = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleMouseLook();
        HandleCursor();
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        if (isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; 
            dashVelocity = Vector3.zero;
            doubleJump = false;
            isDashing = false;
        }
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * inputX + transform.forward * inputZ;
        move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);

        // salto normal
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // doble salto con dash
        if (Input.GetButtonDown("Jump") && !doubleJump && !isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            dashVelocity = cameraForwardXZ() * dashForce;
            isDashing = true;
            doubleJump = true;
        }

        // si estamos en dash → redirige la inercia hacia el forward actual de la cámara
        if (isDashing)
        {
            // desacelera
            dashVelocity = Vector3.Lerp(dashVelocity, Vector3.zero, dashDecay * Time.deltaTime);

            // reorienta la inercia al nuevo forward (conservando magnitud)
            if (dashVelocity.magnitude > 0.1f)
            {
                dashVelocity = cameraForwardXZ() * dashVelocity.magnitude;
            }
            else
            {
                dashVelocity = Vector3.zero;
                isDashing = false;
            }
        }

        // gravedad
        verticalVelocity.y += gravity * Time.deltaTime;

        // movimiento final
        controller.Move((verticalVelocity + dashVelocity) * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        cameraTransform.localEulerAngles = Vector3.right * cameraPitch;
    }

    private Vector3 cameraForwardXZ()
    {
        Vector3 fwd = cameraTransform.forward;
        fwd.y = 0;
        return fwd.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}

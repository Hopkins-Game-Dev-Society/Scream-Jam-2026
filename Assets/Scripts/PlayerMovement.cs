using System.Collections;
using System.Collections.Generic;

//using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput; 
    private Vector2 aimDirection = Vector2.down;
    private Vector2 lookScreenPos;
    private bool hasLookInput;

    private bool isDashing;
    private Vector2 dashVelocity;

    public Transform Aim;

    public Vector2 MoveInput => moveInput;
    public Vector2 AimDirection => aimDirection;
    public float MoveSpeedBonus { get; set; }

    public float TemporaryMoveSpeedBonus { get; set; }

    public bool IsDashing => isDashing;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashVelocity;
            return;
        }

        rb.linearVelocity =
            moveInput * Mathf.Max(moveSpeed + MoveSpeedBonus
            + TemporaryMoveSpeedBonus,0f);
    }

    public void StartDash(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        isDashing = true;
        dashVelocity = direction.normalized * speed;
        rb.linearVelocity = dashVelocity;
    }

    public void EndDash()
    {
        isDashing = false;
    }


    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        if (hasLookInput)
            AimAt(lookScreenPos);
    }

    /*
    public void Look(InputAction.CallbackContext context)
    {
        lookScreenPos = context.ReadValue<Vector2>();
        hasLookInput = true;
        AimAt(lookScreenPos);
    } */

    private void Look(UnityEngine.InputSystem.InputAction.CallbackContext context)
{
    // Ignore cancellation callbacks when input is being disabled.
    if (context.phase !=
            UnityEngine.InputSystem.InputActionPhase.Performed)
        {
            return;
        }

        if (!isActiveAndEnabled)
        {
            return;
        }

        AimAt(context.ReadValue<Vector2>());
    }

    private void AimAt(Vector2 mouseScreenPos)
    {

        if (Aim == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }
        // Aiming logic
        Vector3 mousePos3 = mouseScreenPos; //convert to vector3 pos to do world pos calc correctly
        mousePos3.z = -Camera.main.transform.position.z; //adds camera offset
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos3); //converts mouse screen pos to world pos
        Vector3 targetWorldPos = worldPos - transform.position;
        if (((Vector2)targetWorldPos).sqrMagnitude > 0.0001f)
            aimDirection = ((Vector2)targetWorldPos).normalized;
        Vector3 rotatedWorldPos = Quaternion.Euler(0,0,180) * targetWorldPos; //unsure the meaning for this in regards to the aiming logic
        Aim.rotation = Quaternion.LookRotation(Vector3.forward, rotatedWorldPos);
    }

}

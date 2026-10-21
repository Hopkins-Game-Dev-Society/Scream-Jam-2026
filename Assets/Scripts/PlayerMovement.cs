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

    public Transform Aim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * Mathf.Max(moveSpeed + MoveSpeedBonus, 0f);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        // Aiming logic
        Vector2 mouseScreenPos = context.ReadValue<Vector2>();
        Vector3 mousePos3 = mouseScreenPos; //convert to vector3 pos to do world pos calc correctly
        mousePos3.z = -Camera.main.transform.position.z; //adds camera offset
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos3); //converts mouse screen pos to world pos
        Vector3 targetWorldPos = worldPos - transform.position;
        if (((Vector2)targetWorldPos).sqrMagnitude > 0.0001f)
            aimDirection = ((Vector2)targetWorldPos).normalized;
        print(targetWorldPos);
        Vector3 rotatedWorldPos = Quaternion.Euler(0,0,180) * targetWorldPos; //unsure the meaning for this in regards to the aiming logic
        Aim.rotation = Quaternion.LookRotation(Vector3.forward, rotatedWorldPos);
    }
    public Vector2 MoveInput => moveInput;
    public Vector2 AimDirection => aimDirection;
    public float MoveSpeedBonus { get; set; } // from limb stats, set by PlayerController
}

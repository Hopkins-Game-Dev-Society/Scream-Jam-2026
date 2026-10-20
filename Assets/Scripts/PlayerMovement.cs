using System.Collections;
using System.Collections.Generic;

//using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.InputSystem;


/*
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput; 

    public Transform Aim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
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
        print(targetWorldPos);
        Vector3 rotatedWorldPos = Quaternion.Euler(0,0,180) * targetWorldPos; //unsure the meaning for this in regards to the aiming logic
        Aim.rotation = Quaternion.LookRotation(Vector3.forward, rotatedWorldPos);
    }
    public Vector2 MoveInput => moveInput;
}
*/


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform aim;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 aimDirection = Vector2.down;

    public Vector2 MoveInput => moveInput;
    public Vector2 AimDirection => aimDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = Vector2.ClampMagnitude(moveInput, 1f);
        rb.linearVelocity = direction * moveSpeed;
    }

    private void Update()
    {
        UpdateAim();
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Look(InputAction.CallbackContext context)
    {
        // The mouse position is processed in Update.
        // This callback is retained for the Input Action.
    }

    private void UpdateAim()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z - transform.position.z
        );

        Vector3 mouseScreenPosition3D = new Vector3(
            mouseScreenPosition.x,
            mouseScreenPosition.y,
            cameraDistance
        );

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition3D);

        Vector2 direction =
            (Vector2)(mouseWorldPosition - transform.position);

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        aimDirection = direction.normalized;

        if (aim != null)
        {
            aim.right = aimDirection;
        }
    }
}

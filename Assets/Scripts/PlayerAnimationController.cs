using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    private enum FacingDirection
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform playerTransform;

    [Header("Settings")]
    [SerializeField] private float movementThreshold = 0.01f;

    private FacingDirection facing = FacingDirection.Down;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int FacingHash =
        Animator.StringToHash("FacingDirection");

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerTransform == null)
        {
            playerTransform = transform;
        }
    }


    //Currently having a problem with the facing direction (is moving too sensitive right now)
    private void Update()
    {
        UpdateFacingDirection();

        Vector2 input = playerMovement.MoveInput;

        bool isMoving = input.sqrMagnitude >
            movementThreshold * movementThreshold;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetInteger(FacingHash, (int)facing);
    }

    private void UpdateFacingDirection()
    {
        if (Mouse.current == null || mainCamera == null)
        {
            return;
        }

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z -
            playerTransform.position.z);

        Vector3 mouseScreenPosition = new Vector3(
            mousePosition.x,
            mousePosition.y,
            cameraDistance
        );

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        Vector2 direction =
            (Vector2)(mouseWorldPosition - playerTransform.position);

        // Avoid changing direction when the cursor is
        // exactly on top of the player.
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // Horizontal wins when both axes are equally distant.
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            facing = direction.x < 0
                ? FacingDirection.Left
                : FacingDirection.Right;
        }
        else
        {
            facing = direction.y < 0
                ? FacingDirection.Down
                : FacingDirection.Up;
        }
    }
}

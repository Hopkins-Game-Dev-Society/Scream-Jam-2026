using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private enum FacingDirection
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }

    //Going to need to use later for specifying different attack animations depending on current layout for player
    private enum AttackAnimation
    {
        Lion = 0,
        Tin = 1,
        Scarecrow = 2
    }

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float movementThreshold = 0.01f;

    private FacingDirection facing = FacingDirection.Down;

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int FacingDirectionHash =
        Animator.StringToHash("FacingDirection");

    private void Update()
    {
        if (playerMovement == null || animator == null)
        {
            return;
        }

        UpdateFacingDirection(playerMovement.AimDirection);

        bool isMoving =
            playerMovement.MoveInput.sqrMagnitude >
            movementThreshold * movementThreshold;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetInteger(FacingDirectionHash, (int)facing);
    }

    private void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // Horizontal takes priority when the distances are equal.
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            facing = direction.x < 0f
                ? FacingDirection.Left
                : FacingDirection.Right;
        }
        else
        {
            facing = direction.y < 0f
                ? FacingDirection.Down
                : FacingDirection.Up;
        }
    }
}

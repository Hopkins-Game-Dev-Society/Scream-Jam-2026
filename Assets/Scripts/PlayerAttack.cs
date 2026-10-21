using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Body), typeof(PlayerMovement))]
public class PlayerAttack : MonoBehaviour
{
    private Body body;
    private PlayerMovement movement;

    private void Awake()
    {
        body = GetComponent<Body>();
        movement = GetComponent<PlayerMovement>();
    }

    public void MainAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Attack(BodyPart.LeftArm);
        }
    }

    public void SecondaryAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Attack(BodyPart.RightArm);
        }
    }

    private void Attack(BodyPart arm)
    {
        if (body.IsAttacking)
        {
            return;
        }

        MeleeAttack attack = body.GetAttack(arm);

        if (attack != null)
        {
            attack.TryAttack(movement.AimDirection);
        }
    }
}

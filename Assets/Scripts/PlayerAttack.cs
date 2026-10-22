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

//Needs to be updated so that if the player is attacking it checks what
//body parts they have attached and does the seprate attacks (probably not awful to do) 
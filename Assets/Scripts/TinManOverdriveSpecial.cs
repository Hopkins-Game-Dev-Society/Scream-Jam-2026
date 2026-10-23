using System.Collections;
using UnityEngine;

public class TinManOverdriveSpecial : PlayerSpecial
{
    [Header("Overdrive Settings")]
    [SerializeField] private float buffDuration = 4f;
    [SerializeField] private float movementSpeedBonus = 3f;
    [SerializeField] private float maximumAttackSpeedMultiplier = 2f;
    [SerializeField] private float cooldownDuration = 8f;
    [SerializeField] private Color overdriveColor = Color.red;

    private PlayerMovement movement;
    private Body body;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private LimbAttack[] attacks;
    private Coroutine overdriveRoutine;

    public override float CooldownDuration => cooldownDuration;

    public override void Activate(Vector2 aimDirection)
    {
        // Resolve references at activation time.
        movement = GetComponentInParent<PlayerMovement>();
        body = GetComponentInParent<Body>();

        PlayerController controller =
            GetComponentInParent<PlayerController>();

        if (movement == null || body == null || controller == null)
        {
            Debug.LogWarning(
                "TinManOverdriveSpecial could not find the player components.",
                this);
            return;
        }

        // Refresh these arrays so starting limbs and newly equipped
        // limbs are both included.
        spriteRenderers =
            controller.GetComponentsInChildren<SpriteRenderer>(true);

        attacks = controller.GetComponentsInChildren<LimbAttack>(true);

        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        if (overdriveRoutine != null)
        {
            StopCoroutine(overdriveRoutine);
            RestoreOverdrive();
        }

        overdriveRoutine = StartCoroutine(OverdriveRoutine());
    }

    private IEnumerator OverdriveRoutine()
    {
        float elapsed = 0f;

        while (elapsed < buffDuration)
        {
            float strength = 1f - Mathf.Clamp01(elapsed / buffDuration);

            // Apply the movement bonus.
            movement.TemporaryMoveSpeedBonus =
                movementSpeedBonus * strength;

            // Apply the attack speed bonus.
            float attackMultiplier = Mathf.Lerp(
                1f,
                maximumAttackSpeedMultiplier,
                strength);

            foreach (LimbAttack attack in attacks)
            {
                if (attack is MeleeAttack meleeAttack)
                {
                    meleeAttack.AttackSpeedMultiplier =
                        attackMultiplier;
                }
                else if (attack is ProjectileAttack projectileAttack)
                {
                    projectileAttack.AttackSpeedMultiplier =
                        attackMultiplier;
                }
            }

            // Fade the character's color back to its original color.
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = Color.Lerp(
                        originalColors[i],
                        overdriveColor,
                        strength);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        RestoreOverdrive();
        overdriveRoutine = null;
    }

    private void RestoreOverdrive()
    {
        if (movement != null)
        {
            movement.TemporaryMoveSpeedBonus = 0f;
        }

        if (attacks != null)
        {
            foreach (LimbAttack attack in attacks)
            {
                if (attack is MeleeAttack meleeAttack)
                {
                    meleeAttack.AttackSpeedMultiplier = 1f;
                }
                else if (attack is ProjectileAttack projectileAttack)
                {
                    projectileAttack.AttackSpeedMultiplier = 1f;
                }
            }
        }

        if (spriteRenderers != null && originalColors != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = originalColors[i];
                }
            }
        }
    }

    private void OnDisable()
    {
        if (overdriveRoutine != null)
        {
            StopCoroutine(overdriveRoutine);
            overdriveRoutine = null;
        }

        RestoreOverdrive();
    }
}

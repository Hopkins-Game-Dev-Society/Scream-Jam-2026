using UnityEngine;

public class ScarecrowDecoySpecial : PlayerSpecial
{
    [Header("Decoy Settings")]
    [SerializeField] private DecoyTarget decoyPrefab;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private float cooldownDuration = 6f;

    private PlayerController playerController;
    private PlayerMovement playerMovement;

    public override float CooldownDuration => cooldownDuration;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

        if (playerController != null)
        {
            playerMovement = playerController.Movement;
        }
    }

    public override void Activate(Vector2 aimDirection)
    {
        if (decoyPrefab == null || playerController == null)
        {
            Debug.LogWarning(
                "ScarecrowDecoySpecial is missing its decoy prefab or PlayerController.",
                this);
            return;
        }

        // Prefer the current mouse-aim direction.
        Vector2 direction = playerMovement != null
            ? playerMovement.AimDirection
            : aimDirection;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.down;
        }

        direction.Normalize();

        Vector2 spawnPosition =
            (Vector2)playerController.transform.position
            + direction * spawnDistance;

        Instantiate(
            decoyPrefab,
            spawnPosition,
            playerController.transform.rotation);
    }
}

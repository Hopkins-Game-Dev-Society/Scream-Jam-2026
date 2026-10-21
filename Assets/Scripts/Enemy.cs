using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class Enemy : MonoBehaviour
{
    private static readonly BodyPart[] Arms = { BodyPart.RightArm, BodyPart.LeftArm };

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D player;

    private Rigidbody2D rb;
    private Body body;
    private Health health;
    private Health playerHealth;
    private readonly List<Hurtbox> playerHurtboxes = new List<Hurtbox>();
    private readonly List<Collider2D> hurtboxColliders = new List<Collider2D>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Body>();
        health = GetComponent<Health>();

        if (player != null)
            playerHealth = player.GetComponent<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        // Dead enemies stay as a corpse to loot (unless Health has Destroy On Death ticked) but stop
        // taking part in physics so they don't block the player from standing over them.
        if (health.IsDead)
        {
            if (rb.simulated)
                rb.simulated = false;
            return;
        }

        if (player == null || (playerHealth != null && playerHealth.IsDead))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Measure to the nearest point of the player's body
        //had issues if the enemy was small when i had it based on the center of the player b/c they couldnt reach
        Vector2 toPlayer = NearestPlayerPoint() - (Vector2)transform.position;

        if (toPlayer.sqrMagnitude < 0.0001f)
            toPlayer = player.position - (Vector2)transform.position;

        body.FaceDirection(toPlayer);

        // Stand still while swinging, or while the player is within reach of an arm.
        if (body.IsAttacking || AttackIfInReach(toPlayer))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = Mathf.Max(moveSpeed + body.GetStatBonus(Stat.MoveSpeed), 0f);
        rb.linearVelocity = toPlayer.normalized * speed;
    }

    // Closest point on any of the player's hurtboxes, or their center if they have none.
    private Vector2 NearestPlayerPoint()
    {
        Vector2 position = transform.position;
        Vector2 nearest = player.position;
        float nearestDistance = float.MaxValue;

        player.GetComponentsInChildren(playerHurtboxes);

        foreach (Hurtbox hurtbox in playerHurtboxes)
        {
            hurtbox.GetComponents(hurtboxColliders);

            foreach (Collider2D hurtboxCollider in hurtboxColliders)
            {
                Vector2 point = hurtboxCollider.ClosestPoint(position);
                float distance = (point - position).sqrMagnitude;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = point;
                }
            }
        }

        return nearest;
    }

    // Swings with the first arm that can reach the player and is off cooldown.
    // Returns true if the player is within reach of any arm even if both are cooling down.
    private bool AttackIfInReach(Vector2 toPlayer)
    {
        bool inReach = false;

        foreach (BodyPart arm in Arms)
        {
            MeleeAttack attack = body.GetAttack(arm);

            if (attack == null || toPlayer.magnitude > attack.Range)
                continue;

            inReach = true;

            if (attack.TryAttack(toPlayer))
                break;
        }

        return inReach;
    }
}

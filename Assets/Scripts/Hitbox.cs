using System.Collections.Generic;
using UnityEngine;

// Damages Hurtboxes on another team that overlap this collider while the hitbox is active.
// Only colliders with a Hurtbox can be hit 
[RequireComponent(typeof(Collider2D))]
public class Hitbox : MonoBehaviour
{
    [SerializeField] private float damage = 1f;

    [Tooltip("Active from the start and never turned off, e.g. enemy contact damage. Leave off for hitboxes driven by a MeleeAttack.")]
    [SerializeField] private bool alwaysActive = false;

    [Tooltip("Seconds before the same target can be hit again while active. 0 = once per activation (one hit per swing).")]
    [SerializeField] private float rehitInterval = 0f;

    private Collider2D hitCollider;
    private Health owner;
    private bool isActive;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();

    // The body part to hit on each target this physics step so a swing that overlaps
    // several limbs still only counts as one hit.
    private readonly Dictionary<Health, Hurtbox> hitsThisStep = new Dictionary<Health, Hurtbox>();

    // Tracks who has been hit this activation.
    private readonly Dictionary<Health, float> lastHitTimes = new Dictionary<Health, float>();

    // Added to every hit for things like limb bonuses
    public float DamageBonus { get; set; }

    // Runs when the component is added in the editor
    private void Reset()
    {
        if (TryGetComponent(out Collider2D col))
        {
            col.isTrigger = true;
        }
    }

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        owner = GetComponentInParent<Health>();

        if (alwaysActive)
        {
            Activate();
        }
    }

    public void Activate()
    {
        Physics2D.SyncTransforms();

        lastHitTimes.Clear();
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    private void FixedUpdate()
    {
        if (!isActive || (owner != null && owner.IsDead))
        {
            return;
        }

        int count = hitCollider.Overlap(ContactFilter2D.noFilter, overlaps);
        hitsThisStep.Clear();

        for (int i = 0; i < count; i++)
        {
            if (!overlaps[i].TryGetComponent(out Hurtbox hurtbox) || !CanHit(hurtbox.Health))
            {
                continue;
            }

            // When several limbs overlap hit the one that takes the most damage (e.g. a weak head).
            if (!hitsThisStep.TryGetValue(hurtbox.Health, out Hurtbox best) ||
                hurtbox.DamageMultiplier > best.DamageMultiplier)
            {
                hitsThisStep[hurtbox.Health] = hurtbox;
            }
        }

        Vector2 origin = owner != null ? owner.transform.position : transform.position;

        foreach (KeyValuePair<Health, Hurtbox> hit in hitsThisStep)
        {
            Vector2 direction = ((Vector2)hit.Value.transform.position - origin).normalized;
            float amount = Mathf.Max(damage + DamageBonus, 0f) * hit.Value.DamageMultiplier;
            DamageInfo info = new DamageInfo(amount, owner, hit.Value, direction);

            // Only remember the hit if it landed so a target that was invincible
            // can still be hit once its invincibility wears off.
            if (hit.Key.TakeDamage(info))
            {
                lastHitTimes[hit.Key] = Time.time;
            }
        }
    }

    private bool CanHit(Health target)
    {
        if (target == null || target == owner)
        {
            return false;
        }

        if (owner != null && target.Team == owner.Team)
        {
            return false;
        }


        if (lastHitTimes.TryGetValue(target, out float lastHitTime) &&
            (rehitInterval <= 0f || Time.time < lastHitTime + rehitInterval))
        {
            return false;
        }

        return true;
    }
}

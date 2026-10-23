
using System.Collections;
using UnityEngine;

public class LionRoarSpecial : PlayerSpecial
{
    [Header("Roar")]
    [SerializeField] private Hitbox roarHitbox;
    [SerializeField] private float roarDuration = 0.15f;
    [SerializeField] private float knockbackForce = 12f;
    [SerializeField] private float cooldownDuration = 4f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip roarSound;
    [SerializeField] private SpriteRenderer visual;

    private Health owner;
    private Body body;
    private Rigidbody2D rb;
    private Coroutine roarRoutine;

    public override float CooldownDuration => cooldownDuration;

    private void Awake()
    {
        owner = GetComponentInParent<Health>();
        body = GetComponentInParent<Body>();
        rb = GetComponentInParent<Rigidbody2D>();

        if (roarHitbox == null)
        {
            roarHitbox = GetComponentInChildren<Hitbox>();
        }

        if (roarHitbox != null)
        {
            roarHitbox.SetOwner(owner);
            roarHitbox.Deactivate();
            roarHitbox.TargetHit += HandleTargetHit;
        }
    }

    public override void Activate(Vector2 aimDirection)
    {
        if (roarHitbox == null || roarRoutine != null)
        {
            return;
        }

        roarHitbox.DamageBonus = body != null
            ? body.GetStatBonus(Stat.Damage)
            : 0f;

        if (audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound);
        }

        visual.enabled = true;

        roarRoutine = StartCoroutine(RoarRoutine());
    }

    private IEnumerator RoarRoutine()
    {
        roarHitbox.Activate();

        yield return new WaitForSeconds(roarDuration);

        roarHitbox.Deactivate();
        visual.enabled = false;
        roarRoutine = null;
    }

    private void HandleTargetHit(Health target)
    {
        if (target == null || rb == null)
        {
            return;
        }

        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();

        if (targetRb == null)
        {
            targetRb = target.GetComponentInParent<Rigidbody2D>();
        }

        if (targetRb == null)
        {
            return;
        }

        Vector2 direction = targetRb.position - rb.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            targetRb.AddForce(
                direction.normalized * knockbackForce,
                ForceMode2D.Impulse
            );
        }
    }

    private void OnDisable()
    {
        if (roarRoutine != null)
        {
            StopCoroutine(roarRoutine);
            roarRoutine = null;
        }

        if (roarHitbox != null)
        {
            roarHitbox.Deactivate();
        }

        visual.enabled = false;
    }

    private void OnDestroy()
    {
        if (roarHitbox != null)
        {
            roarHitbox.TargetHit -= HandleTargetHit;
        }
    }
}


using System.Collections;
using UnityEngine;

public class LionDash : PlayerDash
{
	[Header("Dash Settings")]
	[SerializeField] private float dashSpeed = 22f;
	[SerializeField] private float dashDuration = 0.15f;
	[SerializeField] private float cooldownDuration = 1f;

	[Header("Damage and Knockback")]
	[SerializeField] private Hitbox dashHitbox;
	[SerializeField] private float knockbackForce = 12f;

	private PlayerMovement movement;
	private Rigidbody2D rb;
	private Health owner;
	private Body body;

	private Coroutine dashRoutine;
	private float nextDashTime;

	public override float CooldownDuration => cooldownDuration;

	private void Awake()
	{
		movement = GetComponentInParent<PlayerMovement>();
		rb = GetComponentInParent<Rigidbody2D>();
		owner = GetComponentInParent<Health>();
		body = GetComponentInParent<Body>();

		if (dashHitbox == null)
		{
			dashHitbox = GetComponentInChildren<Hitbox>();
		}

		if (dashHitbox != null)
		{
			dashHitbox.SetOwner(owner);
			dashHitbox.Deactivate();
			dashHitbox.TargetHit += HandleTargetHit;
		}
		else
		{
			Debug.LogError(
				"LionDash requires a Hitbox on its prefab.",
				this
			);
		}
	}

	public override void Activate(Vector2 moveDirection)
	{
		if (movement == null
			|| dashHitbox == null
			|| dashRoutine != null
			|| Time.time < nextDashTime
			|| moveDirection.sqrMagnitude < 0.0001f)
		{
			return;
		}

		nextDashTime = Time.time + cooldownDuration;

		if (body != null)
		{
			dashHitbox.DamageBonus =
				body.GetStatBonus(Stat.Damage);
		}

		dashRoutine = StartCoroutine(
			DashRoutine(moveDirection.normalized)
		);
	}

	private IEnumerator DashRoutine(Vector2 direction)
	{
		dashHitbox.Activate();
		movement.StartDash(direction, dashSpeed);

		yield return new WaitForSeconds(dashDuration);

		dashHitbox.Deactivate();
		movement.EndDash();
		dashRoutine = null;
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

		Vector2 direction =
			targetRb.position - rb.position;

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
		if (dashRoutine != null)
		{
			StopCoroutine(dashRoutine);
			dashRoutine = null;
		}

		if (dashHitbox != null)
		{
			dashHitbox.Deactivate();
		}

		if (movement != null)
		{
			movement.EndDash();
		}
	}

	private void OnDestroy()
	{
		if (dashHitbox != null)
		{
			dashHitbox.TargetHit -= HandleTargetHit;
		}
	}
}

/*
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LionDash : PlayerDash
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float cooldownDuration = 1f;

    [Header("Damage and Knockback")]
    [SerializeField] private Hitbox dashHitbox;
    [SerializeField] private float knockbackForce = 12f;

    private Rigidbody2D rb;
    private Coroutine dashRoutine;
    private float nextDashTime;
	private PlayerMovement movement;

	public override float CooldownDuration => cooldownDuration;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
		movement = GetComponentInParent<PlayerMovement>();

		if (dashHitbox == null)
        {
            dashHitbox = GetComponentInChildren<Hitbox>();
        }

        if (dashHitbox != null)
        {
            dashHitbox.Deactivate();
            dashHitbox.TargetHit += HandleTargetHit;
        }
    }

    public override void Activate(Vector2 moveDirection)
    {
        if (rb == null || dashHitbox == null ||
            Time.time < nextDashTime ||
            moveDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
        }

        nextDashTime = Time.time + cooldownDuration;
        dashRoutine = StartCoroutine(
            DashRoutine(moveDirection.normalized));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        dashHitbox.Activate();

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = direction * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        dashHitbox.Deactivate();
        rb.linearVelocity = Vector2.zero;
        dashRoutine = null;
    }

    private void HandleTargetHit(Health target)
    {
        if (target == null)
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

        Vector2 direction =
            ((Vector2)target.transform.position - rb.position).normalized;

        if (direction.sqrMagnitude > 0.0001f)
        {
            targetRb.AddForce(
                direction * knockbackForce,
                ForceMode2D.Impulse);
        }
    }

    private void OnDisable()
    {
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
            dashRoutine = null;
        }

        if (dashHitbox != null)
        {
            dashHitbox.Deactivate();
        }
    }

    private void OnDestroy()
    {
        if (dashHitbox != null)
        {
            dashHitbox.TargetHit -= HandleTargetHit;
        }
    }
}
*/

using System.Collections;
using UnityEngine;

public class TinManDash : PlayerDash
{
	[Header("Dash Settings")]
	[SerializeField] private float dashSpeed = 22f;
	[SerializeField] private float dashDuration = 0.15f;
	[SerializeField] private float cooldownDuration = 1f;

	private PlayerMovement movement;
	private Coroutine dashRoutine;
	private float nextDashTime;

	public override float CooldownDuration => cooldownDuration;

	private void Awake()
	{
		movement = GetComponentInParent<PlayerMovement>();
	}

	public override void Activate(Vector2 moveDirection)
	{
		if (movement == null
			|| dashRoutine != null
			|| Time.time < nextDashTime
			|| moveDirection.sqrMagnitude < 0.0001f)
		{
			return;
		}

		nextDashTime = Time.time + cooldownDuration;

		dashRoutine = StartCoroutine(
			DashRoutine(moveDirection.normalized)
		);
	}

	private IEnumerator DashRoutine(Vector2 direction)
	{
		movement.StartDash(direction, dashSpeed);

		yield return new WaitForSeconds(dashDuration);

		movement.EndDash();
		dashRoutine = null;
	}

	private void OnDisable()
	{
		if (dashRoutine != null)
		{
			StopCoroutine(dashRoutine);
			dashRoutine = null;
		}

		if (movement != null)
		{
			movement.EndDash();
		}
	}
}



/*
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TinManDash : PlayerDash
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float cooldownDuration = 1f;

    private Rigidbody2D rb;
    private Coroutine dashRoutine;
    private float nextDashTime;
	private PlayerMovement movement;

	public override float CooldownDuration => cooldownDuration;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
		movement = GetComponentInParent<PlayerMovement>();
	}

    public override void Activate(Vector2 moveDirection)
    {
        if (rb == null || Time.time < nextDashTime ||
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
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = direction * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        dashRoutine = null;
    }

    private void OnDisable()
    {
        if (dashRoutine != null)
        {
            StopCoroutine(dashRoutine);
            dashRoutine = null;
        }
    }
}
*/
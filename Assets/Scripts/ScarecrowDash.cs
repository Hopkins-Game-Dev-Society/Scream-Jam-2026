/*
using System.Collections;
using UnityEngine;

public class ScarecrowDash : PlayerDash
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.08f;
    [SerializeField] private int dashCharges = 3;
    [SerializeField] private float rechargeDuration = 1.2f;

    private Rigidbody2D rb;
    private Coroutine dashRoutine;

    private int chargesRemaining;
    private float nextRechargeTime;
	private PlayerMovement movement;

	public override float CooldownDuration => rechargeDuration;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        chargesRemaining = dashCharges;
		movement = GetComponentInParent<PlayerMovement>();
	}

    public override void Activate(Vector2 moveDirection)
    {
        if (rb == null || moveDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        RechargeCharges();

        if (chargesRemaining <= 0 || dashRoutine != null)
        {
            return;
        }

        chargesRemaining--;

        if (chargesRemaining == 0)
        {
            nextRechargeTime = Time.time + rechargeDuration;
        }

        dashRoutine = StartCoroutine(
            DashRoutine(moveDirection.normalized)
        );
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

        dashRoutine = null;
    }

    private void RechargeCharges()
    {
        if (chargesRemaining == 0 && Time.time >= nextRechargeTime)
        {
            chargesRemaining = dashCharges;
        }
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


using System.Collections;
using UnityEngine;

public class ScarecrowDash : PlayerDash
{
	[Header("Dash Settings")]
	[SerializeField] private float dashSpeed = 16f;
	[SerializeField] private float dashDuration = 0.08f;
	[SerializeField] private int dashCharges = 3;
	[SerializeField] private float rechargeDuration = 1.2f;

	private PlayerMovement movement;
	private Coroutine dashRoutine;

	private int chargesRemaining;
	private float nextRechargeTime;

	public override float CooldownDuration => rechargeDuration;

	private void Awake()
	{
		movement = GetComponentInParent<PlayerMovement>();
		chargesRemaining = Mathf.Max(1, dashCharges);
	}

	public override void Activate(Vector2 moveDirection)
	{
		if (movement == null
			|| dashRoutine != null
			|| moveDirection.sqrMagnitude < 0.0001f)
		{
			return;
		}

		RechargeCharges();

		if (chargesRemaining <= 0)
		{
			return;
		}

		chargesRemaining--;

		dashRoutine = StartCoroutine(
			DashRoutine(moveDirection.normalized)
		);
		DashStarted();
	}

	private IEnumerator DashRoutine(Vector2 direction)
	{
		movement.StartDash(direction, dashSpeed);

		yield return new WaitForSeconds(dashDuration);

		movement.EndDash();
		dashRoutine = null;

		if (chargesRemaining == 0)
		{
			nextRechargeTime = Time.time + rechargeDuration;
		}
	}

	private void RechargeCharges()
	{
		if (chargesRemaining == 0
			&& Time.time >= nextRechargeTime)
		{
			chargesRemaining = Mathf.Max(1, dashCharges);
		}
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

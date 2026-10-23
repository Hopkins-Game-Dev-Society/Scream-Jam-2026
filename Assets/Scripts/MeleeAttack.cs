using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum AttackMotion
{
    Sweep,  // swings across a cone from one side to the other
    Thrust  // stabs straight out along the aim
}

// An attack that moves through a sweep or thrust. Each physics step the hitbox covers only the ground
// the striker passed over since the last step.
[DefaultExecutionOrder(-10)]
public class MeleeAttack : LimbAttack
{
    [SerializeField] private Hitbox hitbox;

    [Tooltip("Shown only while the hitbox is active, like the placeholder blade sprite.")]
    [SerializeField] private Renderer visual;

    [Tooltip("Moved through the swing, like the blade or spear. Its +x axis points away from the character.")]
    [SerializeField] private Transform striker;

    [SerializeField] private AttackMotion motion = AttackMotion.Sweep;

    [Tooltip("How far the attack reaches, in the character's own units. Enemies swing when the target's body is this close.")]
    [SerializeField] private float range = 3f;

    [Tooltip("Sweep: width of the cone in degrees.")]
    [SerializeField] private float sweepAngle = 90f;

    [Tooltip("Thrust: width of the stab, in the character's own units.")]
    [SerializeField] private float thrustWidth = 1.6f;

    [Tooltip("How far through the swing the striker is over the active time. 0 to 1 sweeps across the cone or stabs out to full reach. Going back down pulls a thrust back in.")]
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Delay between starting the attack and the hitbox turning on. Use it to telegraph enemy attacks.")]
    [SerializeField] private float windupTime = 0f;
    [SerializeField] private float activeTime = 0.2f;
    [SerializeField] private float cooldown = 0.35f;

    [Tooltip("Fired when the attack starts. Hook animations or sounds here.")]
    [SerializeField] private UnityEvent onAttackStarted = new UnityEvent();

    [Tooltip("Fired each time the swing damages something. Hook hit sounds here.")]
    [SerializeField] private UnityEvent onHit = new UnityEvent();

    private const float MinSweepStep = 1f;

    private Health owner;
    private PolygonCollider2D hitArea;
    private readonly List<Vector2> hitAreaPoints = new List<Vector2>();
    private Vector3 strikerScale;
    private float nextAttackTime;
    private bool swinging;
    private float swingStart;
    private float swingTime;   // 0 to 1 through the active time, as of the last physics step
    private float lastReach;   // motion curve value at the last physics step

    public Hitbox Hitbox => hitbox;
    // In world units.
    public float Range => range * (transform.parent != null ? Mathf.Abs(transform.parent.lossyScale.x) : 1f);
    //public bool IsAttacking { get; private set; }

    public bool isAttacking;

    public override bool IsAttacking => isAttacking;
    public bool CanAttack => !IsAttacking && Time.time >= nextAttackTime && (owner == null || !owner.IsDead);
    public float AttackSpeedMultiplier { get; set; } = 1f;
    public UnityEvent OnAttackStarted => onAttackStarted;
    // Sweeps start on the left of the aim and finish on the right. Set by Body for left arms.
    public bool SweepFromLeft { get; set; }

    private void Awake()
    {
        owner = GetComponentInParent<Health>();

        if (hitbox != null && !hitbox.TryGetComponent(out hitArea))
        {
            Debug.LogError("MeleeAttack: the hitbox needs a PolygonCollider2D to trace the swing with.", this);
        }

        if (hitbox != null)
        {
            hitbox.TargetHit += HandleTargetHit;
        }

        if (striker != null)
        {
            strikerScale = striker.localScale;
        }

        if (visual != null)
        {
            visual.enabled = false;
        }
    }

    // Points the attack along a direction and starts it. Returns true if an attack was started.
    public override bool TryAttack(Vector2 direction)
    {
        if (hitbox == null || hitArea == null || !CanAttack || direction.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        StartCoroutine(AttackRoutine());
        return true;
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        onAttackStarted.Invoke();

        if (windupTime > 0f)
        {
            yield return new WaitForSeconds(windupTime);
        }

        swingStart = Time.time;
        swingTime = 0f;
        lastReach = motionCurve.Evaluate(0f);
        SetActive(true);
        MoveStriker(lastReach);

        // FixedUpdate moves the hit area along the swing. Wait until it has covered all of it.
        while (swingTime < 1f)
        {
            yield return new WaitForFixedUpdate();
        }

        SetActive(false);

        isAttacking = false;
        //nextAttackTime = Time.time + cooldown;
        float speedMultiplier = Mathf.Max(AttackSpeedMultiplier, 0.01f);
        nextAttackTime = Time.time + cooldown / speedMultiplier;
    }

    private void FixedUpdate()
    {
        if (!swinging)
        {
            return;
        }

        swingTime = SwingTime();
        float reach = motionCurve.Evaluate(swingTime);

        TraceHitArea(lastReach, reach);
        lastReach = reach;
    }

    private void LateUpdate()
    {
        if (swinging)
        {
            MoveStriker(motionCurve.Evaluate(SwingTime()));
        }
    }

    private float SwingTime()
    {
        return activeTime > 0f ? Mathf.Clamp01((Time.time - swingStart) / activeTime) : 1f;
    }

    // Shapes the hitbox to the ground the striker covers going from one reach to another, in this
    // object's space where +x is the aim.
    private void TraceHitArea(float fromReach, float toReach)
    {
        hitAreaPoints.Clear();

        if (motion == AttackMotion.Sweep)
        {
            float from = SweepAngleAt(fromReach);
            float to = SweepAngleAt(toReach);

            if (Mathf.Abs(to - from) < MinSweepStep)
            {
                to = from + (SweepAngleAt(1f) > SweepAngleAt(0f) ? MinSweepStep : -MinSweepStep);
            }

            int segments = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(to - from) / 10f));
            hitAreaPoints.Add(Vector2.zero);

            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(from, to, (float)i / segments) * Mathf.Deg2Rad;
                hitAreaPoints.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * range);
            }
        }
        else
        {
            // The whole spear from the character out to its tip.
            float length = Mathf.Max(range * Mathf.Max(fromReach, toReach), 0.1f);
            float halfWidth = thrustWidth * 0.5f;

            hitAreaPoints.Add(new Vector2(0f, -halfWidth));
            hitAreaPoints.Add(new Vector2(length, -halfWidth));
            hitAreaPoints.Add(new Vector2(length, halfWidth));
            hitAreaPoints.Add(new Vector2(0f, halfWidth));
        }

        hitArea.SetPath(0, hitAreaPoints);
    }

    private void MoveStriker(float reach)
    {
        if (striker == null)
        {
            return;
        }

        if (motion == AttackMotion.Sweep)
        {
            striker.localRotation = Quaternion.Euler(0f, 0f, SweepAngleAt(reach));
        }
        else
        {
            striker.localScale = new Vector3(strikerScale.x * reach, strikerScale.y, strikerScale.z);
        }
    }

    // Angle from the aim in degrees. Positive is to the left of the aim.
    private float SweepAngleAt(float reach)
    {
        float half = sweepAngle * 0.5f;
        return SweepFromLeft ? Mathf.Lerp(half, -half, reach) : Mathf.Lerp(-half, half, reach);
    }

    private void SetActive(bool active)
    {
        swinging = active;

        if (active)
        {
            hitbox.Activate();
        }
        else
        {
            hitbox.Deactivate();
        }

        if (visual != null)
        {
            visual.enabled = active;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isAttacking = false;

        if (hitbox != null)
        {
            SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (hitbox != null)
        {
            hitbox.TargetHit -= HandleTargetHit;
        }
    }

    private void HandleTargetHit(Health target)
    {
        onHit.Invoke();
    }
}

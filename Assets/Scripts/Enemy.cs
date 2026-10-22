using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Body))]
public class Enemy : MonoBehaviour
{
    private static readonly BodyPart[] Arms = { BodyPart.RightArm, BodyPart.LeftArm };
    private const float ArriveDistance = 0.5f;

    private enum State
    {
        Patrol,
        Chase
    }

    [Tooltip("Speed while chasing the player.")]
    [SerializeField] private float moveSpeed = 25f;
    [Tooltip("Found automatically when left empty.")]
    [SerializeField] private Rigidbody2D player;
    [SerializeField] private SightCone sightCone;

    [Header("Patrol")]
    [Tooltip("Child objects the enemy walks between. Move them in the Scene view. An empty slot means the enemy's start position.")]
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;
    [SerializeField] private float patrolSpeed = 10f;
    [Tooltip("Seconds to wait at each patrol point before heading to the other one.")]
    [SerializeField] private float patrolWait = 1f;
    [Tooltip("How fast the enemy turns to look where it's going, in degrees per second.")]
    [SerializeField] private float turnSpeed = 270f;

    [Header("Chase")]
    [Tooltip("The enemy gives up and goes back to patrolling once the player is this far from its patrol route.")]
    [SerializeField] private float leashDistance = 35f;

    [Header("Events")]
    [Tooltip("Fired when the enemy spots the player. Hook sounds here.")]
    [SerializeField] private UnityEvent onSpottedPlayer = new UnityEvent();
    [Tooltip("Fired when the enemy gives up the chase.")]
    [SerializeField] private UnityEvent onLostPlayer = new UnityEvent();

    private Rigidbody2D rb;
    private Body body;
    private Health health;
    private Health playerHealth;
    private readonly List<Hurtbox> playerHurtboxes = new List<Hurtbox>();
    private readonly List<Collider2D> hurtboxColliders = new List<Collider2D>();

    private State state = State.Patrol;
    private readonly Vector2[] patrolPoints = new Vector2[2];
    private bool patrolPointsSaved;
    private int patrolIndex;
    private float waitUntil;
    private Vector2 lookDirection = Vector2.down;

    public bool IsChasing => state == State.Chase;
    public UnityEvent OnSpottedPlayer => onSpottedPlayer;
    public UnityEvent OnLostPlayer => onLostPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Body>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDamaged.AddListener(HandleDamaged);
    }

    private void OnDisable()
    {
        health.OnDamaged.RemoveListener(HandleDamaged);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null)
        {
            PlayerController found = FindAnyObjectByType<PlayerController>();

            if (found != null)
                player = found.GetComponent<Rigidbody2D>();
        }

        if (player != null)
            playerHealth = player.GetComponent<Health>();

        patrolPoints[0] = PatrolPoint(0);
        patrolPoints[1] = PatrolPoint(1);
        patrolPointsSaved = true;
        LookAt(PatrolPoint(0) - (Vector2)transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (health.IsDead)
        {
            if (rb.simulated)
            {
                rb.simulated = false;

                if (sightCone != null)
                    sightCone.gameObject.SetActive(false);
            }
            return;
        }

        if (state == State.Patrol && CanSeePlayer())
        {
            StartChasing();
        }
        else if (state == State.Chase && !PlayerInsideLeash())
        {
            StopChasing();
        }

        if (state == State.Chase)
            Chase();
        else
            Patrol();

        if (sightCone != null)
        {
            sightCone.Direction = lookDirection;
            sightCone.Alert = state == State.Chase;
        }
    }

    // Walks to one patrol point, waits there while turning toward the other, then walks to that one.
    private void Patrol()
    {
        Vector2 position = transform.position;
        Vector2 target = PatrolPoint(patrolIndex);

        if (Vector2.Distance(position, target) <= ArriveDistance)
        {
            patrolIndex = 1 - patrolIndex;
            waitUntil = Time.time + patrolWait;
            target = PatrolPoint(patrolIndex);
        }

        TurnToward(target - position);

        if (Time.time < waitUntil)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = (target - position).normalized * Speed(patrolSpeed);
    }

    private void Chase()
    {
        // Measure to the nearest point of the player's body
        Vector2 toPlayer = NearestPlayerPoint() - (Vector2)transform.position;

        if (toPlayer.sqrMagnitude < 0.0001f)
            toPlayer = player.position - (Vector2)transform.position;

        LookAt(toPlayer);

        // Stand still while swinging, or while the player is within reach of an arm.
        if (body.IsAttacking || AttackIfInReach(toPlayer))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = toPlayer.normalized * Speed(moveSpeed);
    }

    private void StartChasing()
    {
        state = State.Chase;
        onSpottedPlayer.Invoke();
    }

    // Getting hit by the player starts a chase.
    private void HandleDamaged(DamageInfo damage)
    {
        bool byPlayer = damage.Attacker != null && damage.Attacker.Team == Team.Player;

        if (state == State.Patrol && byPlayer && !health.IsDead && PlayerInsideLeash())
            StartChasing();
    }

    // Heads back to the nearer patrol point after a short pause.
    private void StopChasing()
    {
        state = State.Patrol;
        rb.linearVelocity = Vector2.zero;
        waitUntil = Time.time + patrolWait;

        Vector2 position = transform.position;
        patrolIndex = Vector2.Distance(position, PatrolPoint(0)) <= Vector2.Distance(position, PatrolPoint(1)) ? 0 : 1;

        onLostPlayer.Invoke();
    }

    private bool CanSeePlayer()
    {
        if (sightCone == null || !PlayerInsideLeash())
            return false;

        return sightCone.CanSee(player.position) || sightCone.CanSee(NearestPlayerPoint());
    }

    private bool PlayerInsideLeash()
    {
        bool alive = player != null && (playerHealth == null || !playerHealth.IsDead);
        return alive && DistanceFromRoute(player.position) <= leashDistance;
    }

    // Distance from a point to the nearest spot on the line between the two patrol points.
    private float DistanceFromRoute(Vector2 point)
    {
        Vector2 a = PatrolPoint(0);
        Vector2 route = PatrolPoint(1) - a;
        float along = route.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(point - a, route) / route.sqrMagnitude) : 0f;

        return Vector2.Distance(point, a + route * along);
    }

    private Vector2 PatrolPoint(int index)
    {
        if (patrolPointsSaved)
            return patrolPoints[index];

        Transform point = index == 0 ? patrolPointA : patrolPointB;
        return point != null ? point.position : transform.position;
    }

    private float Speed(float baseSpeed)
    {
        return Mathf.Max(baseSpeed + body.GetStatBonus(Stat.MoveSpeed), 0f);
    }

    private void LookAt(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        lookDirection = direction.normalized;
        body.FaceDirection(lookDirection);
    }

    private void TurnToward(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        float current = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        float target = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(current, target, turnSpeed * Time.deltaTime) * Mathf.Deg2Rad;

        LookAt(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
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
        Vector2 position = transform.position;
        bool inReach = false;

        foreach (BodyPart arm in Arms)
        {
            MeleeAttack attack = body.GetAttack(arm);

            if (attack == null || attack.Hitbox == null || toPlayer.magnitude > attack.Range ||
                attack.Hitbox.IsBlocked(position, position + toPlayer))
                continue;

            inReach = true;

            if (attack.TryAttack(toPlayer))
                break;
        }

        return inReach;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(PatrolPoint(0), PatrolPoint(1));
    }

    // How far the player can get from the route before the enemy gives up.
    private void OnDrawGizmosSelected()
    {
        Vector2 a = PatrolPoint(0);
        Vector2 b = PatrolPoint(1);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Vector2 side = Vector2.Perpendicular((b - a).normalized) * leashDistance;
        Gizmos.DrawWireSphere(a, leashDistance);
        Gizmos.DrawWireSphere(b, leashDistance);
        Gizmos.DrawLine(a + side, b + side);
        Gizmos.DrawLine(a - side, b - side);
    }
}

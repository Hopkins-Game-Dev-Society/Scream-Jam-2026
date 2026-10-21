using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Hitbox hitbox;

    [Tooltip("Shown only while the hitbox is active, e.g. a placeholder swing sprite.")]
    [SerializeField] private Renderer visual;

    [Tooltip("How far the attack reaches, in the character's own units. Enemies swing when the target's body is this close.")]
    [SerializeField] private float range = 3f;

    [Tooltip("Delay between starting the attack and the hitbox turning on. Use it to telegraph enemy attacks.")]
    [SerializeField] private float windupTime = 0f;
    [SerializeField] private float activeTime = 0.15f;
    [SerializeField] private float cooldown = 0.35f;

    [Tooltip("Fired when the attack starts. Hook animations or sounds here.")]
    [SerializeField] private UnityEvent onAttackStarted = new UnityEvent();

    private Health owner;
    private float nextAttackTime;

    public Hitbox Hitbox => hitbox;
    // In world units. The attack scales with the character it belongs to, so its range does too.
    public float Range => range * (transform.parent != null ? Mathf.Abs(transform.parent.lossyScale.x) : 1f);
    public bool IsAttacking { get; private set; }
    public bool CanAttack => !IsAttacking && Time.time >= nextAttackTime && (owner == null || !owner.IsDead);
    public UnityEvent OnAttackStarted => onAttackStarted;

    private void Awake()
    {
        owner = GetComponentInParent<Health>();

        if (visual != null)
        {
            visual.enabled = false;
        }
    }

    // Points the attack along a direction and starts it. Returns true if an attack was started.
    public bool TryAttack(Vector2 direction)
    {
        if (hitbox == null || !CanAttack || direction.sqrMagnitude < 0.0001f)
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
        IsAttacking = true;
        onAttackStarted.Invoke();

        if (windupTime > 0f)
        {
            yield return new WaitForSeconds(windupTime);
        }

        SetActive(true);
        yield return new WaitForSeconds(activeTime);
        SetActive(false);

        IsAttacking = false;
        nextAttackTime = Time.time + cooldown;
    }

    private void SetActive(bool active)
    {
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
        // Disabling stops the coroutine, so make sure the hitbox isn't left on.
        StopAllCoroutines();
        IsAttacking = false;

        if (hitbox != null)
        {
            SetActive(false);
        }
    }
}

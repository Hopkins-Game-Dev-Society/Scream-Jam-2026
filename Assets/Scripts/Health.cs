using UnityEngine;
using UnityEngine.Events;

public enum Team
{
    Player,
    Enemy
}

// Information about a single hit. Passed to Health.TakeDamage and its On Damaged / On Died events.
public readonly struct DamageInfo
{
    public float Amount { get; }
    public Health Attacker { get; }    // null for hazards or anything without a Health
    public Hurtbox Hurtbox { get; }    // the body part that was hit. null if not hit through one
    public Vector2 Direction { get; }  // normalized, from the attacker toward the target

    public DamageInfo(float amount, Health attacker = null, Hurtbox hurtbox = null, Vector2 direction = default)
    {
        Amount = amount;
        Attacker = attacker;
        Hurtbox = hurtbox;
        Direction = direction;
    }
}

// Shared health component for the player and enemies.
public class Health : MonoBehaviour
{
    [SerializeField] private Team team = Team.Enemy;
    [SerializeField] private float maxHealth = 3f;

    [Tooltip("Seconds of invulnerability after taking damage.")]
    [SerializeField] private float invincibilityDuration = 0f;

    [Tooltip("Leave off for the player and for enemies that leave a corpse to loot.")]
    [SerializeField] private bool destroyOnDeath = false;
    [SerializeField] private float destroyDelay = 0f;

    [Header("Events")]
    [SerializeField] private UnityEvent<float, float> onHealthChanged = new UnityEvent<float, float>(); // (current, max)
    [SerializeField] private UnityEvent<DamageInfo> onDamaged = new UnityEvent<DamageInfo>();
    [SerializeField] private UnityEvent<DamageInfo> onDied = new UnityEvent<DamageInfo>(); // receives the killing blow

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float knockbackForce = 5f;

    private float currentHealth;
    private float invincibleUntil;

    public Team Team => team;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsInvincible => Time.time < invincibleUntil;

    public UnityEvent<float, float> OnHealthChanged => onHealthChanged;
    public UnityEvent<DamageInfo> OnDamaged => onDamaged;
    public UnityEvent<DamageInfo> OnDied => onDied;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    // Returns true if the damage was applied (false if dead or invincible).
    public bool TakeDamage(DamageInfo damage)
    {
        if (damage.Amount <= 0f || IsDead || IsInvincible)
        {
            return false;
        }

        currentHealth = Mathf.Max(currentHealth - damage.Amount, 0f);
        invincibleUntil = Time.time + invincibilityDuration;


        // Apply knockback away from the damage source.
        if (rb != null)
        {
            Vector2 knockbackDirection =
                ((Vector2)transform.position - damage.Direction).normalized;

            rb.AddForce(knockbackDirection * knockbackForce,
                ForceMode2D.Impulse);
        }

        onDamaged.Invoke(damage);
        onHealthChanged.Invoke(currentHealth, maxHealth);

        if (IsDead)
        {
            Die(damage);
        }

        return true;
    } //Adding element here so that if you take damage you also get knocked back a bit

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    // Gaining max health heals by the same amount. Losing it only lowers
    // current health if it's above the new max.
    public void SetMaxHealth(float newMaxHealth)
    {
        newMaxHealth = Mathf.Max(newMaxHealth, 1f);
        float gained = newMaxHealth - maxHealth;
        maxHealth = newMaxHealth;

        if (IsDead)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + Mathf.Max(gained, 0f), maxHealth);
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }

    private void Die(DamageInfo killingBlow)
    {
        onDied.Invoke(killingBlow);

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}


using UnityEngine;

public class ProjectileAttack : LimbAttack
{
    [Header("Projectile")]
    [SerializeField] private ProjectileMovement projectilePrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Attack")]
    [SerializeField] private float cooldown = 0.5f;

    public float AttackSpeedMultiplier { get; set; } = 1f;

    private float nextAttackTime;

	private Health owner;
	private Body body;

	public override bool IsAttacking => false;

	private void Awake()
	{
		owner = GetComponentInParent<Health>();
		body = GetComponentInParent<Body>();
	}

	public override bool TryAttack(Vector2 direction)
    {
        if (projectilePrefab == null
            || firingPoint == null
            || direction.sqrMagnitude < 0.0001f
            || Time.time < nextAttackTime)
        {
            return false;
        }

        ProjectileMovement projectile = Instantiate(
            projectilePrefab,
            firingPoint.position,
            Quaternion.identity
        );

		float damageBonus = body != null
	        ? body.GetStatBonus(Stat.Damage)
	        : 0f;

		projectile.Launch(
			owner,
			direction.normalized,
			projectileSpeed,
			damageBonus);
		//nextAttackTime = Time.time + cooldown;

        float speedMultiplier = Mathf.Max(AttackSpeedMultiplier, 0.01f);
        nextAttackTime = Time.time + cooldown / speedMultiplier;

        return true;
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Hitbox))]
public class ProjectileMovement : MonoBehaviour
{
	[SerializeField] private float lifetime = 4f;

	private Rigidbody2D rb;
	private Hitbox hitbox;
	private bool launched;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		hitbox = GetComponent<Hitbox>();

		hitbox.TargetHit += HandleTargetHit;
	}

	public void Launch(
		Health owner,
		Vector2 direction,
		float speed,
		float damageBonus)
	{
		if (launched || owner == null || direction.sqrMagnitude < 0.0001f)
		{
			Destroy(gameObject);
			return;
		}

		launched = true;

		hitbox.SetOwner(owner);
		hitbox.DamageBonus = damageBonus;
		hitbox.Activate();

		direction.Normalize();

		rb.linearVelocity = direction * speed;
		transform.right = direction;

		Destroy(gameObject, lifetime);
	}

	private void HandleTargetHit(Health target)
	{
		Destroy(gameObject);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// Hurtboxes are handled by Hitbox. Other trigger colliders
		// do not necessarily represent walls, so don't destroy here.
		if (other.TryGetComponent(out Hurtbox _))
		{
			return;
		}

		// Non-trigger colliders can stop the projectile, provided
		// the projectile's layer and collision matrix allow contact.
		if (!other.isTrigger)
		{
			Destroy(gameObject);
		}
	}

	private void OnDestroy()
	{
		if (hitbox != null)
		{
			hitbox.TargetHit -= HandleTargetHit;
		}
	}
}
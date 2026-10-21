using UnityEngine;

// Marks a collider as a body part that can take damage. Hitboxes only hit colliders that have one
// so weapons, props, and trigger zones aren't hit by accident.
// In a limb prefab put it on the root next to the Limb and the colliders. Damage goes to the
// closest Health above it and the body part comes from the Limb.
[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour
{
    [Tooltip("Damage taken through this body part is multiplied by this")]
    [SerializeField] private float damageMultiplier = 1f;

    private Health health;
    private Limb limb;

    public Health Health => health;
    public Limb Limb => limb;                                          // null for enemies without limbs
    public BodyPart BodyPart => limb != null ? limb.Slot : BodyPart.None;
    public float DamageMultiplier => damageMultiplier;

    private void Awake()
    {
        health = GetComponentInParent<Health>();
        limb = GetComponentInParent<Limb>();
    }
}

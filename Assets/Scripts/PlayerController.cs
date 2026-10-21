using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Body), typeof(PlayerMovement), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Limb Library")]
    [Tooltip("Every limb prefab the player can swap to. Fresh limbs are spawned from these.")]
    [SerializeField] private List<Limb> limbPrefabs = new List<Limb>();

    private Body body;
    private Health health;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    public Body Body => body;
    public Health Health => health;
    public PlayerMovement Movement => movement;
    public bool IsDead => health.IsDead;

    private void Awake()
    {
        body = GetComponent<Body>();
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();

        if (health.Team != Team.Player)
        {
            Debug.LogError("PlayerController: set the Health component's Team to Player.", this);
        }
    }

    private void OnEnable()
    {
        health.OnDied.AddListener(HandleDeath);
        body.OnLimbsChanged.AddListener(ApplyLimbStats);
    }

    private void OnDisable()
    {
        health.OnDied.RemoveListener(HandleDeath);
        body.OnLimbsChanged.RemoveListener(ApplyLimbStats);
    }

    private void Update()
    {
        if (!IsDead)
        {
            body.FaceDirection(movement.AimDirection);
        }
    }


    public bool EquipLimb(LimbKind kind, BodyPart slot)
    {
        if (IsDead)
        {
            return false;
        }

        Limb prefab = FindLimbPrefab(kind, slot);
        return prefab != null && body.ReplaceLimb(prefab) != null;
    }
 
    public bool SwapLimbWith(Body corpse, BodyPart theirSlot, BodyPart mySlot)
    {
        Limb mine = body.GetLimb(mySlot);
        Limb theirs = corpse.GetLimb(theirSlot);

        if (IsDead || mine == null || theirs == null || !theirSlot.Fits(mySlot) || mine.Kind == theirs.Kind)
        {
            return false;
        }

        Limb newMine = FindLimbPrefab(theirs.Kind, mySlot);
        Limb newTheirs = FindLimbPrefab(mine.Kind, theirSlot);

        if (newMine == null || newTheirs == null)
        {
            return false;
        }

        body.ReplaceLimb(newMine);
        corpse.ReplaceLimb(newTheirs);
        return true;
    }

    private Limb FindLimbPrefab(LimbKind kind, BodyPart slot)
    {
        Limb prefab = limbPrefabs.Find(p => p != null && p.Kind == kind && p.Slot == slot);

        if (prefab == null)
        {
            Debug.LogError($"PlayerController: the Limb Library has no {kind} {slot} prefab.", this);
        }

        return prefab;
    }

     private void ApplyLimbStats()
    {
        movement.MoveSpeedBonus = body.GetStatBonus(Stat.MoveSpeed);
    }

    private void HandleDeath(DamageInfo killingBlow)
    {
         movement.enabled = false;
        rb.linearVelocity = Vector2.zero;
    }
}

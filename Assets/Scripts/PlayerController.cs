using System.Collections.Generic;
using UnityEngine.InputSystem;
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

    private void HandleAttackInput()
    {
        if (Mouse.current == null || IsDead)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryArmAttack(BodyPart.LeftArm);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryArmAttack(BodyPart.RightArm);
        }
    }

    private void HandleDashInput()
    {
        if (Keyboard.current == null || IsDead)
        {
            return;
        }

        // Left Shift activates the currently equipped dash.
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            Debug.Log("Dash input detected.");
            PlayerDash dash = body.GetDash();

            if (dash == null)
            {
                Debug.LogWarning("No dash ability found on the current body.");
                return;
            }

            Debug.Log($"Dash ability found: {dash.GetType().Name}");
            Debug.Log($"Aim direction: {movement.AimDirection}");

            if (dash != null)
            {
                dash.Activate(movement.AimDirection);
            }
        }
    }

    private void TryArmAttack(BodyPart slot)
    {
        LimbAttack attack = body.GetLimbAttack(slot);

        if (attack != null)
        {
            attack.TryAttack(movement.AimDirection);
        }
    }

    private void HandleSpecialInput()
    {
        if (Keyboard.current == null
            || !Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return;
        }

        if (IsDead)
        {
            return;
        }

        PlayerSpecial special = body.GetSpecial();

        if (special != null)
        {
            special.TryActivate(movement.AimDirection);
        }
    }

    private void Update()
    {
        if (!IsDead)
        {
            body.FaceDirection(movement.AimDirection);
            HandleSpecialInput();
            HandleAttackInput();
            HandleDashInput();
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
        if (IsDead || corpse == null || !theirSlot.Fits(mySlot))
        {
            return false;
        }

        // Keep the rest of the existing swap logic below.

        bool theirSlotIsLeg =
            theirSlot == BodyPart.LeftLeg
            || theirSlot == BodyPart.RightLeg;

        bool mySlotIsLeg =
            mySlot == BodyPart.LeftLeg
            || mySlot == BodyPart.RightLeg;

        // Legs are exchanged as a complete pair.
        if (theirSlotIsLeg && mySlotIsLeg)
        {
            Limb myLeftLeg = body.GetLimb(BodyPart.LeftLeg);
            Limb theirLeftLeg = corpse.GetLimb(BodyPart.LeftLeg);

            if (myLeftLeg == null || theirLeftLeg == null
                || myLeftLeg.Kind == theirLeftLeg.Kind)
            {
                return false;
            }

            Limb myNewLeft = FindLimbPrefab(
                theirLeftLeg.Kind, BodyPart.LeftLeg);
            Limb myNewRight = FindLimbPrefab(
                theirLeftLeg.Kind, BodyPart.RightLeg);

            Limb corpseNewLeft = FindLimbPrefab(
                myLeftLeg.Kind, BodyPart.LeftLeg);
            Limb corpseNewRight = FindLimbPrefab(
                myLeftLeg.Kind, BodyPart.RightLeg);

            if (myNewLeft == null || myNewRight == null
                || corpseNewLeft == null || corpseNewRight == null)
            {
                return false;
            }

            body.ReplaceLegs(myNewLeft, myNewRight);
            corpse.ReplaceLegs(corpseNewLeft, corpseNewRight);

            return true;
        }

        // All other body parts retain their original individual-swap behavior.
        Limb mine = body.GetLimb(mySlot);
        Limb theirs = corpse.GetLimb(theirSlot);

        if (mine == null || theirs == null || mine.Kind == theirs.Kind)
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

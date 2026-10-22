using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Facing
{
    Down,
    Up,
    Left,
    Right
}

// What a character is made of. The limb in each body-part socket, which way it's facing,
// the stat bonuses its limbs add up to, and the attacks its arms give it.
// Used by both the player and enemies.
[RequireComponent(typeof(Health))]
public class Body : MonoBehaviour
{
    private static readonly BodyPart[] Slots =
    {
        BodyPart.Head,
        BodyPart.Chest,
        BodyPart.LeftArm,
        BodyPart.RightArm,
        BodyPart.LeftLeg,
        BodyPart.RightLeg
    };

    [Header("Limb Sockets")]
    [Tooltip("The animated body part objects under Character Visuals. Each holds one limb prefab instance.")]
    [SerializeField] private Transform headSocket;
    [SerializeField] private Transform chestSocket;
    [SerializeField] private Transform leftArmSocket;
    [SerializeField] private Transform rightArmSocket;
    [SerializeField] private Transform leftLegSocket;
    [SerializeField] private Transform rightLegSocket;

    [Header("Events")]
    [Tooltip("Fired after a limb is swapped, and once at start so listeners can set themselves up.")]
    [SerializeField] private UnityEvent onLimbsChanged = new UnityEvent();

    private static readonly HashSet<Body> all = new HashSet<Body>();

    private readonly Dictionary<BodyPart, Limb> limbs = new Dictionary<BodyPart, Limb>();
    private readonly Dictionary<BodyPart, MeleeAttack> attacks = new Dictionary<BodyPart, MeleeAttack>();
    private readonly Dictionary<BodyPart, PlayerSpecial> specials = new Dictionary<BodyPart, PlayerSpecial>();
    //private readonly Dictionary<BodyPart, PlayerDash> dashes = new Dictionary<BodyPart, PlayerDash>();
    private readonly Dictionary<Stat, float> statBonuses = new Dictionary<Stat, float>();

    private readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private Health health;
    private float baseMaxHealth;

    private PlayerDash dashAbility;


    // Every active body in the scene.
    public static IReadOnlyCollection<Body> All => all;

    public Health Health => health;
    public Facing Facing { get; private set; } = Facing.Down;
    public UnityEvent OnLimbsChanged => onLimbsChanged;

    public bool IsAttacking
    {
        get
        {
            foreach (MeleeAttack attack in attacks.Values)
            {
                if (attack != null && attack.IsAttacking)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        baseMaxHealth = health.MaxHealth;

        FindStartingLimbs();
    }

    private void OnEnable()
    {
        all.Add(this);
    }

    private void OnDisable()
    {
        all.Remove(this);
    }

    private void Start()
    {
        ApplyStats();
        onLimbsChanged.Invoke();
    }

    // World space box around every limb's sprite.
    public Bounds GetVisualBounds()
    {
        bool found = false;
        Bounds bounds = new Bounds(transform.position, Vector3.zero);

        foreach (Limb limb in limbs.Values)
        {
            if (limb == null)
            {
                continue;
            }

            limb.GetComponentsInChildren(renderers);

            foreach (SpriteRenderer spriteRenderer in renderers)
            {
                if (found)
                {
                    bounds.Encapsulate(spriteRenderer.bounds);
                }
                else
                {
                    bounds = spriteRenderer.bounds;
                    found = true;
                }
            }
        }

        return bounds;
    }

    // Returns the limb in a slot or null if it's empty.
    public Limb GetLimb(BodyPart slot)
    {
        return limbs.TryGetValue(slot, out Limb limb) ? limb : null;
    }

    // Returns the attack the limb in a slot gives (arms) or null if it has none.
    public MeleeAttack GetAttack(BodyPart slot)
    {
        return attacks.TryGetValue(slot, out MeleeAttack attack) ? attack : null;
    } //This is the big section here where everything is going to be actually implemented

    //Also need to create basically identical systems to this but for the special and dash slots.

    // Total of every equipped limb's modifiers for a stat. Add it to the character's base value.
    // Returns the special ability provided by the chest.
    public PlayerSpecial GetSpecial()
    {
        return specials.TryGetValue(BodyPart.Chest, out PlayerSpecial special)
            ? special
            : null;
    }

    // Returns the dash ability provided by the specified leg.
    public PlayerDash GetDash()
    {
        return dashAbility;
    }

    public float GetStatBonus(Stat stat)
    {
        return statBonuses.TryGetValue(stat, out float bonus) ? bonus : 0f;
    }

    // Replaces the limb in the prefab's slot with a fresh instance of the prefab. Returns the new limb.
    
    public Limb ReplaceLimb(Limb prefab)
    {
    if (prefab == null)
    {
        return null;
    }

    if (prefab.Slot == BodyPart.LeftLeg
        || prefab.Slot == BodyPart.RightLeg)
    {
         Debug.LogError("Body: use ReplaceLegs with both leg prefabs.", this);
            return null;
        }

    Transform socket = GetSocket(prefab.Slot);

    if (socket == null)
    {
        Debug.LogError(
            $"Body: the {prefab.Slot} socket isn't assigned.",
            this);
        return null;
    }

    RemoveLimb(prefab.Slot);

    Limb limb = Instantiate(prefab, socket);
    AddLimb(prefab.Slot, limb);
    ApplyStats();

    onLimbsChanged.Invoke();
    return limb;
}


    public Limb ReplaceLegs(Limb leftPrefab, Limb rightPrefab)
    {
        if (leftPrefab == null || rightPrefab == null
            || leftPrefab.Slot != BodyPart.LeftLeg
            || rightPrefab.Slot != BodyPart.RightLeg
            || leftPrefab.Kind != rightPrefab.Kind)
        {
            Debug.LogError(
                "Body: ReplaceLegs requires matching left and right leg prefabs.",
                this);
            return null;
        }

        Transform leftSocket = GetSocket(BodyPart.LeftLeg);
        Transform rightSocket = GetSocket(BodyPart.RightLeg);

        if (leftSocket == null || rightSocket == null)
        {
            Debug.LogError("Body: both leg sockets must be assigned.", this);
            return null;
        }

        RemoveLimb(BodyPart.LeftLeg);
        RemoveLimb(BodyPart.RightLeg);

        Limb leftLeg = Instantiate(leftPrefab, leftSocket);
        Limb rightLeg = Instantiate(rightPrefab, rightSocket);

        AddLimb(BodyPart.LeftLeg, leftLeg);
        AddLimb(BodyPart.RightLeg, rightLeg);

        ApplyStats();
        onLimbsChanged.Invoke();

        return leftLeg;
    }


    private void SetLimbSlot(Limb limb, BodyPart slot)
    {
        limb.SetSlot(slot);
    }


    // Turns the body toward a direction. Horizontal has priority when tied
    public void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Facing facing;

        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            facing = direction.x < 0f ? Facing.Left : Facing.Right;
        }
        else
        {
            facing = direction.y < 0f ? Facing.Down : Facing.Up;
        }

        if (facing == Facing)
        {
            return;
        }

        Facing = facing;

        foreach (Limb limb in limbs.Values)
        {
            ShowFacing(limb);
        }
    }

    private void FindStartingLimbs()
    {
        foreach (BodyPart slot in Slots)
        {
            Transform socket = GetSocket(slot);

            if (socket == null)
            {
                Debug.LogError($"Body: the {slot} socket isn't assigned.", this);
                continue;
            }

            Limb limb = socket.GetComponentInChildren<Limb>();

            if (limb == null)
            {
                Debug.LogError($"Body: the {slot} socket has no limb prefab in it.", socket);
                continue;
            }

            if (limb.Slot != slot)
            {
                Debug.LogError($"Body: {limb.name} is set to the {limb.Slot} slot but is in the {slot} socket.", limb);
            }

            AddLimb(slot, limb);
        }
    }

    private void AddLimb(BodyPart slot, Limb limb)
    {
        limbs[slot] = limb;
        SetLayer(limb.gameObject);

        if (limb.AttackPrefab != null)
        {
            attacks[slot] = Instantiate(limb.AttackPrefab, transform);
            SetLayer(attacks[slot].gameObject);

            // Each arm swings across from its own side.
            attacks[slot].SweepFromLeft = slot == BodyPart.LeftArm;
        }

        //Added from the origin here
        if (slot == BodyPart.Chest && limb.SpecialPrefab != null)
        {
            specials[slot] = Instantiate(limb.SpecialPrefab, transform);
            SetLayer(specials[slot].gameObject);
        }

        if (slot == BodyPart.LeftLeg && limb.DashPrefab != null)
        {
            dashAbility = Instantiate(limb.DashPrefab, transform);
            SetLayer(dashAbility.gameObject);
        }

        ShowFacing(limb);
    }

    private void SetLayer(GameObject root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = gameObject.layer;
        }
    }

    private void RemoveLimb(BodyPart slot)
    {
        if (limbs.TryGetValue(slot, out Limb limb))
        {
            if (limb != null)
            {
                Destroy(limb.gameObject);
            }

            limbs.Remove(slot);
        }

        if (attacks.TryGetValue(slot, out MeleeAttack attack))
        {
            if (attack != null)
            {
                Destroy(attack.gameObject);
            }

            attacks.Remove(slot);
        }

        if (specials.TryGetValue(slot, out PlayerSpecial special))
        {
            if (special != null)
            {
                Destroy(special.gameObject);
            }

            specials.Remove(slot);
        }

        if (slot == BodyPart.LeftLeg && dashAbility != null)
        {
            Destroy(dashAbility.gameObject);
            dashAbility = null;
        }
    }

    private void ApplyStats()
    {
        statBonuses.Clear();

        foreach (Limb limb in limbs.Values)
        {
            if (limb == null)
            {
                continue;
            }

            foreach (StatModifier modifier in limb.StatModifiers)
            {
                statBonuses[modifier.stat] = GetStatBonus(modifier.stat) + modifier.amount;
            }
        }

        health.SetMaxHealth(baseMaxHealth + GetStatBonus(Stat.MaxHealth));

        foreach (MeleeAttack attack in attacks.Values)
        {
            if (attack != null && attack.Hitbox != null)
            {
                attack.Hitbox.DamageBonus = GetStatBonus(Stat.Damage);
            }
        }
    }

    private void ShowFacing(Limb limb)
    {
        if (limb != null && limb.TryGetComponent(out LimbVisual visual))
        {
            visual.SetFacing(Facing);
        }
    }

    private Transform GetSocket(BodyPart slot)
    {
        switch (slot)
        {
            case BodyPart.Head: return headSocket;
            case BodyPart.Chest: return chestSocket;
            case BodyPart.LeftArm: return leftArmSocket;
            case BodyPart.RightArm: return rightArmSocket;
            case BodyPart.LeftLeg: return leftLegSocket;
            case BodyPart.RightLeg: return rightLegSocket;
            default: return null;
        }
    }
}

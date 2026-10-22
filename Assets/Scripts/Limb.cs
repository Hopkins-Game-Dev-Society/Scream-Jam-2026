using System.Collections.Generic;
using UnityEngine;

public enum LimbKind
{
    Lion,
    Tin,
    Scarecrow
}

public enum BodyPart
{
    None,
    Head,
    Chest,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}

public static class BodyPartExtensions
{
    // Whether a limb taken from one slot can go in another.
    public static bool Fits(this BodyPart from, BodyPart to)
    {
        return from == to || (IsArm(from) && IsArm(to)) || (IsLeg(from) && IsLeg(to));
    }

    private static bool IsArm(BodyPart part) => part == BodyPart.LeftArm || part == BodyPart.RightArm;
    private static bool IsLeg(BodyPart part) => part == BodyPart.LeftLeg || part == BodyPart.RightLeg;
}

public enum Stat
{
    MaxHealth,  // hit points
    MoveSpeed,  // units per second
    Damage      // added to every hit of this character's attacks
}

// One stat change from a limb
[System.Serializable]
public struct StatModifier
{
    public Stat stat;
    public float amount;
}

// Goes on the root of every limb prefab. Says which creature the limb comes
// from, which slot it fits, how it changes stats, and (for arms) which attack it gives.
// Limbs sit inside the body-part sockets on both the player and enemies and Body keeps track of them.
public class Limb : MonoBehaviour
{
    [SerializeField] private LimbKind kind = LimbKind.Lion;
    [SerializeField] private BodyPart slot = BodyPart.None;

    [Tooltip("Stat changes while this limb is equipped. A limb can have several, and negative amounts are drawbacks.")]
    [SerializeField] private StatModifier[] statModifiers = new StatModifier[0];

    [Tooltip("Arms only: the attack prefab this arm uses. Left arm = left click, right arm = right click.")]
    [SerializeField] private LimbAttack attackPrefab;

    [Tooltip("Chest only: the special ability prefab.")]
    [SerializeField] private PlayerSpecial specialPrefab;

    [Tooltip("Legs only: the dash ability prefab.")]
    [SerializeField] private PlayerDash dashPrefab;


    public LimbKind Kind => kind;
    public BodyPart Slot => slot;
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;
    public LimbAttack AttackPrefab => attackPrefab;
    public PlayerSpecial SpecialPrefab => specialPrefab;
    public PlayerDash DashPrefab => dashPrefab;

    public void SetSlot(BodyPart newSlot)
    {
        slot = newSlot;
    }
}

using System.Collections.Generic;
using UnityEngine;

// What the player takes through a SceneDoor: their health, the kind of limb in each slot, and which door
// they come out of. It's static, so it survives loading the next scene, where the scene's own Player is
// set up from it. It stays set until a new game starts, so Try Again restarts a room as it was entered.
public static class PlayerCarryOver
{
    // Legs always match, so the left leg stands for both.
    private static readonly BodyPart[] Slots =
    {
        BodyPart.Head,
        BodyPart.Chest,
        BodyPart.LeftArm,
        BodyPart.RightArm,
        BodyPart.LeftLeg
    };

    private static readonly Dictionary<BodyPart, LimbKind> limbs = new Dictionary<BodyPart, LimbKind>();
    private static float health;

    public static bool HasSnapshot { get; private set; }
    public static string TargetScene { get; private set; }
    public static string TargetDoorId { get; private set; }

    public static void Save(PlayerController player, string targetScene, string targetDoorId)
    {
        limbs.Clear();

        foreach (BodyPart slot in Slots)
        {
            Limb limb = player.Body.GetLimb(slot);

            if (limb != null)
            {
                limbs[slot] = limb.Kind;
            }
        }

        health = player.Health.CurrentHealth;
        TargetScene = targetScene;
        TargetDoorId = targetDoorId;
        HasSnapshot = true;
    }

    // Limbs first, since they can change max health, then health.
    public static void Restore(PlayerController player)
    {
        if (!HasSnapshot)
        {
            return;
        }

        foreach (KeyValuePair<BodyPart, LimbKind> saved in limbs)
        {
            Limb current = player.Body.GetLimb(saved.Key);

            if (current != null && current.Kind == saved.Value)
            {
                continue;
            }

            if (saved.Key == BodyPart.LeftLeg)
            {
                player.EquipLegs(saved.Value);
            }
            else
            {
                player.EquipLimb(saved.Value, saved.Key);
            }
        }

        player.Health.SetCurrentHealth(health);
    }

    public static void Clear()
    {
        limbs.Clear();
        HasSnapshot = false;
        TargetScene = null;
        TargetDoorId = null;
    }

    // Domain reload is off in this project, so statics would otherwise carry over between Play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        Clear();
    }
}

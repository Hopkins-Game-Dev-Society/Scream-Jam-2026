using System.Collections.Generic;
using UnityEngine;


public class LootFigure : MonoBehaviour
{
    [Tooltip("UI pixels per world unit.")]
    [SerializeField] private float pixelsPerUnit = 45f;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.35f);

    private static readonly Dictionary<BodyPart, (Vector2 joint, float angle)> Pose = new Dictionary<BodyPart, (Vector2, float)>
    {
        { BodyPart.Head, (new Vector2(0.19f, 2.6f), 0f) },
        { BodyPart.Chest, (Vector2.zero, 0f) },
        { BodyPart.LeftArm, (new Vector2(-2f, 0.6f), -10f) },
        { BodyPart.RightArm, (new Vector2(2f, 0.6f), 10f) },
        { BodyPart.LeftLeg, (new Vector2(-1.3f, -2.6f), -15f) },
        { BodyPart.RightLeg, (new Vector2(1.3f, -2.6f), 15f) },
    };

    private LootSlot[] slots;

    /*
    public void Show(Body body)
    {
        foreach (LootSlot slot in Slots())
        {
            var (joint, angle) = Pose[slot.BodyPart];
            Limb limb = body.GetLimb(slot.BodyPart);
            LimbVisual visual = limb != null ? limb.GetComponent<LimbVisual>() : null;
            slot.Show(visual, joint * pixelsPerUnit, angle, pixelsPerUnit);
        }
    } */

    public void Show(Body body)
    {
        foreach (LootSlot slot in Slots())
        {
            var (joint, angle) = Pose[slot.BodyPart];

            Limb limb = body.GetLimb(slot.BodyPart);

            LimbVisual visual = limb != null
                ? limb.GetComponentInChildren<LimbVisual>(true)
                : null;

            slot.Show(
                visual,
                joint * pixelsPerUnit,
                angle,
                pixelsPerUnit
            );
        }
    }


    public void Highlight(BodyPart part)
    {
        foreach (LootSlot slot in Slots())
        {
            bool fits = part != BodyPart.None && part.Fits(slot.BodyPart);
            slot.SetTint(fits ? highlightColor : Color.white);
        }
    }

    private LootSlot[] Slots()
    {
        if (slots == null)
        {
            slots = GetComponentsInChildren<LootSlot>(true);
        }

        return slots;
    }
}

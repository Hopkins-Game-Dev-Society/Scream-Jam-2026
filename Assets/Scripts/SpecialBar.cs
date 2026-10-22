using TMPro;
using UnityEngine;

public class SpecialBar : MonoBehaviour
{
    [SerializeField] private Body body;

    [Tooltip("Stretches from the bar's left edge as the special recharges.")]
    [SerializeField] private RectTransform fill;

    [SerializeField] private TMP_Text label;

    private PlayerSpecial currentSpecial;

    private void Update()
    {
        PlayerSpecial equippedSpecial = body.GetSpecial();

        if (equippedSpecial != currentSpecial)
        {
            currentSpecial = equippedSpecial;
        }

        if (currentSpecial == null)
        {
            fill.anchorMax = new Vector2(0f, 1f);

            if (label != null)
            {
                label.text = "SPECIAL UNAVAILABLE";
            }

            return;
        }

        float fraction = currentSpecial.CooldownFill;

        fill.anchorMax = new Vector2(fraction, 1f);

        if (label != null)
        {
            label.text = currentSpecial.IsReady
                ? "SPECIAL READY"
                : "SPECIAL RECHARGING";
        }
    }
}

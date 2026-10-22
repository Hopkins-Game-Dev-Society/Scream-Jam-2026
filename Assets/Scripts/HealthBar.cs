using TMPro;
using UnityEngine;

// A bar and a number showing a character's health, like the player's HUD in the top left.
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;

    [Tooltip("Stretches from the bar's left edge to show how much health is left.")]
    [SerializeField] private RectTransform fill;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        health.OnHealthChanged.AddListener(Show);
        Show(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        health.OnHealthChanged.RemoveListener(Show);
    }

    private void Show(float current, float max)
    {
        float fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        fill.anchorMax = new Vector2(fraction, 1f);
        label.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}

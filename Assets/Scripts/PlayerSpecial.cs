using UnityEngine;
using UnityEngine.Events;

public abstract class PlayerSpecial : MonoBehaviour
{
    [Tooltip("Fired when the special is used. Hook sounds here.")]
    [SerializeField] private UnityEvent onActivated = new UnityEvent();

    public abstract float CooldownDuration { get; }

    private float cooldownStartedAt;
    private bool coolingDown;

    public bool IsReady =>
        !coolingDown || CooldownDuration <= 0f
        || Time.time >= cooldownStartedAt + CooldownDuration;

    // 0 = just activated; 1 = ready to use.
    public float CooldownFill
    {
        get
        {
            if (IsReady)
            {
                return 1f;
            }

            float elapsed = Time.time - cooldownStartedAt;
            return Mathf.Clamp01(elapsed / CooldownDuration);
        }
    }

    public void TryActivate(Vector2 aimDirection)
    {
        if (!IsReady)
        {
            return;
        }

        Activate(aimDirection);
        onActivated.Invoke();

        if (CooldownDuration > 0f)
        {
            cooldownStartedAt = Time.time;
            coolingDown = true;
        }
    }

    public abstract void Activate(Vector2 aimDirection);
}

//Will need some sort of meter to show how ready the special actually is as well.
////Add that to the UI while I also add the functionality to everything 
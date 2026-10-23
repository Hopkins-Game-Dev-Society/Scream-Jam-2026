using UnityEngine;
using UnityEngine.Events;

public abstract class PlayerDash : MonoBehaviour
{
    [Tooltip("Fired when a dash starts. Hook sounds here.")]
    [SerializeField] private UnityEvent onDash = new UnityEvent();

    public abstract float CooldownDuration { get; }

    public abstract void Activate(Vector2 moveDirection);

    // Call when a dash actually starts, after the cooldown and charge checks pass.
    protected void DashStarted()
    {
        onDash.Invoke();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


/*
 *  All of Dash, Special, and attack, should have different functionalities
 *  depending on the currently active limbs so each attack should be coded
 *  and then this basically just picks the one to use.
 */
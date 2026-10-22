using UnityEngine;

public abstract class PlayerDash : MonoBehaviour
{
    public abstract float CooldownDuration { get; }

    public abstract void Activate(Vector2 moveDirection);

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
 *  depending on the currently active limbs; so each attack should be coded
 *  and then this basically just picks the one to use.
 */
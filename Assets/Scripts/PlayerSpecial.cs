using UnityEngine;

public abstract class PlayerSpecial : MonoBehaviour
{
    public abstract float CooldownDuration { get; }

    public abstract void Activate(Vector2 aimDirection);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

//Will need some sort of meter to show how ready the special actually is as well.
////Add that to the UI while I also add the functionality to everything 
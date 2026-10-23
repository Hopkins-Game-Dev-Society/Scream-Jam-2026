using UnityEngine;

// Plays a footstep at a steady rhythm while the player walks. Silent while dashing, dead or paused.
[RequireComponent(typeof(PlayerMovement))]
public class Footsteps : MonoBehaviour
{
    [SerializeField] private RandomSound steps;

    [Tooltip("Seconds between footsteps while walking")]
    [SerializeField] private float stepInterval = 0.3f;

    private PlayerMovement movement;
    private float nextStepTime;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        bool walking = movement.enabled && !movement.IsDashing && movement.MoveInput.sqrMagnitude > 0.01f;

        if (!walking)
        {
            nextStepTime = Time.time;
            return;
        }

        if (Time.time >= nextStepTime)
        {
            steps.Play();
            nextStepTime = Time.time + stepInterval;
        }
    }
}

using UnityEngine;

// Plays one of several clips at a slightly random pitch, so repeated sounds don't feel mechanical.
// Call Play from code or hook it to a UnityEvent like an attack's On Attack Started.
[RequireComponent(typeof(AudioSource))]
public class RandomSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips = new AudioClip[0];

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Tooltip("Each play picks a pitch between these two values. Use 1 and 1 for no variation.")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private AudioSource source;
    private int lastIndex = -1;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (clips.Length == 0)
        {
            return;
        }

        // Never the same clip twice in a row when there's a choice.
        int index = Random.Range(0, clips.Length);

        if (clips.Length > 1 && index == lastIndex)
        {
            index = (index + 1) % clips.Length;
        }

        lastIndex = index;
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clips[index], volume);
    }
}

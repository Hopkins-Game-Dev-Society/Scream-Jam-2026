
using UnityEngine;

/// <summary>
/// Mirrors the Animator's local Z rotation for a mirrored limb.
/// The Animator must animate this GameObject's local Z rotation.
/// </summary>
public class MirroredLimbRotation : MonoBehaviour
{
    [SerializeField] private LimbVisual limbVisual;
    [SerializeField] private Animator animator;

    private Quaternion previousRotation;
    private Quaternion correction = Quaternion.identity;

    private void Awake()
    {
        if (limbVisual == null)
        {
            limbVisual = GetComponent<LimbVisual>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void LateUpdate()
    {
        if (limbVisual == null || animator == null ||
            !limbVisual.Mirrored)
        {
            return;
        }

        // The Animator has already written this frame's rotation.
        Vector3 angles = transform.localEulerAngles;

        // Normalize Z to the signed range [-180, 180].
        float z = Mathf.DeltaAngle(0f, angles.z);

        transform.localRotation =
            Quaternion.Euler(angles.x, angles.y, -z);
    }
}

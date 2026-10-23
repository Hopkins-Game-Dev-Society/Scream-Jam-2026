
using UnityEngine;

[DefaultExecutionOrder(100)]
public class MirroredLimbRotation : MonoBehaviour
{
    [SerializeField] private LimbVisual limbVisual;

    private void Awake()
    {
        if (limbVisual == null)
        {
            limbVisual = GetComponent<LimbVisual>();
        }
    }

    private void LateUpdate()
    {
        if (limbVisual == null)
        {
            return;
        }

        Vector3 scale = transform.localScale;

        // Mirrored limbs must always have a negative X scale.
        if (limbVisual.Mirrored)
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = Mathf.Abs(scale.x);
        }

        transform.localScale = scale;

        if (!limbVisual.Mirrored)
        {
            return;
        }

        // Reverse the Animator's local Z rotation.
        Vector3 angles = transform.localEulerAngles;
        float signedZ = Mathf.DeltaAngle(0f, angles.z);
        angles.z = -signedZ;

        transform.localRotation = Quaternion.Euler(angles);
    }
}


/*
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

    Vector3 angles = transform.localEulerAngles;

    // Convert Unity's 0–360 degree representation to -180–180.
    float signedZ = Mathf.DeltaAngle(0f, angles.z);

    // Reverse only the Z rotation for the mirrored arm.
    angles.z = -signedZ;

    transform.localRotation = Quaternion.Euler(angles);
}

}
*/
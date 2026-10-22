
using UnityEngine;

public abstract class LimbAttack : MonoBehaviour
{
    public abstract bool IsAttacking { get;  }

    public abstract bool TryAttack(Vector2 direction);
}

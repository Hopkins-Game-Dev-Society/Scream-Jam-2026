using UnityEngine;

// Shows a limb's sprite for the direction its body is facing. Goes on the root of each limb prefab.
// Draw side sprites facing right. they're mirrored when the body faces left.
public class LimbVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite front;  // facing down
    [SerializeField] private Sprite back;   // facing up
    [SerializeField] private Sprite side;   // facing right

    [Tooltip("Tick on left-side limbs, like a variant of the right arm, so their front and back views are mirrored.")]
    [SerializeField] private bool mirrored;

    public Sprite Front => front;
    public bool Mirrored => mirrored;

    // Where the sprite sits relative to the limb's joint, before any mirroring.
    public Vector2 SpriteOffset => spriteRenderer.transform.localPosition;

    public void SetFacing(Facing facing)
    {
        switch (facing)
        {
            case Facing.Down:
                Show(front, mirrored);
                break;
            case Facing.Up:
                Show(back, mirrored);
                break;
            case Facing.Right:
                Show(side, false);
                break;
            case Facing.Left:
                Show(side, true);
                break;
        }
    }

    private void Show(Sprite sprite, bool mirror)
    {
        spriteRenderer.sprite = sprite;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (mirror ? -1f : 1f);
        transform.localScale = scale;
    }
}
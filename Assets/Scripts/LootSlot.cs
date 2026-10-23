using UnityEngine;
using UnityEngine.EventSystems;


public class LootSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private BodyPart bodyPart;
    [SerializeField] private UnityEngine.UI.Image part;

    private LootMenu menu;
    private LootFigure figure;

    public BodyPart BodyPart => bodyPart;
    public LootFigure Figure => figure != null ? figure : figure = GetComponentInParent<LootFigure>(true);
    public bool HasPart => part.enabled;
    public Sprite Sprite => part.sprite;
    public Vector2 PartSize => part.rectTransform.sizeDelta;
    public bool Mirrored { get; private set; }

    private LootMenu Menu => menu != null ? menu : menu = GetComponentInParent<LootMenu>(true);

    // Places the limb's front sprite around this joint. A null visual leaves the slot empty.
    /*
    public void Show(LimbVisual visual, Vector2 jointPosition, float angle, float pixelsPerUnit)
    {
        part.enabled = visual != null && visual.Front != null;

        if (!part.enabled)
        {
            return;
        }

        Mirrored = visual.Mirrored;

        var joint = (RectTransform)transform;
        joint.anchoredPosition = jointPosition;
        joint.localRotation = Quaternion.Euler(0f, 0f, angle);
        joint.localScale = new Vector3(Mirrored ? -1f : 1f, 1f, 1f);

        part.sprite = visual.Front;
        part.rectTransform.anchoredPosition = visual.SpriteOffset * pixelsPerUnit;
        part.rectTransform.sizeDelta = (Vector2)visual.Front.bounds.size * pixelsPerUnit;
        part.color = Color.white;
    } */

    /*
    public void Show(
    LimbVisual visual,
    Vector2 jointPosition,
    float angle,
    float pixelsPerUnit)
    {
        // Clear any previous visual state.
        part.enabled = false;
        part.sprite = null;
        part.color = Color.white;

        Mirrored = false;

        if (visual == null || visual.Front == null)
        {
            return;
        }

        Mirrored = visual.Mirrored;

        var joint = (RectTransform)transform;
        joint.anchoredPosition = jointPosition;
        joint.localRotation = Quaternion.Euler(0f, 0f, angle);
        joint.localScale = new Vector3(
            Mirrored ? -1f : 1f,
            1f,
            1f
        );

        part.sprite = visual.Front;
        part.rectTransform.anchoredPosition =
            visual.SpriteOffset * pixelsPerUnit;
        part.rectTransform.sizeDelta =
            (Vector2)visual.Front.bounds.size * pixelsPerUnit;

        part.enabled = true;
    }*/


public void Show(
    LimbVisual visual,
    Vector2 jointPosition,
    float angle,
    float pixelsPerUnit)
{
    part.enabled = false;
    part.sprite = null;
    part.color = Color.white;
    Mirrored = false;

    if (visual == null || visual.Front == null)
    {
        return;
    }

    bool flipForLootMenu =
        bodyPart == BodyPart.LeftArm ||
        bodyPart == BodyPart.RightArm ||
        bodyPart == BodyPart.LeftLeg ||
        bodyPart == BodyPart.RightLeg;

    Mirrored = visual.Mirrored ^ flipForLootMenu;

    RectTransform joint = (RectTransform)transform;
    joint.anchoredPosition = jointPosition;
    joint.localRotation = Quaternion.Euler(0f, 0f, angle);

    // Keep the joint itself unflipped so its position stays correct.
    joint.localScale = Vector3.one;

    part.sprite = visual.Front;

    RectTransform partTransform = part.rectTransform;
    //partTransform.anchoredPosition =
        //visual.SpriteOffset * pixelsPerUnit;
    Vector2 spriteOffset = visual.SpriteOffset;

    if (bodyPart == BodyPart.LeftArm)
    {
        spriteOffset.x = -spriteOffset.x;
    }

    partTransform.anchoredPosition = spriteOffset * pixelsPerUnit;

    partTransform.sizeDelta =
        (Vector2)visual.Front.bounds.size * pixelsPerUnit;

    // Apply mirroring to the sprite instead of the joint.
    partTransform.localScale = new Vector3(
        Mirrored ? -1f : 1f,
        1f,
        1f
    );

    part.enabled = true;
}

    public void SetTint(Color color)
    {
        part.color = color;
    }

    public void OnBeginDrag(PointerEventData eventData) => Menu.BeginDrag(this);
    public void OnDrag(PointerEventData eventData) => Menu.Drag(eventData);
    public void OnEndDrag(PointerEventData eventData) => Menu.EndDrag();
    public void OnDrop(PointerEventData eventData) => Menu.Drop(this);
}

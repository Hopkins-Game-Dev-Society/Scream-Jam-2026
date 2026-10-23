using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;


public class LootMenu : MonoBehaviour
{
    [SerializeField] private GameObject window;
    [SerializeField] private GameObject promptBox;
    [SerializeField] private TMPro.TMP_Text promptText;
    [SerializeField] private LootFigure playerFigure;
    [SerializeField] private LootFigure corpseFigure;
    [SerializeField] private UnityEngine.UI.Image dragGhost;
    [SerializeField] private UnityEngine.UI.Button closeButton;

    [Header("Events")]
    [Tooltip("Fired when the menu opens. Hook sounds here.")]
    [SerializeField] private UnityEvent onOpened = new UnityEvent();
    [Tooltip("Fired when the menu closes.")]
    [SerializeField] private UnityEvent onClosed = new UnityEvent();
    [Tooltip("Fired when a limb is picked up off the corpse.")]
    [SerializeField] private UnityEvent onLimbPickedUp = new UnityEvent();
    [Tooltip("Fired when a limb is swapped onto the player.")]
    [SerializeField] private UnityEvent onLimbSwapped = new UnityEvent();

    private PlayerController player;
    private Body corpse;
    private PlayerInput playerInput;
    private InputAction cancel;
    private string previousActionMap;
    private float previousTimeScale = 1f;
    private LootSlot dragging;

    public bool IsOpen => window.activeSelf;

    private void Awake()
    {
        window.SetActive(false);
        dragGhost.gameObject.SetActive(false);
        promptBox.SetActive(false);
        closeButton.onClick.AddListener(Close);
        EnsureEventSystem();
    }

    private void Update()
    {
        if (IsOpen && cancel != null && cancel.WasPressedThisFrame())
        {
            Close();
        }
    }

    public void ShowPrompt(string text)
    {
        bool show = !string.IsNullOrEmpty(text);
        promptBox.SetActive(show);

        if (show)
        {
            promptText.text = text;
        }
    }

    public void Open(PlayerController player, Body corpse)
    {
        if (IsOpen)
        {
            return;
        }

        this.player = player;
        this.corpse = corpse;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        playerInput = player.GetComponent<PlayerInput>();
        previousActionMap = playerInput.currentActionMap?.name;
        playerInput.SwitchCurrentActionMap("UI");
        cancel = playerInput.actions.FindAction("UI/Cancel");

        ShowPrompt(null);
        window.SetActive(true);
        Refresh();
        onOpened.Invoke();
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        EndDrag();
        window.SetActive(false);
        Time.timeScale = previousTimeScale;

        if (playerInput != null && !string.IsNullOrEmpty(previousActionMap))
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);
        }

        player = null;
        corpse = null;
        cancel = null;
        onClosed.Invoke();
    }

    public void BeginDrag(LootSlot slot)
    {
        if (slot.Figure != corpseFigure || !slot.HasPart)
        {
            return;
        }

        dragging = slot;
        dragGhost.sprite = slot.Sprite;
        dragGhost.rectTransform.sizeDelta = slot.PartSize;
        dragGhost.rectTransform.localScale = new Vector3(slot.Mirrored ? -1f : 1f, 1f, 1f);
        dragGhost.gameObject.SetActive(true);
        playerFigure.Highlight(slot.BodyPart);
        onLimbPickedUp.Invoke();
    }

    public void Drag(PointerEventData eventData)
    {
        if (dragging == null)
        {
            return;
        }

        var area = (RectTransform)dragGhost.transform.parent;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out Vector2 point);
        dragGhost.rectTransform.anchoredPosition = point;
    }

    public void EndDrag()
    {
        dragging = null;
        dragGhost.gameObject.SetActive(false);
        playerFigure.Highlight(BodyPart.None);
    }

    public void Drop(LootSlot target)
    {
        if (dragging == null || target.Figure != playerFigure || !dragging.BodyPart.Fits(target.BodyPart))
        {
            return;
        }

        if (player.SwapLimbWith(corpse, dragging.BodyPart, target.BodyPart))
        {
            Refresh();
            onLimbSwapped.Invoke();
        }
    }

    private void Refresh()
    {
        playerFigure.Show(player.Body);
        corpseFigure.Show(corpse);
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }
}

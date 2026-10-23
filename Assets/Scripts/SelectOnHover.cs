using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Selects this button while the mouse is over it
[RequireComponent(typeof(Selectable))]
public class SelectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
{
    private Selectable selectable;
    private bool selectedByHover;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (EventSystem.current != null && selectable.IsInteractable())
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            selectedByHover = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectedByHover && EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selectedByHover = false;
    }
}

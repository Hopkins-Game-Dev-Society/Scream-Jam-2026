using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Brings the highlight back when the player navigates with the keyboard or a gamepad and nothing
// is selected, like after the mouse moves off a button. The first press only shows the highlight.
// Runs after the EventSystem so that same press doesn't also move the selection.
[DefaultExecutionOrder(100)]
public class MenuKeyboardFocus : MonoBehaviour
{
    [Tooltip("Selected the first time the player navigates, before anything else has been selected.")]
    [SerializeField] private Selectable firstSelected;

    private GameObject lastSelected;

    private void Update()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            return;
        }

        GameObject selected = eventSystem.currentSelectedGameObject;

        if (selected != null)
        {
            lastSelected = selected;
            return;
        }

        if (eventSystem.currentInputModule is InputSystemUIInputModule module
            && module.move != null && module.move.action.WasPressedThisFrame())
        {
            GameObject target = lastSelected != null && lastSelected.activeInHierarchy
                ? lastSelected
                : firstSelected != null ? firstSelected.gameObject : null;

            eventSystem.SetSelectedGameObject(target);
        }
    }
}

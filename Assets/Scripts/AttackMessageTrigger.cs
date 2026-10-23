
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class AttackMessageTrigger : MonoBehaviour
{
    [Header("Message UI")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;

    [TextArea]
    [SerializeField] private string message =
        "You have discovered something!";

    [Header("Trigger Options")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool pauseGame = true;

    private bool hasTriggered;
    private bool waitingForInput;

    private void Awake()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!waitingForInput)
        {
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            ResumeGame();
            return;
        }

        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
        {
            ResumeGame();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        hasTriggered = true;
        ShowMessage();
    }

    private void ShowMessage()
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(true);
        }

        if (pauseGame)
        {
            Time.timeScale = 0f;
            waitingForInput = true;
        }
    }

    public void Activate()
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        hasTriggered = true;
        ShowMessage();
    }

    private void ResumeGame()
    {
        waitingForInput = false;
        Time.timeScale = 1f;

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    public void CloseMessage()
    {
        ResumeGame();
    }

    private void OnDisable()
    {
        if (waitingForInput)
        {
            waitingForInput = false;
            Time.timeScale = 1f;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// The game over overlay. Put the prefab in a level and it stays hidden until the player dies, then fades in
// over the frozen game. Try Again restarts the level and Main Menu goes back to the title screen.
public class GameOverScreen : MonoBehaviour
{
    [Tooltip("The player whose death shows the overlay")]
    [SerializeField] private Health player;

    [Tooltip("Everything the overlay shows")]
    [SerializeField] private CanvasGroup window;

    [Tooltip("Seconds between the killing blow and the overlay appearing.")]
    [SerializeField] private float showDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.75f;

    [Tooltip("Scene Main Menu loads")]
    [SerializeField] private string menuScene = "TitleScreen";

    [Header("Sound")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickSound;

    private bool leaving;

    private void Awake()
    {
        window.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (player == null)
        {
            PlayerController found = FindAnyObjectByType<PlayerController>();

            if (found != null)
            {
                player = found.Health;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("GameOverScreen: no player found so the overlay will never show.", this);
            return;
        }

        player.OnDied.AddListener(HandlePlayerDied);
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnDied.RemoveListener(HandlePlayerDied);
        }
    }

    public void TryAgain()
    {
        if (!leaving)
        {
            StartCoroutine(Leave(ReloadLevel));
        }
    }

    public void MainMenu()
    {
        if (!leaving)
        {
            StartCoroutine(Leave(() => SceneManager.LoadScene(menuScene)));
        }
    }

    private void HandlePlayerDied(DamageInfo killingBlow)
    {
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        yield return new WaitForSeconds(showDelay);

        // Freeze the level behind the overlay. Leave() unfreezes it.
        Time.timeScale = 0f;

        window.alpha = 0f;
        window.interactable = false;
        window.gameObject.SetActive(true);

        for (float time = 0f; time < fadeDuration; time += Time.unscaledDeltaTime)
        {
            window.alpha = time / fadeDuration;
            yield return null;
        }

        window.alpha = 1f;
        window.interactable = true;
    }

    // Plays the click and waits for it to finish so leaving the scene doesn't cut it off
    private IEnumerator Leave(System.Action load)
    {
        leaving = true;

        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
            yield return new WaitForSecondsRealtime(clickSound.length);
        }

        Time.timeScale = 1f;
        load();
    }

    private static void ReloadLevel()
    {
        Scene level = SceneManager.GetActiveScene();

#if UNITY_EDITOR
        if (level.buildIndex < 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                level.path, new LoadSceneParameters(LoadSceneMode.Single));
            return;
        }
#endif

        SceneManager.LoadScene(level.buildIndex);
    }
}

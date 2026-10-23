using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu. Play loads the game scene and Quit closes the game.
public class TitleScreen : MonoBehaviour
{
    [Tooltip("Scene the Play button loads. It must be in the Build Profile's scene list.")]
    [SerializeField] private string gameScene = "Brady_sandbox_3";

    [Tooltip("Hidden in web builds where the game can't close the browser tab.")]
    [SerializeField] private GameObject quitButton;

    [Header("Sound")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip clickSound;

    private bool leaving;

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer && quitButton != null)
        {
            quitButton.SetActive(false);
        }
    }

    public void Play()
    {
        if (leaving)
        {
            return;
        }

        leaving = true;
        PlayerCarryOver.Clear(); // a new game starts fresh, not with the last run's health and limbs
        StartCoroutine(LoadGame());
    }

    public void Quit()
    {
        if (leaving)
        {
            return;
        }

        leaving = true;
        StartCoroutine(QuitGame());
    }

    private IEnumerator LoadGame()
    {
        yield return PlayClick();
        SceneManager.LoadScene(gameScene);
    }

    private IEnumerator QuitGame()
    {
        yield return PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator PlayClick()
    {
        if (sfxSource == null || clickSound == null)
        {
            yield break;
        }

        sfxSource.PlayOneShot(clickSound);
        yield return new WaitForSecondsRealtime(clickSound.length);
    }
}

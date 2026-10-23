using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// A doorway to another scene. It stays locked until every enemy in the scene is dead. Walking into it then
// saves the player's health and limbs and loads the target scene, where the player comes out at the door
// whose Door ID matches this door's Target Door ID.
[RequireComponent(typeof(Collider2D))]
public class SceneDoor : MonoBehaviour
{
    [Tooltip("This door's name. Doors in other scenes lead here by it.")]
    [SerializeField] private string doorId = "Door";

    [Tooltip("Scene this door leads to. It must be in the Build Profile's scene listz")]
    [SerializeField] private string targetScene;

    [Tooltip("Door ID of the door in the target scene that the player comes out of.")]
    [SerializeField] private string targetDoorId;

    [Tooltip("Where the player appears when arriving through this door. Keep it outside the door's trigger.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Shown while the door is locked, like a gate sprite. Optional.")]
    [SerializeField] private GameObject lockedVisual;

    [Header("Events")]
    [Tooltip("Fired when the last enemy dies and the door unlocks. Hook sounds here.")]
    [SerializeField] private UnityEvent onUnlocked = new UnityEvent();

    [Tooltip("Fired when the player arrives in this scene through this door.")]
    [SerializeField] private UnityEvent onPlayerArrived = new UnityEvent();

    private const float ArrivalGrace = 0.5f;

    private static bool loading;

    private Health[] enemies = new Health[0];
    private bool unlocked;

    public bool IsUnlocked => unlocked;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        Enemy[] found = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        enemies = new Health[found.Length];

        for (int i = 0; i < found.Length; i++)
        {
            enemies[i] = found[i].GetComponent<Health>();
            enemies[i].OnDied.AddListener(HandleEnemyDied);
        }

        unlocked = AllEnemiesDead();

        if (lockedVisual != null)
        {
            lockedVisual.SetActive(!unlocked);
        }
    }

    private void OnDestroy()
    {
        foreach (Health enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.OnDied.RemoveListener(HandleEnemyDied);
            }
        }
    }

    private void HandleEnemyDied(DamageInfo killingBlow)
    {
        if (unlocked || !AllEnemiesDead())
        {
            return;
        }

        unlocked = true;

        if (lockedVisual != null)
        {
            lockedVisual.SetActive(false);
        }

        onUnlocked.Invoke();
    }

    private bool AllEnemiesDead()
    {
        foreach (Health enemy in enemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                return false;
            }
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryUse(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryUse(other);
    }

    private void TryUse(Collider2D other)
    {
        if (!unlocked || loading || Time.timeSinceLevelLoad < ArrivalGrace || string.IsNullOrEmpty(targetScene))
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player == null || player.IsDead)
        {
            return;
        }

        PlayerCarryOver.Save(player, targetScene, targetDoorId);
        loading = true;
        LoadScene(targetScene);
    }

    private void Receive(PlayerController player)
    {
        Vector2 position = spawnPoint != null ? spawnPoint.position : transform.position;
        player.GetComponent<Rigidbody2D>().position = position;
        player.transform.position = position;
        onPlayerArrived.Invoke();
    }

    private static void LoadScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

#if UNITY_EDITOR
        // Test scenes that aren't in the build list can still be loaded in the Editor.
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Scene " + sceneName))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

            if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                    path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
        }
#endif

        loading = false;
        Debug.LogError($"SceneDoor: can't load '{sceneName}'. Add it to the Build Profile's scene list.");
    }

    // Domain reload is off in this project, so the static state and the subscription are reset each Play session.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        loading = false;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    // Runs once the new scene's objects are awake but before their Start, so the player is set up before
    // anything else reads their health or limbs.
    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loading = false;

        if (!PlayerCarryOver.HasSnapshot || scene.name != PlayerCarryOver.TargetScene)
        {
            return;
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();

        if (player == null)
        {
            return;
        }

        PlayerCarryOver.Restore(player);

        foreach (SceneDoor door in FindObjectsByType<SceneDoor>(FindObjectsSortMode.None))
        {
            if (door.doorId == PlayerCarryOver.TargetDoorId)
            {
                door.Receive(player);
                return;
            }
        }

        Debug.LogWarning($"SceneDoor: no door with ID '{PlayerCarryOver.TargetDoorId}' in {scene.name}, so the player starts where the scene puts them.");
    }

    private void OnDrawGizmos()
    {
        Collider2D area = GetComponent<Collider2D>();

        if (area == null)
        {
            return;
        }

        Gizmos.color = Application.isPlaying && unlocked ? Color.green : new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);

        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(area.bounds.center, spawnPoint.position);
            Gizmos.DrawWireSphere(spawnPoint.position, 2f);
        }
    }
}

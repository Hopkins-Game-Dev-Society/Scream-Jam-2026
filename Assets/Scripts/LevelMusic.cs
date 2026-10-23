using UnityEngine;
using UnityEngine.Events;

public class LevelMusic : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource calmMusic;
    [SerializeField] private AudioSource combatMusic;
    [SerializeField] private AudioSource gameOverMusic;
    [SerializeField] private AudioSource ambience;

    [Tooltip("Volume of whichever music loop is playing.")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Tooltip("Seconds to crossfade between calm and combat music.")]
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Tooltip("Seconds combat music continues after enemies stop chasing.")]
    [SerializeField] private float combatHoldTime = 3f;

    [Tooltip("Seconds to fade music out after the player dies.")]
    [SerializeField] private float deathFadeDuration = 1f;

    [Tooltip("Seconds to fade into game-over music.")]
    [SerializeField] private float gameOverFadeDuration = 1f;

    [Header("Game Over Detection")]
    [Tooltip("Optional: assign the game-over UI panel here.")]
    [SerializeField] private GameObject gameOverPanel;

    private Enemy[] enemies;
    private Health[] enemyHealth;
    private Health player;

    private float ambienceVolume;
    private float combatUntil;
    private float combatMix;
    private float gameOverMix;
    private float fade = 1f;

    private bool playerDead;
    private bool gameOver;

    private void Start()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        enemyHealth = new Health[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            enemyHealth[i] = enemies[i] != null
                ? enemies[i].GetComponent<Health>()
                : null;
        }

        PlayerController found =
            FindAnyObjectByType<PlayerController>();

        if (found != null)
        {
            player = found.Health;

            if (player != null)
            {
                player.OnDied.AddListener(HandlePlayerDied);
            }
        }

        ambienceVolume = ambience != null ? ambience.volume : 0f;

        if (gameOverMusic != null)
        {
            gameOverMusic.volume = 0f;

            if (!gameOverMusic.isPlaying)
            {
                gameOverMusic.Play();
            }
        }

        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnDied.RemoveListener(HandlePlayerDied);
        }
    }

    private void Update()
    {
        // Detect the game-over UI even if death was handled elsewhere.
        if (gameOverPanel != null && gameOverPanel.activeInHierarchy)
        {
            gameOver = true;
        }

        if (!gameOver && !playerDead && AnyEnemyChasing())
        {
            combatUntil = Time.unscaledTime + combatHoldTime;
        }

        if (!gameOver)
        {
            float target =
                Time.unscaledTime < combatUntil ? 1f : 0f;

            float duration = Mathf.Max(0.01f, crossfadeDuration);

            combatMix = Mathf.MoveTowards(
                combatMix,
                target,
                Time.unscaledDeltaTime / duration
            );
        }

        if (playerDead || gameOver)
        {
            float duration = Mathf.Max(0.01f, deathFadeDuration);

            fade = Mathf.MoveTowards(
                fade,
                0f,
                Time.unscaledDeltaTime / duration
            );

            float gameOverDuration =
                Mathf.Max(0.01f, gameOverFadeDuration);

            gameOverMix = Mathf.MoveTowards(
                gameOverMix,
                1f,
                Time.unscaledDeltaTime / gameOverDuration
            );
        }

        ApplyVolumes();
    }

    private bool AnyEnemyChasing()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null &&
                enemies[i].IsChasing &&
                (enemyHealth[i] == null || !enemyHealth[i].IsDead))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyVolumes()
    {
        float normalMusicVolume = musicVolume * fade;
        float gameOverVolume = musicVolume * gameOverMix;

        if (calmMusic != null)
        {
            calmMusic.volume =
                normalMusicVolume * (1f - combatMix);
        }

        if (combatMusic != null)
        {
            combatMusic.volume =
                normalMusicVolume * combatMix;
        }

        if (gameOverMusic != null)
        {
            gameOverMusic.volume = gameOverVolume;
        }

        if (ambience != null)
        {
            ambience.volume = ambienceVolume * fade;
        }
    }

    private void HandlePlayerDied(DamageInfo killingBlow)
    {
        playerDead = true;
        gameOver = true;
    }

    // Call this from your game-over UI controller if needed.
    public void SetGameOver()
    {
        gameOver = true;
    }
}

/*
using UnityEngine;

// Plays the calm loop, crossfades to the combat loop while any enemy is
// chasing the player, and fades everything out when the player dies.
public class LevelMusic : MonoBehaviour
{
    [SerializeField] private AudioSource calmMusic;
    [SerializeField] private AudioSource combatMusic;
    [SerializeField] private AudioSource ambience;

    [Tooltip("Volume of whichever music loop is playing. The ambience keeps its own AudioSource volume.")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Tooltip("Seconds to crossfade between the calm and combat loops.")]
    [SerializeField] private float crossfadeDuration = 1.5f;

    [Tooltip("Seconds the combat loop keeps going after the last enemy gives up the chase.")]
    [SerializeField] private float combatHoldTime = 3f;

    [Tooltip("Seconds to fade everything out after the player dies.")]
    [SerializeField] private float deathFadeDuration = 1f;

    private Enemy[] enemies;
    private Health[] enemyHealth;
    private Health player;
    private float ambienceVolume;
    private float combatUntil;
    private float combatMix;    // 0 = calm loop, 1 = combat loop
    private float fade = 1f;    // drops to 0 after the player dies
    private bool playerDead;

    private void Start()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        enemyHealth = new Health[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            enemyHealth[i] = enemies[i].GetComponent<Health>();
        }

        PlayerController found = FindAnyObjectByType<PlayerController>();

        if (found != null)
        {
            player = found.Health;
            player.OnDied.AddListener(HandlePlayerDied);
        }

        ambienceVolume = ambience != null ? ambience.volume : 0f;
        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnDied.RemoveListener(HandlePlayerDied);
        }
    }

    // Unscaled time, so the fades still run while the game over screen has the game frozen.
    private void Update()
    {
        if (AnyEnemyChasing())
        {
            combatUntil = Time.unscaledTime + combatHoldTime;
        }

        float target = Time.unscaledTime < combatUntil ? 1f : 0f;
        combatMix = Mathf.MoveTowards(combatMix, target, Time.unscaledDeltaTime / crossfadeDuration);

        if (playerDead)
        {
            fade = Mathf.MoveTowards(fade, 0f, Time.unscaledDeltaTime / deathFadeDuration);
        }

        ApplyVolumes();
    }

    private bool AnyEnemyChasing()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            // Dead enemies stay in the scene as corpses and keep whatever state they died in.
            if (enemies[i] != null && enemies[i].IsChasing
                && (enemyHealth[i] == null || !enemyHealth[i].IsDead))
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyVolumes()
    {
        calmMusic.volume = musicVolume * (1f - combatMix) * fade;
        combatMusic.volume = musicVolume * combatMix * fade;

        if (ambience != null)
        {
            ambience.volume = ambienceVolume * fade;
        }
    }

    private void HandlePlayerDied(DamageInfo killingBlow)
    {
        playerDead = true;
    }
}
 */
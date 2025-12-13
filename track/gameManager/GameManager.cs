using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ------------------------------------
    [Header("Checkpoints du circuit")]
    [Tooltip("Liste ordonnée des checkpoints (0 = ligne de départ, 1 = premier checkpoint, etc.)")]
    public Transform[] checkpoints;

    // ------------------------------------
    [Header("UI - Chronomètre")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI lapCountText;
    [SerializeField] private TextMeshProUGUI bestLapText;

    // ------------------------------------
    [Header("UI - Compte à rebours")]
    [SerializeField] private TextMeshProUGUI countdownText;

    // ------------------------------------
    [Header("Ghost Reference")]
    [SerializeField] private GhostController ghostController;

    // ------------------------------------
    [Header("Settings")]
    [SerializeField] private int totalLaps = 3;
    [SerializeField] private bool showDebugLogs = true;

    private bool raceStarted = false;
    private bool raceActive = false;
    private PlayerMovement playerMovement;

    private float totalTime = 0f;
    private float currentLapTime = 0f;
    private float bestLapTime = Mathf.Infinity;
    private int currentLap = 0;

    // ------------------------------------
    [Header("Coin")]
    public TextMeshProUGUI coinText;
    private int coinCount = 0;

    [Header("UI - Respawn")]
    [SerializeField] private TextMeshProUGUI respawnText;
    [SerializeField] private int respawnCountdownSeconds = 3;

    private bool respawnInProgress = false;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ValidateCheckpoints();
        UpdateTimerUI();
        UpdateCoinGUI();

        if (countdownText != null)
        {
            countdownText.text = "Franchissez la ligne de départ pour commencer";
            countdownText.color = Color.white;
        }

        // Trouver le ghost automatiquement si non assigné
        if (ghostController == null)
        {
            ghostController = FindObjectOfType<GhostController>();
        }
    }

    void ValidateCheckpoints()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogError("❌ ERREUR : Aucun checkpoint assigné dans le GameManager !");
            return;
        }

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null)
            {
                Debug.LogError($"❌ ERREUR : Le checkpoint {i} n'est pas assigné !");
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"✓ GameManager initialisé avec {checkpoints.Length} checkpoints");

            for (int i = 0; i < checkpoints.Length; i++)
            {
                Debug.Log($"  → Checkpoint {i}: {checkpoints[i].name}");
            }
        }
    }

    void Update()
    {
        if (raceActive)
        {
            totalTime += Time.deltaTime;
            currentLapTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // Ligne de départ (trigger sur le GameObject avec le GameManager)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!raceStarted)
            {
                playerMovement = other.GetComponent<PlayerMovement>();
                StartRaceImmediately();
            }
            else if (raceActive)
            {
                CompleteLap();
            }
        }
    }

    void StartRaceImmediately()
    {
        raceStarted = true;
        raceActive = true;

        currentLap = 1;
        totalTime = 0f;
        currentLapTime = 0f;

        if (playerMovement != null)
            playerMovement.enabled = true;

        // DÉMARRER LE GHOST en même temps que le joueur
        if (ghostController != null)
        {
            ghostController.StartRace();
            if (showDebugLogs)
                Debug.Log("✅ Ghost démarré en même temps que le joueur !");
        }

        if (showDebugLogs)
            Debug.Log("✅ Course démarrée !");
    }

    void CompleteLap()
    {
        if (showDebugLogs)
            Debug.Log($"🏁 Tour {currentLap} terminé en {FormatTime(currentLapTime)}");

        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
            if (showDebugLogs)
                Debug.Log($"⭐ Nouveau meilleur tour : {FormatTime(bestLapTime)}");
        }

        currentLap++;

        if (currentLap > totalLaps)
            FinishRace();
        else
        {
            currentLapTime = 0f;
            UpdateTimerUI();
        }
    }

    void FinishRace()
    {
        raceActive = false;

        if (showDebugLogs)
        {
            Debug.Log("🏆 Course terminée !");
            Debug.Log($"⏱️ Temps total : {FormatTime(totalTime)}");
            Debug.Log($"⭐ Meilleur tour : {FormatTime(bestLapTime)}");
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (raceActive)
                timerText.text = "Time: " + FormatTime(totalTime);
            else
                timerText.text = "Time: --:--:---";
        }

        if (lapCountText != null)
        {
            if (raceActive)
                lapCountText.text = $"Lap: {currentLap}/{totalLaps}";
            else
                lapCountText.text = $"Lap: 0/{totalLaps}";
        }

        if (bestLapText != null)
        {
            if (bestLapTime < Mathf.Infinity)
                bestLapText.text = "Best: " + FormatTime(bestLapTime);
            else
                bestLapText.text = "Best: --:--:--";
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);

        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }

    public void RestartRace()
    {
        raceStarted = false;
        raceActive = false;

        currentLap = 0;
        totalTime = 0f;
        currentLapTime = 0f;
        bestLapTime = Mathf.Infinity;

        // Arrêter le ghost
        if (ghostController != null)
        {
            ghostController.StopRace();
        }
        UpdateTimerUI();

        if (showDebugLogs)
            Debug.Log("🔄 Course réinitialisée");
    }

    // ------------------------------------
    // MÉTHODES POUR LES COINS
    // ------------------------------------
    public void AddCoin()
    {
        coinCount++;
        UpdateCoinGUI();
    }

    void UpdateCoinGUI()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coinCount.ToString();
    }

    // ------------------------------------
    // MÉTHODES POUR LES CHECKPOINTS
    // ------------------------------------
    public Vector3 GetCheckpointPosition(int index)
    {
        if (index < 0 || index >= checkpoints.Length)
        {
            Debug.LogWarning($"⚠️ Index checkpoint invalide : {index}");
            return Vector3.zero;
        }
        return checkpoints[index].position;
    }

    public Quaternion GetCheckpointRotation(int index)
    {
        if (index < 0 || index >= checkpoints.Length)
        {
            Debug.LogWarning($"⚠️ Index checkpoint invalide : {index}");
            return Quaternion.identity;
        }
        return checkpoints[index].rotation;
    }

    public Transform GetCheckpoint(int index)
    {
        if (index < 0 || index >= checkpoints.Length)
        {
            Debug.LogWarning($"⚠️ Index checkpoint invalide : {index}");
            return null;
        }
        return checkpoints[index];
    }

    public int GetCheckpointCount()
    {
        return checkpoints != null ? checkpoints.Length : 0;
    }

    // ------------------------------------
    // GETTERS
    // ------------------------------------
    public float GetTotalTime() => totalTime;
    public float GetCurrentLapTime() => currentLapTime;
    public float GetBestLapTime() => bestLapTime;
    public int GetCurrentLap() => currentLap;
    public bool IsRaceActive() => raceActive;
    public int GetCoinCount() => coinCount;

    // ------------------------------------
    // GIZMOS
    // ------------------------------------
    void OnDrawGizmos()
    {
        if (checkpoints == null || checkpoints.Length == 0) return;

        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null) continue;

            int nextIndex = (i + 1) % checkpoints.Length;
            if (checkpoints[nextIndex] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(checkpoints[i].position, checkpoints[nextIndex].position);
            }

            if (i == 0)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawSphere(checkpoints[i].position, 1f);
        }
    }
    public void RequestPlayerRespawn(RespawnManager respawnManager, bool immediate = false)
    {
        if (respawnManager == null) return;

        if (immediate)
        {
            respawnManager.TriggerRespawn();
            return;
        }

        if (respawnInProgress)
        {
            Debug.Log("GameManager: Respawn déjà en cours.");
            return;
        }

        StartCoroutine(RespawnCountdownRoutine(respawnManager, respawnCountdownSeconds));
    }
    private IEnumerator RespawnCountdownRoutine(RespawnManager respawnManager, int seconds)
    {
        respawnInProgress = true;

        if (respawnText != null)
            respawnText.gameObject.SetActive(true);

        for (int i = seconds; i > 0; i--)
        {
            if (respawnText != null)
            {
                respawnText.text = $"Out of playing zone... Respawning in\n{i}";
                respawnText.fontSize = 24;
                respawnText.color = Color.white;
            }

            yield return new WaitForSeconds(1f);
        }

        if (respawnText != null)
        {
            respawnText.text = "Respawn!";
            respawnText.color = Color.red;
        }

        yield return new WaitForSeconds(0.3f);

        respawnManager.TriggerRespawn();

        yield return new WaitForSeconds(0.4f);

        if (respawnText != null)
            respawnText.gameObject.SetActive(false);

        respawnInProgress = false;
    }
    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
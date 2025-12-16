using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    // ------------------------------------
    [Header("Panels")]
    public GameObject successMenu;
    public GameObject failureMenu;
    public GameObject gameInfoUI;
    public TextMeshProUGUI playreBestTimeUI;
    public TextMeshProUGUI ghostBestTimeUI;
    public TextMeshProUGUI playerCurrentSpeed;

    // ------------------------------------

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
        UpdateUI();
        UpdateCoinGUI();

        if (countdownText != null)
        {
            countdownText.text = "Franchissez la ligne de départ pour commencer";
            countdownText.color = Color.white;
        }
        if (ghostController == null)
        {
            ghostController = FindObjectOfType<GhostController>();
        }

        successMenu.SetActive(false);
        failureMenu.SetActive(false);
    }

    void ValidateCheckpoints()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
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
        // if (playerCurrentSpeed != null)
        // {
        //     playerCurrentSpeed.text = Mathf.RoundToInt(playerMovement.GetCurrentSpeed()).ToString() + " km/h";
        // }

        if (raceActive)
        {
            totalTime += Time.deltaTime;
            currentLapTime += Time.deltaTime;
            UpdateUI();
        }
    }

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

        if (ghostController != null)
        {
            ghostController.StartRace();
        }
    }

    void CompleteLap()
    {
        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
        }

        currentLap++;

        if (currentLap > totalLaps)
            FinishRace();
        else
        {
            currentLapTime = 0f;
            UpdateUI();
        }
    }

    void FinishRace()
    {
        raceActive = false;
        if (totalLaps == 3)
        {
            Time.timeScale = 0f;
            if (bestLapTime <= ghostController.GetBestLapTime())
            {
                successMenu.SetActive(true);
                gameInfoUI.SetActive(false);

                if (playreBestTimeUI != null && ghostBestTimeUI != null)
                {
                    ghostBestTimeUI.text = "Ghost Best Time: " + FormatTime(ghostController.GetBestLapTime());
                    playreBestTimeUI.text = "Player Best Time: " + FormatTime(bestLapTime);
                }
            }
            else
            {
                failureMenu.SetActive(true);
            }
        }
    }

    void UpdateUI()
    {
        if (timerText != null)
        {
            if (raceActive)
                timerText.text = FormatTime(totalTime);
            else
                timerText.text = "Time: --:--:--";
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
        UpdateUI();

    }


    // ------------------------------------
    // MÉTHODES POUR LES COINS
    // ------------------------------------
    public void AddCoin()
    {
        coinCount++;
        if (coinCount >= 5)
        {

        }
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
            return Vector3.zero;
        }
        return checkpoints[index].position;
    }

    public Quaternion GetCheckpointRotation(int index)
    {
        if (index < 0 || index >= checkpoints.Length)
        {
            return Quaternion.identity;
        }
        return checkpoints[index].rotation;
    }

    public Transform GetCheckpoint(int index)
    {
        if (index < 0 || index >= checkpoints.Length)
        {
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
            return;
        }

        StartCoroutine(RespawnCountdownRoutine(respawnManager, respawnCountdownSeconds));
    }
    private IEnumerator RespawnCountdownRoutine(RespawnManager respawnManager, int seconds)
    {
        respawnInProgress = true;

        if (respawnText != null)
        {
            respawnText.gameObject.SetActive(true);
        }

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
        {
            respawnText.gameObject.SetActive(false);
        }

        respawnInProgress = false;
    }
    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI - Chronomètre")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI lapCountText;
    [SerializeField] private TextMeshProUGUI bestLapText;

    [Header("UI - Compte à rebours")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private bool useCountdown = true;

    [Header("Settings")]
    [SerializeField] private int totalLaps = 3;
    [SerializeField] private bool disablePlayerControlsDuringCountdown = true;
    private bool raceStarted = false;
    private bool raceActive = false;
    private PlayerMovement playerMovement;
    private float totalTime = 0f;
    private float currentLapTime = 0f;
    private float bestLapTime = Mathf.Infinity;
    private int currentLap = 0;

    // coin

    [Header("Coin")]
    public TextMeshProUGUI coinText;
    private Coin coin;

    void Start()
    {
        UpdateTimerUI();
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!raceStarted)
            {
                playerMovement = other.GetComponent<PlayerMovement>();

                if (useCountdown)
                {
                    StartCoroutine(CountdownSequence());
                }
                else
                {
                    StartRaceImmediately();
                }
            }
            else if (raceActive)
            {
                CompleteLap();
            }
        }
    }

    IEnumerator CountdownSequence()
    {
        raceStarted = true;

        Debug.Log("🏁 Joueur détecté sur la ligne de départ !");

        if (disablePlayerControlsDuringCountdown && playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.fontSize = 100;
                countdownText.color = Color.red;
            }

            Debug.Log($"⏱️ Compte à rebours: {i}");
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = Color.green;
        }

        Debug.Log("🚀 GO! La course commence !");
        yield return new WaitForSeconds(0.5f);


        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        StartRaceImmediately();
    }

    void StartRaceImmediately()
    {
        raceStarted = true;
        raceActive = true;
        currentLap = 1;
        totalTime = 0f;
        currentLapTime = 0f;


        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Debug.Log("✅ Course démarrée !");
    }

    void CompleteLap()
    {
        Debug.Log($"🏁 Tour {currentLap} terminé en {FormatTime(currentLapTime)}");


        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
            Debug.Log($"⭐ Nouveau meilleur tour : {FormatTime(bestLapTime)}");
        }

        currentLap++;


        if (currentLap > totalLaps)
        {
            FinishRace();
        }
        else
        {
            currentLapTime = 0f;
            UpdateTimerUI();
        }
    }

    void FinishRace()
    {
        raceActive = false;

        Debug.Log("🏆 Course terminée !");
        Debug.Log($"⏱️ Temps total : {FormatTime(totalTime)}");
        Debug.Log($"⭐ Meilleur tour : {FormatTime(bestLapTime)}");

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Course terminée !\nTemps total: " + FormatTime(totalTime);
            countdownText.fontSize = 50;
            countdownText.color = Color.cyan;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (raceActive)
                timerText.text = "Temps: " + FormatTime(totalTime);
            else
                timerText.text = "Temps: --:--:---";
        }

        if (lapCountText != null)
        {
            if (raceActive)
                lapCountText.text = $"Tour: {currentLap}/{totalLaps}";
            else
                lapCountText.text = $"Tour: 0/{totalLaps}";
        }

        if (bestLapText != null)
        {
            if (bestLapTime < Mathf.Infinity)
                bestLapText.text = "Meilleur: " + FormatTime(bestLapTime);
            else
                bestLapText.text = "Meilleur: --:--:---";
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public void RestartRace()
    {
        raceStarted = false;
        raceActive = false;
        currentLap = 0;
        totalTime = 0f;
        currentLapTime = 0f;
        bestLapTime = Mathf.Infinity;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "Franchissez la ligne de départ pour commencer";
            countdownText.fontSize = 50;
            countdownText.color = Color.white;
        }

        UpdateTimerUI();
        Debug.Log("🔄 Course réinitialisée");
    }

    // Getters pour accéder aux données depuis d'autres scripts
    public float GetTotalTime() => totalTime;
    public float GetCurrentLapTime() => currentLapTime;
    public float GetBestLapTime() => bestLapTime;
    public int GetCurrentLap() => currentLap;
    public bool IsRaceActive() => raceActive;

    void UpdateCoinGUI()
    {
        if (coinText != null && coin != null)
        {
            coinText.text = "Coins: " + coin.CoinCount.ToString();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private int totalLaps = 3;

    [Header("UI References")]
    [SerializeField] private Text currentTimeText;
    [SerializeField] private Text lapTimeText;
    [SerializeField] private Text bestLapText;
    [SerializeField] private Text lapCounterText;

    private float currentTime;
    private float currentLapTime;
    private float bestLapTime = Mathf.Infinity;
    private int currentLap = 0;
    private bool raceStarted = false;
    private bool raceFinished = false;

    private List<float> lapTimes = new List<float>();

    void Update()
    {
        if (raceStarted && !raceFinished)
        {
            currentTime += Time.deltaTime;
            currentLapTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void StartRace()
    {
        raceStarted = true;
        currentTime = 0f;
        currentLapTime = 0f;
        currentLap = 1;
        lapTimes.Clear();
        bestLapTime = Mathf.Infinity;
        raceFinished = false;

        Debug.Log("Course démarrée !");
    }

    public void CompleteLap()
    {
        if (!raceStarted || raceFinished) return;

        lapTimes.Add(currentLapTime);

        // Mise à jour du meilleur temps
        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
            Debug.Log("Nouveau meilleur tour : " + FormatTime(bestLapTime));
        }

        Debug.Log($"Tour {currentLap} terminé en {FormatTime(currentLapTime)}");

        currentLap++;

        // Vérifier si la course est terminée
        if (currentLap > totalLaps)
        {
            FinishRace();
        }
        else
        {
            currentLapTime = 0f; // Reset le temps du tour
        }
    }

    void FinishRace()
    {
        raceFinished = true;
        Debug.Log($"Course terminée ! Temps total : {FormatTime(currentTime)}");
        Debug.Log($"Meilleur tour : {FormatTime(bestLapTime)}");

        // Afficher tous les temps de tour
        for (int i = 0; i < lapTimes.Count; i++)
        {
            Debug.Log($"Tour {i + 1}: {FormatTime(lapTimes[i])}");
        }
    }

    void UpdateTimerUI()
    {
        if (currentTimeText != null)
            currentTimeText.text = "Temps: " + FormatTime(currentTime);

        if (lapTimeText != null)
            lapTimeText.text = "Tour actuel: " + FormatTime(currentLapTime);

        if (bestLapText != null)
        {
            if (bestLapTime < Mathf.Infinity)
                bestLapText.text = "Meilleur tour: " + FormatTime(bestLapTime);
            else
                bestLapText.text = "Meilleur tour: --:--:---";
        }

        if (lapCounterText != null)
            lapCounterText.text = $"Tour: {currentLap}/{totalLaps}";
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    // Getters utiles
    public float GetCurrentTime() => currentTime;
    public float GetCurrentLapTime() => currentLapTime;
    public float GetBestLapTime() => bestLapTime;
    public int GetCurrentLap() => currentLap;
    public bool IsRaceFinished() => raceFinished;
    public List<float> GetAllLapTimes() => lapTimes;
}
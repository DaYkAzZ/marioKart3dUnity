using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TrackZoneController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float offTrackDuration = 3f;
    [SerializeField] private bool showDebug = true;

    [Header("UI Warning")]
    [SerializeField] private Image warningUI;
    [SerializeField] private Color warningColor = Color.red;

    private RespawnManager respawnManager;
    private readonly HashSet<Collider> safeZones = new HashSet<Collider>();
    private float offTrackTimer = 0f;
    private bool isRespawning = false;
    private bool isImmune = false;

    void Start()
    {
        respawnManager = GetComponent<RespawnManager>();
        if (warningUI != null) warningUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRespawning || isImmune) return;

        if (safeZones.Count == 0)
        {
            offTrackTimer += Time.deltaTime;
            if (warningUI != null) ShowWarningUI();

            if (offTrackTimer >= offTrackDuration)
                RequestRespawn(false);
        }
        else
        {
            if (offTrackTimer > 0f) offTrackTimer = 0f;
            if (warningUI != null) warningUI.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isImmune) return;

        TrackZone zone = other.GetComponent<TrackZone>();
        if (zone == null) return;

        if (zone.zoneType == TrackZone.ZoneType.SafeZone)
        {
            safeZones.Add(other);
            if (showDebug) Debug.Log($"Entrée SafeZone: {other.name} (count={safeZones.Count})");
        }
        else if (zone.zoneType == TrackZone.ZoneType.DeathZone)
        {
            if (showDebug) Debug.Log($"DeathZone touchée: {other.name}");
            RequestRespawn(true);
        }
        else if (zone.zoneType == TrackZone.ZoneType.Checkpoint)
        {
            // enregistrement checkpoint
            int idx = zone.checkpointIndex;
            if (GameManager.Instance != null && idx >= 0 && idx < GameManager.Instance.GetCheckpointCount())
            {
                respawnManager?.SetCheckpoint(idx);
                if (showDebug) Debug.Log($"Checkpoint passé: {idx}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (isImmune) return;

        TrackZone zone = other.GetComponent<TrackZone>();
        if (zone != null && zone.zoneType == TrackZone.ZoneType.SafeZone)
        {
            safeZones.Remove(other);
            if (showDebug) Debug.Log($"Sortie SafeZone: {other.name} (count={safeZones.Count})");
        }
    }

    private void RequestRespawn(bool immediate)
    {
        if (isRespawning) return;
        if (respawnManager == null)
        {
            Debug.LogError("RespawnManager manquant !");
            return;
        }

        isRespawning = true;
        offTrackTimer = 0f;

        if (immediate)
        {
            respawnManager.TriggerRespawn();
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RequestPlayerRespawn(respawnManager);
            else
                respawnManager.TriggerRespawn();
        }
    }

    // ---------- NOUVELLES API pour le RespawnManager ----------
    // Ajoute un collider SAFE sans passer par Trigger (utilisé après TP)
    public void ForceAddSafeZone(Collider safeZoneCollider)
    {
        if (safeZoneCollider == null) return;
        safeZones.Add(safeZoneCollider);
        if (showDebug) Debug.Log($"ForceAddSafeZone: {safeZoneCollider.name} (count={safeZones.Count})");
    }

    // Enregistre un checkpoint si on spawn dedans
    public void ForceRegisterCheckpoint(int index)
    {
        if (respawnManager != null)
        {
            respawnManager.SetCheckpoint(index);
            if (showDebug) Debug.Log($"ForceRegisterCheckpoint: {index}");
        }
    }

    // ---------- Immunité ----------
    public void EnterRespawnImmunity(float duration)
    {
        StartCoroutine(RespawnImmunityRoutine(duration));
    }

    private IEnumerator RespawnImmunityRoutine(float duration)
    {
        isImmune = true;
        safeZones.Clear();
        offTrackTimer = 0f;

        if (showDebug) Debug.Log("Immunité aux triggers activée");

        yield return new WaitForSeconds(duration);

        isImmune = false;
        if (showDebug) Debug.Log("Immunité aux triggers désactivée");
    }

    public void OnRespawnComplete()
    {
        isRespawning = false;
        safeZones.Clear();
        offTrackTimer = 0f;
        if (warningUI != null) warningUI.gameObject.SetActive(false);
    }

    private void ShowWarningUI()
    {
        if (warningUI == null) return;
        warningUI.gameObject.SetActive(true);
        float alpha = Mathf.PingPong(Time.time * 3f, 1f);
        Color c = warningColor; c.a = alpha;
        warningUI.color = c;
    }
}

using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class RespawnManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 respawnOffset = new Vector3(0, 2.0f, 0);
    [SerializeField] private float triggerImmunityDuration = 0.6f; // plus long pour sécurité
    [SerializeField] private float postRespawnMovementDisable = 0.5f; // disable PlayerMovement X sec
    [SerializeField] private float overlapScanRadius = 1.2f; // rayon pour scan de zones après TP
    [SerializeField] private bool showDebug = true;

    private Rigidbody rb;
    private int lastCheckpoint = 0;
    private Transform[] checkpoints;
    private TrackZoneController zoneController;
    private MonoBehaviour playerMovementScript; // optionnel : script de movement à désactiver

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        zoneController = GetComponent<TrackZoneController>();

        if (GameManager.Instance != null)
            checkpoints = GameManager.Instance.checkpoints;
        else
            checkpoints = null;

        // essayer de trouver un script de movement courant (nom commun)
        playerMovementScript = GetComponent<MonoBehaviour>();
        // si tu as un script spécifique (ex: PlayerMovement), assigne-le manuellement dans l'inspector au besoin.

        if (showDebug)
        {
            if (checkpoints == null || checkpoints.Length == 0)
                Debug.LogWarning("RespawnManager : aucun checkpoint défini.");
            else
                Debug.Log($"RespawnManager initialisé ({checkpoints.Length} checkpoints).");
        }
    }

    public void SetCheckpoint(int index)
    {
        if (checkpoints == null) return;
        if (index < 0 || index >= checkpoints.Length) return;
        lastCheckpoint = index;
        if (showDebug) Debug.Log($"RespawnManager: checkpoint {lastCheckpoint} enregistré.");
    }

    public void TriggerRespawn()
    {
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogError("RespawnManager.TriggerRespawn annulé : aucun checkpoint disponible.");
            return;
        }
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        lastCheckpoint = Mathf.Clamp(lastCheckpoint, 0, checkpoints.Length - 1);
        Transform cp = checkpoints[lastCheckpoint];
        if (cp == null)
        {
            Debug.LogError($"RespawnManager: checkpoint null pour index {lastCheckpoint}");
            yield break;
        }

        if (showDebug) Debug.Log($"RespawnManager: teleport vers CP {lastCheckpoint} ({cp.name})");

        // DESACTIVER collisions pour éviter triggers durant le repositionnement
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // STOPPER forces/vélocité
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // petit délai (physique stable)
        yield return new WaitForFixedUpdate();
        yield return null;

        // position & rotation (Y only)
        Vector3 targetPos = cp.position + respawnOffset;
        Quaternion yawOnly = Quaternion.Euler(0f, cp.rotation.eulerAngles.y, 0f);
        transform.position = targetPos;
        transform.rotation = yawOnly;

        // Force the physics engine to be aware of the new transform
        Physics.SyncTransforms();

        // START immunity in zone controller and clear safe zones
        if (zoneController != null)
            zoneController.EnterRespawnImmunity(triggerImmunityDuration);

        // SCAN des volumes qui recouvrent la nouvelle position et informe zoneController
        PopulateZonesAtPosition();

        // Désactiver le movement temporairement (empêche re-mouvement instantané)
        DisableMovementTemporarily(postRespawnMovementDisable);

        // attendre l'immunité avant réactiver collider
        yield return new WaitForSeconds(triggerImmunityDuration);

        if (col != null) col.enabled = true;

        // second sync pour éviter events différés
        Physics.SyncTransforms();

        // notifier fin
        if (zoneController != null)
            zoneController.OnRespawnComplete();

        if (showDebug) Debug.Log("RespawnManager: respawn terminé proprement.");
    }

    private void PopulateZonesAtPosition()
    {
        // récupère tous les colliders proches et ajoute ceux qui sont des SafeZone au TrackZoneController
        Collider[] hits = Physics.OverlapSphere(transform.position, overlapScanRadius);
        foreach (var c in hits)
        {
            TrackZone tz = c.GetComponent<TrackZone>();
            if (tz != null && tz.zoneType == TrackZone.ZoneType.SafeZone)
            {
                // fournir le collider au controller
                zoneController?.ForceAddSafeZone(c);
                if (showDebug) Debug.Log($"RespawnManager: zone sûre détectée post-TP -> {c.gameObject.name}");
            }
            else if (tz != null && tz.zoneType == TrackZone.ZoneType.Checkpoint)
            {
                // si on spawn dans un checkpoint trigger, enregistre le checkpoint
                zoneController?.ForceRegisterCheckpoint(tz.checkpointIndex);
                if (showDebug) Debug.Log($"RespawnManager: checkpoint détecté post-TP -> idx {tz.checkpointIndex}");
            }
        }
    }

    private void DisableMovementTemporarily(float duration)
    {
        // cherche un script PlayerMovement sur le joueur (standard name)
        var pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            StartCoroutine(DisableComponentRoutine(pm, duration));
            return;
        }

        // Si tu utilises un autre script de movement, ajoute la logique ici ou assigne manuellement.
    }

    private IEnumerator DisableComponentRoutine(Behaviour comp, float duration)
    {
        if (comp == null) yield break;
        bool prev = comp.enabled;
        comp.enabled = false;
        yield return new WaitForSeconds(duration);
        comp.enabled = prev;
    }
}
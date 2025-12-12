using UnityEngine;

public class TrackZone : MonoBehaviour
{
    public enum ZoneType { SafeZone, DeathZone, Checkpoint }

    [Header("Zone Type")]
    public ZoneType zoneType = ZoneType.SafeZone;

    [Header("Checkpoint Settings")]
    public int checkpointIndex = -1;

    [Header("Gizmo")]
    public Color gizmoColor = Color.yellow;

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError("❌ TrackZone sans collider !");
        else if (!col.isTrigger)
            Debug.LogWarning("⚠️ TrackZone devrait être un Trigger !");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
            Gizmos.DrawWireCube(transform.TransformPoint(box.center), box.size);
    }
}

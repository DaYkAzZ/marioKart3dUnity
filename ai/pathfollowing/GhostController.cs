using UnityEngine;

public class GhostController : MonoBehaviour
{
    public WaypointsManager waypointManager;
    public float speed = 12f;
    public float turnSpeed = 6f;
    public float waypointReachDistance = 3f;

    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Sicherheit
        if (waypointManager == null)
        {
            Debug.LogError("Assign WaypointManager to the AI kart!");
        }
    }

    private void FixedUpdate()
    {
        if (waypointManager == null || waypointManager.waypoints.Count == 0)
            return;

        MoveAI();
    }

    void MoveAI()
    {
        Transform target = waypointManager.waypoints[currentWaypointIndex];

        // Direction vers le waypoint
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        // Rotation smooth
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);

        // Accélération
        rb.AddForce(transform.forward * speed, ForceMode.Acceleration);

        // Changement de waypoint
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < waypointReachDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypointManager.waypoints.Count)
                currentWaypointIndex = 0; // boucle du circuit
        }
    }
}

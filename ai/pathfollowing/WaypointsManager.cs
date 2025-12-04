using System.Collections.Generic;
using UnityEngine;

public class WaypointsManager : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();

    private void Awake()
    {
        waypoints.Clear();

        foreach (Transform t in transform)
        {
            waypoints.Add(t);
        }
    }
    public Color lineColor = Color.yellow;
    public Color directionColor = Color.red;
    public float arrowLength = 1.5f;

    private void OnDrawGizmos()
    {
        // Récupère tous les waypoints
        int count = transform.childCount;
        if (count < 2) return;

        Gizmos.color = lineColor;

        for (int i = 0; i < count; i++)
        {
            Transform current = transform.GetChild(i);
            Transform next = transform.GetChild((i + 1) % count);

            // Dessine une sphère au waypoint
            Gizmos.DrawSphere(current.position, 0.3f);

            // Dessine la ligne vers le prochain
            Gizmos.DrawLine(current.position, next.position);

            // Dessine la direction du waypoint (une flèche rouge)
            Gizmos.color = directionColor;
            Vector3 arrowStart = current.position;
            Vector3 arrowEnd = current.position + current.forward * arrowLength;
            Gizmos.DrawLine(arrowStart, arrowEnd);

            // Petite flèche en V
            Vector3 rightWing = Quaternion.AngleAxis(20, Vector3.up) * -current.forward;
            Vector3 leftWing = Quaternion.AngleAxis(-20, Vector3.up) * -current.forward;

            Gizmos.DrawLine(arrowEnd, arrowEnd + rightWing * 0.5f);
            Gizmos.DrawLine(arrowEnd, arrowEnd + leftWing * 0.5f);

            Gizmos.color = lineColor; // reset couleur
        }
    }
}
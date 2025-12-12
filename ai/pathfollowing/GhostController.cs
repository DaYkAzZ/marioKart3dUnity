using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaypointData
{
    public Vector3 position;
    public float optimalSpeed;
    public float lateralOffset;

    public WaypointData(Vector3 pos, float speed, float offset)
    {
        position = pos;
        optimalSpeed = speed;
        lateralOffset = offset;
    }
}

/// <summary>
/// Contrôleur complet du kart fantôme : IA avec apprentissage + apparence transparente + sans collisions
/// </summary>
public class GhostController : MonoBehaviour
{
    [Header("References")]
    public WaypointsManager waypointManager;

    [Header("AI Learning Settings")]
    [SerializeField] private bool isLearning = true;
    [SerializeField] private int totalTrainingLaps = 10;
    [SerializeField] private float explorationRate = 0.3f;
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private bool waitForRaceStart = true; // Attendre le signal de départ

    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 15f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float turnSpeed = 6f;
    [SerializeField] private float waypointLookAhead = 6f;

    [Header("Exploration Settings")]
    [SerializeField] private float maxLateralOffset = 3f;
    [SerializeField] private float speedVariation = 5f;

    [Header("Ghost Appearance")]
    [SerializeField][Range(0f, 1f)] private float opacity = 0.5f;
    [SerializeField] private Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    [Header("Ghost Physics")]
    [SerializeField] private bool disableCollisions = true;

    private int currentWP = 0;
    private Rigidbody rb;
    private int currentLap = 0;
    private float currentLapTime = 0f;
    private float bestLapTime = Mathf.Infinity;
    private bool raceStarted = false; // Signal de départ

    // Données d'apprentissage
    private List<WaypointData> learnedPath = new List<WaypointData>();
    private List<WaypointData> currentAttempt = new List<WaypointData>();
    private List<float> lapTimes = new List<float>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Configuration du fantôme (apparence + physique)
        SetupGhostAppearance();
        SetupGhostPhysics();

        // Initialisation de l'IA
        InitializeLearning();
        Debug.Log("L'IA démarre un nouvel apprentissage.");
    }

    void InitializeLearning()
    {
        learnedPath.Clear();
        foreach (Transform wp in waypointManager.waypoints)
        {
            learnedPath.Add(new WaypointData(wp.position, baseSpeed, 0f));
        }
    }

    void FixedUpdate()
    {
        if (waypointManager == null) return;

        // Attendre le signal de départ si activé
        if (waitForRaceStart && !raceStarted)
        {
            // Le ghost reste immobile jusqu'au départ
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        if (isLearning)
        {
            currentLapTime += Time.fixedDeltaTime;
            LearnAndDrive();
        }
        else
        {
            DriveOptimal();
        }
    }

    void LearnAndDrive()
    {
        Transform baseWaypoint = waypointManager.waypoints[currentWP];
        WaypointData learnedData = learnedPath[currentWP];

        bool explore = Random.value < explorationRate && currentLap < totalTrainingLaps;

        Vector3 targetPosition;
        float targetSpeed;

        if (explore)
        {
            // EXPLORATION
            float randomOffset = Random.Range(-maxLateralOffset, maxLateralOffset);
            float randomSpeed = baseSpeed + Random.Range(-speedVariation, speedVariation);
            randomSpeed = Mathf.Clamp(randomSpeed, minSpeed, maxSpeed);

            Vector3 right = Vector3.Cross(Vector3.up, (waypointManager.waypoints[(currentWP + 1) % waypointManager.waypoints.Count].position - baseWaypoint.position).normalized);
            targetPosition = baseWaypoint.position + right * randomOffset;
            targetSpeed = randomSpeed;

            currentAttempt.Add(new WaypointData(targetPosition, targetSpeed, randomOffset));
        }
        else
        {
            // EXPLOITATION
            Vector3 right = Vector3.Cross(Vector3.up, (waypointManager.waypoints[(currentWP + 1) % waypointManager.waypoints.Count].position - baseWaypoint.position).normalized);
            targetPosition = baseWaypoint.position + right * learnedData.lateralOffset;
            targetSpeed = learnedData.optimalSpeed;

            currentAttempt.Add(new WaypointData(targetPosition, targetSpeed, learnedData.lateralOffset));
        }

        MoveToTarget(targetPosition, targetSpeed);

        Vector3 dir = targetPosition - transform.position;
        dir.y = 0;

        if (dir.magnitude < waypointLookAhead)
        {
            currentWP++;
            if (currentWP >= waypointManager.waypoints.Count)
            {
                CompleteLap();
            }
        }
    }

    void DriveOptimal()
    {
        Transform baseWaypoint = waypointManager.waypoints[currentWP];
        WaypointData optimalData = learnedPath[currentWP];

        Vector3 right = Vector3.Cross(Vector3.up, (waypointManager.waypoints[(currentWP + 1) % waypointManager.waypoints.Count].position - baseWaypoint.position).normalized);
        Vector3 targetPosition = baseWaypoint.position + right * optimalData.lateralOffset;

        MoveToTarget(targetPosition, optimalData.optimalSpeed);

        Vector3 dir = targetPosition - transform.position;
        dir.y = 0;

        if (dir.magnitude < waypointLookAhead)
        {
            currentWP++;
            if (currentWP >= waypointManager.waypoints.Count)
                currentWP = 0;
        }
    }

    void MoveToTarget(Vector3 targetPosition, float speed)
    {
        Vector3 dir = (targetPosition - transform.position);
        dir.y = 0;
        Vector3 dirNorm = dir.normalized;

        Quaternion rot = Quaternion.LookRotation(dirNorm);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);

        float angleToTarget = Vector3.Angle(transform.forward, dirNorm);
        float speedMultiplier = Mathf.Lerp(1f, 0.6f, angleToTarget / 90f);

        rb.velocity = transform.forward * speed * speedMultiplier;
    }

    void CompleteLap()
    {
        currentWP = 0;
        currentLap++;

        Debug.Log($"Ghost - Tour {currentLap} terminé en {currentLapTime:F2}s");

        if (currentLapTime < bestLapTime)
        {
            bestLapTime = currentLapTime;
            UpdateLearnedPath();
            Debug.Log($"✓ Nouveau meilleur temps ! {bestLapTime:F2}s");
        }

        lapTimes.Add(currentLapTime);
        currentLapTime = 0f;
        currentAttempt.Clear();

        if (currentLap < totalTrainingLaps)
        {
            explorationRate = Mathf.Lerp(0.3f, 0.05f, (float)currentLap / totalTrainingLaps);
        }
        else if (isLearning)
        {
            isLearning = false;
            explorationRate = 0f;
            Debug.Log($"=== Apprentissage terminé ! ===");
            Debug.Log($"Meilleur temps : {bestLapTime:F2}s");
            Debug.Log($"Tours total : {currentLap}");
            Debug.Log("Le Ghost utilise maintenant son meilleur tracé.");
        }
    }

    void UpdateLearnedPath()
    {
        for (int i = 0; i < currentAttempt.Count && i < learnedPath.Count; i++)
        {
            learnedPath[i].optimalSpeed = Mathf.Lerp(learnedPath[i].optimalSpeed,
                                                     currentAttempt[i].optimalSpeed,
                                                     learningRate);

            learnedPath[i].lateralOffset = Mathf.Lerp(learnedPath[i].lateralOffset,
                                                      currentAttempt[i].lateralOffset,
                                                      learningRate);
        }
    }

    // ========== APPARENCE FANTÔME ==========

    void SetupGhostAppearance()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                SetMaterialTransparent(mat);

                mat.color = new Color(
                    ghostColor.r,
                    ghostColor.g,
                    ghostColor.b,
                    opacity
                );
            }
        }

        Debug.Log($"✓ Kart fantôme rendu transparent (opacité: {opacity})");
    }

    void SetMaterialTransparent(Material material)
    {
        material.SetFloat("_Mode", 3);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    void SetupGhostPhysics()
    {
        if (!disableCollisions) return;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.detectCollisions = false;
        }

        Debug.Log("✓ Collisions du kart fantôme désactivées");
    }

    // ========== MÉTHODES PUBLIQUES ==========

    public float GetBestLapTime() => bestLapTime;
    public int GetCurrentLap() => currentLap;
    public bool IsStillLearning() => isLearning;

    /// <summary>
    /// Démarre la course du ghost (appelé par GameManager)
    /// </summary>
    public void StartRace()
    {
        raceStarted = true;
        Debug.Log("🏁 Ghost : Course démarrée !");
    }

    /// <summary>
    /// Arrête le ghost (appelé par GameManager au restart)
    /// </summary>
    public void StopRace()
    {
        raceStarted = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("⏸️ Ghost : Course arrêtée !");
    }

    public void SetOpacity(float newOpacity)
    {
        opacity = Mathf.Clamp01(newOpacity);
        SetupGhostAppearance();
    }

    public void SetGhostColor(Color color)
    {
        ghostColor = color;
        ghostColor.a = opacity;
        SetupGhostAppearance();
    }

    public void ResetTraining()
    {
        currentLap = 0;
        bestLapTime = Mathf.Infinity;
        lapTimes.Clear();
        isLearning = true;
        explorationRate = 0.3f;
        raceStarted = false;
        InitializeLearning();

        Debug.Log("IA réinitialisée. Nouvel apprentissage commencé.");
    }

    public void SetPerformanceMode()
    {
        isLearning = false;
        explorationRate = 0f;
        Debug.Log("Mode performance activé.");
    }

    // ========== GIZMOS ==========

    void OnDrawGizmos()
    {
        if (learnedPath == null || learnedPath.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < learnedPath.Count; i++)
        {
            int next = (i + 1) % learnedPath.Count;
            Gizmos.DrawLine(learnedPath[i].position, learnedPath[next].position);
            Gizmos.DrawSphere(learnedPath[i].position, 0.5f);
        }
    }
}
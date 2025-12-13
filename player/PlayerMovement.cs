using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float accelerationSpeed = 5f;
    public float maxSpeed = 10f;
    public float turnSpeed = 60f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float brakingForce = 5f;

    // --------------------------------
    [Header("Physics Settings")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isGrounded;
    // --------------------------------

    [Header("Boost Settings")]
    public bool isBoosted = false;
    // --------------------------------
    [SerializeField] private MeshRenderer[] meshRenderer;
    private Color originalColor;
    private Boostpad boostpad;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boostpad = GetComponent<Boostpad>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
        if (meshRenderer.Length > 0)
        {
            originalColor = meshRenderer[0].material.color;
        }
    }

    void Update()
    {
        CheckGrounded();
        if (isBoosted)
        {
            EnableBoostApparence(true);
            isBoosted = false;
        }
    }

    void FixedUpdate()
    {
        KartMovement();
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    void KartMovement()
    {
        if (!isGrounded) return;

        float moveVertical = Input.GetAxis("Vertical");
        float moveHorizontal = Input.GetAxis("Horizontal");

        if (moveVertical > 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, accelerationSpeed * Time.fixedDeltaTime);
        }
        else if (moveVertical < 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, -maxSpeed * 0.5f, brakingForce * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, drag * Time.fixedDeltaTime);
        }

        Vector3 moveDirection = transform.forward * currentSpeed;
        rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);

        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnAmount = moveHorizontal * turnSpeed * Time.fixedDeltaTime;
            if (currentSpeed < 0)
                turnAmount = -turnAmount;

            Quaternion turnRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    public void SetSpeed(float speed)
    {
        currentSpeed = Mathf.Clamp(speed, -maxSpeed * 0.5f, maxSpeed);
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
    void EnableBoostApparence(bool boost)
    {
        for (int i = 0; i < meshRenderer.Length; i++)
        {
            meshRenderer[i].material.color = Color.yellow;
        }

        Invoke(nameof(DisableBoostApparence), 1.5f);
    }
    void DisableBoostApparence()
    {
        for (int i = 0; i < meshRenderer.Length; i++)
        {
            meshRenderer[i].material.color = originalColor;
        }
    }
}
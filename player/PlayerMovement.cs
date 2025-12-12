using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float accelerationSpeed = 8f;
    public float maxSpeed = 14f;
    public float turnSpeed = 60f;
    [SerializeField] private float drag = 2f;
    [SerializeField] private float brakingForce = 5f;

    [Header("Physics Settings")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private float currentSpeed;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        CheckGrounded();
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
}
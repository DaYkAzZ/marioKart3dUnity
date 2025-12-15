using UnityEngine;

public class WheelController : MonoBehaviour
{
    [Header("Wheel Transforms")]
    public Transform wheelPivotFL;
    public Transform wheelPivotFR;
    public Transform[] Wheels;

    [Header("Wheel Settings")]
    public float rotationAngle = 30f;
    public float rotationSpeed = 500f;

    void Update()
    {
        RotateWheels();
        TurnWheels();
    }
    void TurnWheels()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        float steer = horizontalInput * rotationAngle;

        wheelPivotFL.localRotation = Quaternion.Euler(0f, 0f, steer);
        wheelPivotFR.localRotation = Quaternion.Euler(0f, 0f, steer);
    }
    void RotateWheels()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float rotationAmount = (verticalInput * rotationSpeed * Time.deltaTime) * 100f;

        foreach (Transform wheel in Wheels)
        {
            wheel.Rotate(Vector3.right, rotationAmount);
        }
    }
}
using UnityEngine;

public class Coin : MonoBehaviour
{
    public Transform player;
    public float CoinCount = 0;
    public float rotationSpeed = 50f;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CoinCount += 1;
            Destroy(gameObject);
        }
    }
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
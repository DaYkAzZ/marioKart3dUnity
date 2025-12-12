using UnityEngine;

public class Coin : MonoBehaviour
{
    public Transform player;
    [SerializeField] private float rotationSpeed = 50f;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
            GameManager.Instance.AddCoin();
        }
    }
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
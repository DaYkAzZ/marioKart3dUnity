using System.Collections;
using UnityEngine;

public enum ItemType
{
    SpeedBoost,
    SlowDown,
    Rocket
}

public class ItemBox : MonoBehaviour
{
    [SerializeField] private UIItemDisplay uiDisplay;

    [Header("Probabilities (%)")]
    [Range(0, 100)][SerializeField] private float speedBoostChance = 40f;
    [Range(0, 100)][SerializeField] private float slowDownChance = 30f;
    [Range(0, 100)][SerializeField] private float rocketChance = 20f;

    [Header("Visual Animation")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private Collider boxCollider;
    private Renderer[] childRenderers;
    private PlayerMovement playerMovement;

    private float baseMaxSpeed;
    private float baseTurnSpeed;

    private bool alreadyTriggered = false;

    void Start()
    {
        startPosition = transform.position;
        boxCollider = GetComponent<Collider>();

        childRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        if (boxCollider == null)
            Debug.LogError("❌ ItemBox : Pas de collider détecté !");

        if (uiDisplay == null)
            Debug.LogError("❌ ItemBox : uiDisplay n'est pas assigné !");
    }

    void Update()
    {
        // Animation de flottement
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = true;

        playerMovement = other.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("❌ ItemBox : Le joueur n'a pas de PlayerMovement !");
            return;
        }

        // Stockage valeurs de base
        baseMaxSpeed = playerMovement.maxSpeed;
        baseTurnSpeed = playerMovement.turnSpeed;

        // Sélection de l'item
        ItemType item = SelectRandomItem();

        // UI Roulette
        uiDisplay.ShowItemRandom(item);

        // Applique le bonus après la roulette
        StartCoroutine(DelayedItemUse(item, 1.5f));

        DisableBox();
    }

    private IEnumerator DelayedItemUse(ItemType item, float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyItemEffect(item);
    }

    void DisableBox()
    {
        if (childRenderers != null)
        {
            foreach (var r in childRenderers)
            {
                if (r != null)
                    r.enabled = false;
            }
        }

        if (boxCollider != null)
            boxCollider.enabled = false;

        // Destroy après un petit délai
        Destroy(gameObject, 0.3f);
    }

    ItemType SelectRandomItem()
    {
        float rand = Random.Range(0f, 100f);
        float cumulative = speedBoostChance;

        if (rand <= cumulative) return ItemType.SpeedBoost;

        cumulative += slowDownChance;
        if (rand <= cumulative) return ItemType.SlowDown;

        return ItemType.Rocket;
    }

    void ApplyItemEffect(ItemType item)
    {
        switch (item)
        {
            case ItemType.SpeedBoost:
                StartCoroutine(SpeedBoostEffect());
                break;

            case ItemType.SlowDown:
                StartCoroutine(SlowDownEffect());
                break;

            case ItemType.Rocket:
                StartCoroutine(RocketEffect());
                break;
        }
    }

    IEnumerator SpeedBoostEffect()
    {
        playerMovement.maxSpeed = baseMaxSpeed + 5f;
        playerMovement.turnSpeed = baseTurnSpeed + 10f;

        yield return new WaitForSeconds(3f);

        playerMovement.maxSpeed = baseMaxSpeed;
        playerMovement.turnSpeed = baseTurnSpeed;
    }

    IEnumerator SlowDownEffect()
    {
        playerMovement.maxSpeed = baseMaxSpeed - 5f;

        yield return new WaitForSeconds(3f);

        playerMovement.maxSpeed = baseMaxSpeed;
    }

    IEnumerator RocketEffect()
    {
        playerMovement.maxSpeed = baseMaxSpeed + 12f;

        yield return new WaitForSeconds(2f);

        playerMovement.maxSpeed = baseMaxSpeed;
    }
}

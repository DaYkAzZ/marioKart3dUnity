using UnityEngine;

public class ItemBox : MonoBehaviour
{
    [SerializeField] private UIItemDisplay uiDisplay;

    [Header("Probabilities (%)")]
    [Range(0, 100)] public float speedBoostChance = 30f;
    [Range(0, 100)] public float slowDownChance = 20f;
    [Range(0, 100)] public float rocketChance = 20f;
    [Range(0, 100)] public float coinBoostChance = 30f;

    private bool alreadyTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = true;

        PlayerItemManager itemManager = other.GetComponent<PlayerItemManager>();
        if (itemManager == null)
        {
            return;
        }

        ItemType item = SelectRandomItem();

        itemManager.ShowItemRouletteAndActivate(item);

        Destroy(gameObject);
    }

    ItemType SelectRandomItem()
    {
        float rand = Random.Range(0f, 100f);
        float cumulative = speedBoostChance;

        if (rand <= cumulative) return ItemType.SpeedBoost;

        cumulative += slowDownChance;
        if (rand <= cumulative) return ItemType.SlowDown;

        cumulative += rocketChance;
        if (rand <= cumulative) return ItemType.Rocket;

        return ItemType.CoinBoost;
    }
}

using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerItemManager : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float boostMultiplier = 2f;
    [SerializeField] private float boostDuration = 3f;

    [Header("Slow Down Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 2f;

    [Header("Rocket Boost Settings")]
    [SerializeField] private float rocketMultiplier = 3f; // x3 au lieu de x2
    [SerializeField] private float rocketDuration = 5f;   // Plus long
    [SerializeField] private ParticleSystem rocketParticles; // Effet visuel
    [SerializeField] private TrailRenderer rocketTrail;      // Traînée

    [Header("UI - Item stocké")]
    [SerializeField] private Image itemIconUI;
    [SerializeField] private Sprite speedSprite;
    [SerializeField] private Sprite slowSprite;
    [SerializeField] private Sprite rocketSprite;

    [Header("UI - Roulette")]
    [SerializeField] private UIItemDisplay itemDisplay;

    private PlayerMovement playerMovement;
    private Rigidbody rb;

    private ItemType? currentItem = null;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();

        if (itemIconUI != null)
            itemIconUI.enabled = false;

        // Désactiver les effets au départ
        if (rocketParticles != null) rocketParticles.Stop();
        if (rocketTrail != null) rocketTrail.emitting = false;
    }

    void Update()
    {
        if (currentItem != null && Input.GetKeyDown(KeyCode.Space))
        {
            UseStoredItem();
        }
    }

    // ------------------------------------
    //    ROULETTE + STOCKAGE DE L'ITEM
    // ------------------------------------
    public void ShowItemRouletteAndActivate(ItemType itemType)
    {
        if (itemDisplay != null)
        {
            itemDisplay.ShowItemRandom(itemType);
        }

        StartCoroutine(StoreItemAfterDelay(itemType, 2.8f));
    }

    private IEnumerator StoreItemAfterDelay(ItemType itemType, float delay)
    {
        yield return new WaitForSeconds(delay);

        ReceiveItem(itemType);

        Debug.Log($"✅ Item {itemType} stocké ! Appuyez sur ESPACE pour l'utiliser.");
    }

    // ------------------------------------
    //         ITEM REÇU DE LA BOX
    // ------------------------------------
    public void ReceiveItem(ItemType item)
    {
        currentItem = item;
        UpdateUI();
    }

    // ------------------------------------
    //         UTILISATION DE L'ITEM
    // ------------------------------------
    private void UseStoredItem()
    {
        if (currentItem == null) return;

        Debug.Log($"🎯 Utilisation de l'item : {currentItem}");

        switch (currentItem)
        {
            case ItemType.SpeedBoost:
                StartCoroutine(SpeedBoostEffect());
                break;

            case ItemType.SlowDown:
                StartCoroutine(SlowDownEffect());
                break;

            case ItemType.Rocket:
                StartCoroutine(RocketBoostEffect());
                break;
        }

        currentItem = null;
        UpdateUI();
    }

    // ------------------------------------
    //         EFFETS DES ITEMS
    // ------------------------------------
    IEnumerator SpeedBoostEffect()
    {
        if (playerMovement == null) yield break;

        float originalSpeed = playerMovement.maxSpeed;
        playerMovement.maxSpeed *= boostMultiplier;

        Debug.Log($"⚡ Speed Boost ! Vitesse: {originalSpeed} → {playerMovement.maxSpeed}");

        yield return new WaitForSeconds(boostDuration);

        playerMovement.maxSpeed = originalSpeed;
        Debug.Log("⚡ Speed Boost terminé");
    }

    IEnumerator SlowDownEffect()
    {
        if (playerMovement == null) yield break;

        float originalSpeed = playerMovement.maxSpeed;
        playerMovement.maxSpeed *= slowMultiplier;

        Debug.Log($"🐌 Ralentissement ! Vitesse: {originalSpeed} → {playerMovement.maxSpeed}");

        yield return new WaitForSeconds(slowDuration);

        playerMovement.maxSpeed = originalSpeed;
        Debug.Log("🐌 Ralentissement terminé");
    }

    IEnumerator RocketBoostEffect()
    {
        if (playerMovement == null) yield break;

        float originalSpeed = playerMovement.maxSpeed;
        playerMovement.maxSpeed *= rocketMultiplier;

        // Effets visuels
        if (rocketParticles != null)
        {
            rocketParticles.Play();
        }

        if (rocketTrail != null)
        {
            rocketTrail.emitting = true;
        }

        Debug.Log($"🚀 ROCKET BOOST ! Vitesse: {originalSpeed} → {playerMovement.maxSpeed} (x{rocketMultiplier})");

        yield return new WaitForSeconds(rocketDuration);

        playerMovement.maxSpeed = originalSpeed;

        // Arrêter les effets
        if (rocketParticles != null)
        {
            rocketParticles.Stop();
        }

        if (rocketTrail != null)
        {
            rocketTrail.emitting = false;
        }

        Debug.Log("🚀 Rocket Boost terminé");
    }

    // ------------------------------------
    //                UI
    // ------------------------------------
    void UpdateUI()
    {
        if (itemIconUI == null) return;

        if (currentItem == null)
        {
            itemIconUI.enabled = false;
            return;
        }

        itemIconUI.enabled = true;

        switch (currentItem)
        {
            case ItemType.SpeedBoost:
                itemIconUI.sprite = speedSprite;
                break;
            case ItemType.SlowDown:
                itemIconUI.sprite = slowSprite;
                break;
            case ItemType.Rocket:
                itemIconUI.sprite = rocketSprite;
                break;
        }
    }

    public bool HasItem() => currentItem != null;
    public ItemType? GetCurrentItem() => currentItem;
}
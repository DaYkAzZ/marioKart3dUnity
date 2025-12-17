using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
public class PlayerItemManager : MonoBehaviour
{
    [Header("Speed Boost Settings")]
    [SerializeField] private float boostMultiplier = 1.1f;
    [SerializeField] private float boostDuration = 1f;

    [Header("Slow Down Settings")]
    [SerializeField] private float slowMultiplier = 0.8f;
    [SerializeField] private float slowDuration = 2f;

    [Header("Rocket Boost Settings")]
    [SerializeField] private float rocketMultiplier = 1.2f; // x3 au lieu de x2
    [SerializeField] private float rocketDuration = 1f;

    [Header("UI - Item stocké")]
    [SerializeField] private Image itemIconUI;
    [SerializeField] private Sprite speedSprite;
    [SerializeField] private Sprite slowSprite;
    [SerializeField] private Sprite rocketSprite;
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private TextMeshProUGUI activateItemText;

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
    }

    void Update()
    {
        if (currentItem != null && Input.GetKeyDown(KeyCode.Space))
        {
            UseStoredItem();
        }
    }
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

        if (activateItemText != null)
        {
            activateItemText.text = "Press [Space] to use item";
            activateItemText.gameObject.SetActive(true);

            yield return new WaitForSeconds(2f);

            activateItemText.gameObject.SetActive(false);
        }
    }

    public void ReceiveItem(ItemType item)
    {
        currentItem = item;
        UpdateUI();
    }

    private void UseStoredItem()
    {
        if (currentItem == null) return;

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
            case ItemType.CoinBoost:
                StartCoroutine(BoostCoinCount());
                break;
        }

        currentItem = null;
        UpdateUI();
    }

    IEnumerator SpeedBoostEffect()
    {
        if (playerMovement == null) yield break;

        float originalSpeed = playerMovement.maxSpeed;
        playerMovement.maxSpeed *= boostMultiplier;

        yield return new WaitForSeconds(boostDuration);
        playerMovement.maxSpeed = originalSpeed;
    }

    IEnumerator SlowDownEffect()
    {
        if (playerMovement == null) yield break;

        float originalSpeed = playerMovement.maxSpeed;
        playerMovement.maxSpeed *= slowMultiplier;

        yield return new WaitForSeconds(slowDuration);

        playerMovement.maxSpeed = originalSpeed;
    }

    IEnumerator RocketBoostEffect()
    {
        if (playerMovement == null) yield break;

        float originalSpeed = playerMovement.maxSpeed;
        playerMovement.maxSpeed *= rocketMultiplier;

        yield return new WaitForSeconds(rocketDuration);

        playerMovement.maxSpeed = originalSpeed;
    }
    IEnumerator BoostCoinCount()
    {
        // add 2 coins instantly
        GameManager.Instance.AddCoin();
        GameManager.Instance.AddCoin();
        yield break;
    }
    // --------------------------------
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
            case ItemType.CoinBoost:
                itemIconUI.sprite = coinSprite;
                break;
        }
    }
    public bool HasItem() => currentItem != null;
    public ItemType? GetCurrentItem() => currentItem;
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Header("Item Icons")]
    [SerializeField] private Sprite speedIcon;
    [SerializeField] private Sprite slowIcon;
    [SerializeField] private Sprite rocketIcon;

    [Header("Animation Settings")]
    [SerializeField] private float rouletteDuration = 1.3f;
    [SerializeField] private float rouletteSpeed = 0.05f;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        if (itemIcon == null)
            Debug.LogError("❌ UIItemDisplay : itemIcon n'est pas assigné !");

        if (itemNameText == null)
            Debug.LogError("❌ UIItemDisplay : itemNameText n'est pas assigné !");

        if (itemNameText != null)
            itemNameText.alpha = 0f;

        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.enabled = true;
        }

        gameObject.SetActive(false);
    }

    public void ShowItemRandom(ItemType finalItem)
    {
        if (showDebugLogs)
            Debug.Log($"🎰 Roulette démarrée pour : {finalItem}");

        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ItemRoulette(finalItem));
    }

    private IEnumerator ItemRoulette(ItemType finalItem)
    {
        if (itemNameText != null)
            itemNameText.alpha = 0f;

        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        float elapsed = 0f;

        // ROULETTE
        while (elapsed < rouletteDuration)
        {
            Sprite randomSprite = GetRandomIcon();

            if (itemIcon != null && randomSprite != null)
            {
                itemIcon.sprite = randomSprite;
                itemIcon.enabled = true;
            }

            yield return new WaitForSeconds(rouletteSpeed);
            elapsed += rouletteSpeed;
        }

        // Affiche l'icône finale
        Sprite finalSprite = GetIcon(finalItem);
        if (itemIcon != null && finalSprite != null)
        {
            itemIcon.sprite = finalSprite;
            if (showDebugLogs)
                Debug.Log($"✅ Icône finale : {finalSprite.name}");
        }

        // Affiche le texte avec fade-in
        if (itemNameText != null)
        {
            itemNameText.text = GetItemName(finalItem);

            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                itemNameText.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
                yield return null;
            }
            itemNameText.alpha = 1f;
        }

        yield return new WaitForSeconds(5f);

        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        gameObject.SetActive(false);
    }

    Sprite GetRandomIcon()
    {
        int r = Random.Range(0, 3);
        switch (r)
        {
            case 0: return speedIcon;
            case 1: return slowIcon;
            default: return rocketIcon;
        }
    }

    Sprite GetIcon(ItemType item)
    {
        return item switch
        {
            ItemType.SpeedBoost => speedIcon,
            ItemType.SlowDown => slowIcon,
            ItemType.Rocket => rocketIcon,
            _ => null
        };
    }

    string GetItemName(ItemType item)
    {
        return item switch
        {
            ItemType.SpeedBoost => "Speed",
            ItemType.SlowDown => "Slow",
            ItemType.Rocket => "Rocket",
            _ => "Unknown"
        };
    }

    IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
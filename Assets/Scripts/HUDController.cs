using JetBrains.Annotations;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI eText;
    [SerializeField] private TextMeshProUGUI endText;
    private float nextPulseTime;
    public float pulseCooldown = 1f;

    [Header("Press E Text")]
    [SerializeField] private float eTextFontSize = 24f;

    [Header("Knockback Cooldown Bar")]
    [Range(0.1f, 1f)]
    [SerializeField] private float barScreenFraction = 0.45f; // portion of screen width the bar covers
    [SerializeField] private float barHeight = 14f;
    [SerializeField] private float barYOffset = 30f;      // distance from bottom of screen
    [SerializeField] private float barPadding = 2f;       // gap between frame and fill
    [SerializeField] private float drainDuration = 0.12f; // how fast it empties after pressing E
    [SerializeField] private Color barBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color barFillColor = new Color(1f, 1f, 1f, 1f); // opaque white

    private GameObject barRoot;
    private RectTransform barFillRect;
    private float lastPressTime = -999f;

    private void Awake()
    {
        //Initialize UI on start
        PositionTopLeft();
        PositionTopRight();
        PositionBottomMiddle();
        PositionCenter();
        UpdateLives(0);
        UpdateEnemies(0);
        UpdateE();
        CreateCooldownBar();

        //end message stays hidden until the game ends
        if (endText != null)
            endText.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && Time.time >= nextPulseTime)
        {
            eText.text = "";
            nextPulseTime = Time.time + pulseCooldown;
            lastPressTime = Time.time;
        }
        if (Time.time > nextPulseTime)
        {
            UpdateE();
        }

        UpdateCooldownBar();
    }

    // Builds cooldown bar at runtime. Its width is a fraction of the screen, so it scales with resolution.
    private void CreateCooldownBar()
    {
        if (eText == null) return;

        // Frame / background
        barRoot = new GameObject("KnockbackBar", typeof(RectTransform), typeof(Image));
        barRoot.transform.SetParent(eText.rectTransform.parent, false);

        RectTransform bgRect = barRoot.GetComponent<RectTransform>();
        float half = barScreenFraction * 0.5f;
        bgRect.anchorMin = new Vector2(0.5f - half, 0);
        bgRect.anchorMax = new Vector2(0.5f + half, 0);
        bgRect.pivot = new Vector2(0.5f, 0);
        bgRect.sizeDelta = new Vector2(0f, barHeight);   // width comes from the anchors
        bgRect.anchoredPosition = new Vector2(0f, barYOffset);

        Image bg = barRoot.GetComponent<Image>();
        bg.color = barBackgroundColor;
        bg.raycastTarget = false;

        // Fill area: the frame minus padding
        GameObject areaGO = new GameObject("FillArea", typeof(RectTransform));
        areaGO.transform.SetParent(barRoot.transform, false);
        RectTransform areaRect = areaGO.GetComponent<RectTransform>();
        areaRect.anchorMin = Vector2.zero;
        areaRect.anchorMax = Vector2.one;
        areaRect.offsetMin = new Vector2(barPadding, barPadding);
        areaRect.offsetMax = new Vector2(-barPadding, -barPadding);

        // Fill: grows/shrinks from the center by moving its anchors
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(areaRect, false);

        barFillRect = fillGO.GetComponent<RectTransform>();
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
        SetFillAmount(1f);

        Image fill = fillGO.GetComponent<Image>();
        fill.color = barFillColor;
        fill.raycastTarget = false;
    }

    private void SetFillAmount(float amount)
    {
        float half = Mathf.Clamp01(amount) * 0.5f;
        barFillRect.anchorMin = new Vector2(0.5f - half, 0f);
        barFillRect.anchorMax = new Vector2(0.5f + half, 1f);
    }

    // Full -> quickly drains -> refills from the middle outward, timed when E is ready again.
    private void UpdateCooldownBar()
    {
        if (barFillRect == null) return;

        float drain = Mathf.Min(drainDuration, pulseCooldown * 0.5f);
        float refill = Mathf.Max(0.01f, pulseCooldown - drain);
        float elapsed = Time.time - lastPressTime;

        float amount;
        if (elapsed < drain)
        {
            // Emptying, ease in so it snaps away
            float t = elapsed / drain;
            amount = 1f - t * t;
        }
        else if (elapsed < drain + refill)
        {
            // Refilling from the center outward (linear = honest cooldown readout)
            amount = (elapsed - drain) / refill;
        }
        else
        {
            amount = 1f;
        }

        SetFillAmount(amount);
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = "Lives: " + lives;
        else
            UnityEngine.Debug.LogError("HUDController: livesText is not assigned in the Inspector.");
    }

    public void UpdateEnemies(int defeated)
    {
        if (enemiesText != null)
            enemiesText.text = "Enemies Defeated: " + defeated;
        else
            UnityEngine.Debug.LogError("HUDController: enemiesText is not assigned in the Inspector.");
    }

    public void UpdateE()
    {
        if (eText != null)
            eText.text = "Press E to knockback";
        else
            UnityEngine.Debug.LogError("HUDController: eText is not assigned in the Inspector.");
    }

    public void ShowEndMessage(string message)
    {
        //hide every other element
        if (livesText != null) livesText.gameObject.SetActive(false);
        if (enemiesText != null) enemiesText.gameObject.SetActive(false);
        if (eText != null) eText.gameObject.SetActive(false);
        if (barRoot != null) barRoot.SetActive(false);

        if (endText != null)
        {
            endText.text = message;
            endText.gameObject.SetActive(true);
        }
        else
        {
            UnityEngine.Debug.LogError("HUDController: endText is not assigned in the Inspector.");
        }
    }

    public void PositionTopLeft(float xOffset = 20f, float yOffset = -20f)
    {
        livesText.rectTransform.anchorMin = new Vector2(0, 1);
        livesText.rectTransform.anchorMax = new Vector2(0, 1);
        livesText.rectTransform.pivot = new Vector2(0, 1);

        livesText.rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
    }

    public void PositionTopRight(float xOffset = -20f, float yOffset = -20f)
    {
        enemiesText.rectTransform.anchorMin = new Vector2(1, 1);
        enemiesText.rectTransform.anchorMax = new Vector2(1, 1);
        enemiesText.rectTransform.pivot = new Vector2(1, 1);

        enemiesText.rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
    }

    public void PositionBottomMiddle(float xOffset = 0f, float yOffset = 35f)
    {
        eText.rectTransform.anchorMin = new Vector2(0.5f, 0);
        eText.rectTransform.anchorMax = new Vector2(0.5f, 0);
        eText.rectTransform.pivot = new Vector2(0.5f, 0);
        eText.rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
        eText.rectTransform.sizeDelta = new Vector2(600f, eTextFontSize * 1.5f);

        eText.fontSize = eTextFontSize;
        eText.enableAutoSizing = false;
        eText.alignment = TextAlignmentOptions.Center;
    }

    public void PositionCenter()
    {
        if (endText == null) return;

        endText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        endText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        endText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        endText.rectTransform.anchoredPosition = Vector2.zero;

        endText.alignment = TextAlignmentOptions.Center;
        endText.fontSize = 75f;
    }
}
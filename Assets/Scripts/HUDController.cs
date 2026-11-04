using JetBrains.Annotations;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI eText;
    [SerializeField] private TextMeshProUGUI endText;
    private float nextPulseTime;
    public float pulseCooldown = 1f;

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

        //end message stays hidden until the game ends
        if (endText != null)
            endText.gameObject.SetActive(false);
    }

    public void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && Time.time >= nextPulseTime)
        {
            eText.text = "";
            nextPulseTime = Time.time + pulseCooldown;
        }
        if (Time.time > nextPulseTime)
        {
            UpdateE();
        }
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
            UnityEngine.Debug.LogError("HUDController: enemiesText is not assigned in the Inspector.");
    }

    public void ShowEndMessage(string message)
    {
        //hide every other element
        if (livesText != null) livesText.gameObject.SetActive(false);
        if (enemiesText != null) enemiesText.gameObject.SetActive(false);
        if (eText != null) eText.gameObject.SetActive(false);

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

    public void PositionBottomMiddle(float xOffset = -20f, float yOffset = 35f)
    {
        eText.rectTransform.anchorMin = new Vector2(0.5f, 0);
        eText.rectTransform.anchorMax = new Vector2(0.5f, 0);
        eText.rectTransform.pivot = new Vector2(0.5f, 0);

        eText.rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
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
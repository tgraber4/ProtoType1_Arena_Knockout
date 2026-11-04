using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance for easy global access
    public static GameManager Instance { get; private set; }

    public enum GameStatus
    {
        Playing,
        Won,
        Lost
    }

    [Header("Game Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int targetEnemies = 5;

    [Header("Game State")]
    [SerializeField] private GameStatus currentStatus = GameStatus.Playing;
    private int currentLives;
    private int enemiesDefeated = 0;

    [Header("References")]
    [SerializeField] private HUDController hud;

    public GameStatus CurrentStatus => currentStatus;

    private void Awake()
    {
        // Enforce Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //make sure time runs if the scene was reloaded after a game end
        Time.timeScale = 1f;

        currentLives = startingLives;
        currentStatus = GameStatus.Playing;

        // Find HUD if not assigned in the Inspector
        if (hud == null)
        {
            hud = FindFirstObjectByType<HUDController>();
            if (hud == null)
            {
                UnityEngine.Debug.LogWarning("GameManager: No HUDController found in the scene.");
            }
        }

        // Initialize HUD display
        UpdateHUD();
    }

    public void EnemyDefeated()
    {
        // Do nothing if the game is already over
        if (currentStatus != GameStatus.Playing) return;

        enemiesDefeated++;
        UpdateHUD();

        UnityEngine.Debug.Log("Enemies Defeated: " + enemiesDefeated + " / " + targetEnemies);

        if (enemiesDefeated >= targetEnemies)
        {
            WinGame();
        }
    }

    public void LoseLife()
    {
        // Do nothing if the game is already over
        if (currentStatus != GameStatus.Playing) return;

        currentLives--;
        UpdateHUD();

        UnityEngine.Debug.Log("Lives remaining: " + currentLives);

        if (currentLives <= 0)
        {
            LoseGame();
        }
    }

    private void UpdateHUD()
    {
        if (hud != null)
        {
            hud.UpdateLives(currentLives);
            hud.UpdateEnemies(enemiesDefeated);
        }
    }

    private void WinGame()
    {
        currentStatus = GameStatus.Won;
        UnityEngine.Debug.Log("YOU WIN!");

        //ui first then freeze
        if (hud != null)
            hud.ShowEndMessage("YOU WIN");

        Time.timeScale = 0f;
    }

    private void LoseGame()
    {
        currentStatus = GameStatus.Lost;
        UnityEngine.Debug.Log("GAME OVER");

        //ui first then freeze
        if (hud != null)
            hud.ShowEndMessage("YOU LOSE");

        Time.timeScale = 0f;
    }
}
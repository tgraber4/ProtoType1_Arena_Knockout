using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GameManager : MonoBehaviour
{

    public int targetScore = 3;
    public int startingLife = 3;

    private int currentScore = 0;
    private int currentLife;

    public bool isPlaying = true;
    public enum GameStatus
    {
        Playing,
        Won,
        Lost
    }
    public GameStatus currentStatus;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStatus = GameStatus.Playing;
        currentLife = startingLife;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore()
    {
        if (currentStatus != GameStatus.Playing)
        {
            return;
        }

        currentScore++;

        Debug.Log(
            "Score: " +
            currentScore +
            " / " +
            targetScore
        );

        if (currentScore >= targetScore)
        {
            WinGame();
        }
    }

    public void LoseLife()
    {
        if (currentStatus != GameStatus.Playing)
        {
            return;
        }

        currentLife--;

        Debug.Log(
            "Lives: " +
            currentLife
        );

        if (currentLife <= 0)
        {
            LoseGame();
        }
    }

    void WinGame()
    {
        currentStatus = GameStatus.Won;

        Debug.Log("YOU WIN!");
    }

    void LoseGame()
    {
        currentStatus = GameStatus.Lost;

        Debug.Log("GAME OVER");
    }
}

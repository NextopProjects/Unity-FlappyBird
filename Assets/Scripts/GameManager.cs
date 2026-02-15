using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private int score = 0;
    private static int bestScore = 0;

    public void AddScore(int value = 1)
    {
        score += value;

        if (score > bestScore)
            bestScore = score;
    }

    public void GameOver()
    {
        Time.timeScale = 0;
    }

    public void Restart()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }
}

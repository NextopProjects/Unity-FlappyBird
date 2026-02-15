using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    private AIAgent aiAgent;

    public int score = 0;

    /*
        최고 점수는 static으로 선언한다.
        씬이 다시 로드되어도 값이 유지된다.
    */
    private static int bestScore = 0;

    public float timeScale = 3f;

    private bool isGameOver = false;

    void Start()
    {
        aiAgent = FindFirstObjectByType<AIAgent>();
        Time.timeScale = timeScale;
    }

    void Update()
    {
        if (statsText == null || aiAgent == null) return;

        statsText.text =
            "Score: " + score + "\n" +
            "Best Score: " + bestScore + "\n" +
            "Episode: " + aiAgent.episodeCount + "\n" +
            "Epsilon: " + aiAgent.epsilonValue.ToString("F3") + "\n" +
            "Q-States: " + aiAgent.qTableSize;
    }

    // 점수 증가
    public void AddScore(int value = 1)
    {
        score += value;

        if (score > bestScore)
            bestScore = score;
    }

    // 게임 오버 시 현재 씬 재로드
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name);
    }
}
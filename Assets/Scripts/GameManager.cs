using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI statsText;
    public Button restartButton;
    
    // AI 학습용 설정
    public bool autoRestart = true;
    public float restartDelay = 0.5f;
    public float timeScale = 3f;

    public int score = 0;
    private bool isGameOver = false;

    void Start()
    {
        if (statsText != null)
        {
            statsText.text = "";
        }
        restartButton.gameObject.SetActive(false);
        restartButton.onClick.AddListener(Restart);
        isGameOver = false;
        
        Time.timeScale = timeScale;
    }

    void Update()
    {
        if (statsText != null)
        {
            AIAgent aiAgent = FindFirstObjectByType<AIAgent>();
            if (aiAgent != null)
            {
                statsText.text = $"Score: {score}\n" +
                                 $"Episode: {aiAgent.episodeCount}\n" +
                                 $"Best Score: {aiAgent.bestScoreValue}\n" +
                                 $"Epsilon: {aiAgent.epsilonValue:F3}\n" +
                                 $"Q-States: {aiAgent.qTableSize}";
            }
        }
    }

    public void AddScore(int value = 1)
    {
        score += value;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        if (autoRestart)
        {
            Invoke(nameof(Restart), restartDelay);
        }
        else
        {
            restartButton.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void Restart()
    {
        Time.timeScale = timeScale;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}
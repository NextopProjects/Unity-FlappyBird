using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public Button restartButton;

    private int score = 0;

    void Start()
    {
        scoreText.text = "Score: 0";
        restartButton.gameObject.SetActive(false);
        restartButton.onClick.AddListener(Restart);
    }

    public void AddScore(int value = 1)
    {
        score += value;
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        restartButton.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void Restart()
    {
        Time.timeScale = 1; // 시간 정상화
        
        // 현재 활성 씬 이름 가져오기
        string currentScene = SceneManager.GetActiveScene().name;

        // 현재 씬 다시 로드
        SceneManager.LoadScene(currentScene);
    }
}
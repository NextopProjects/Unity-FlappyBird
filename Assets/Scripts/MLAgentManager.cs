using TMPro;
using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// ML-Agent 학습 관리자
/// 
/// [주요 역할]
/// 1. 환경 리셋: 에피소드 종료 시 파이프 제거
/// 2. 학습 속도 조절: TimeScale 설정
/// 3. 통계 추적: 최고 점수, 평균 점수
/// 
/// [사용 방법]
/// 1. 빈 게임 오브젝트에 이 스크립트 추가
/// 2. FlappyBirdAgent 연결
/// 3. PipeSpawner 연결
/// </summary>
public class MLAgentManager : MonoBehaviour
{
    // ========================================
    // Inspector 설정
    // ========================================
    
    [Header("에이전트 연결")]
    public FlappyBirdAgent agent;
    public PipeSpawner pipeSpawner;
    
    [Header("UI")]
    public TextMeshProUGUI statsText;
    
    [Header("학습 설정")]
    public float timeScale = 1f;
    public bool autoReset = true;
    
    // ========================================
    // 내부 변수
    // ========================================
    private int _episodeCount;
    private float _bestScore;
    private float _totalScore;
    private float _lastScore;
    
    // ========================================
    // 프로퍼티
    // ========================================
    public int EpisodeCount => _episodeCount;
    public float BestScore => _bestScore;
    public float AverageScore => _episodeCount > 0 ? _totalScore / _episodeCount : 0f;
    
    // ========================================
    // 초기화
    // ========================================
    void Start()
    {
        Time.timeScale = timeScale;
        _episodeCount = 0;
        _bestScore = 0f;
        _totalScore = 0f;
        _lastScore = 0f;
    }
    
    // ========================================
    // UI 업데이트
    // ========================================
    void Update()
    {
        if (statsText != null)
        {
            statsText.text = $"Best: {_bestScore:F0}\n" +
                             $"Avg: {AverageScore:F1}\n" +
                             $"Episodes: {_episodeCount}\n" +
                             $"TimeScale: {timeScale:F1}x";
        }
    }
    
    // ========================================
    // 에피소드 종료 처리
    // ========================================
    public void OnEpisodeEnd(float score)
    {
        _episodeCount++;
        _lastScore = score;
        _totalScore += score;
        
        if (score > _bestScore)
        {
            _bestScore = score;
        }
        
        if (autoReset)
        {
            ResetEnvironment();
        }
    }
    
    // ========================================
    // 환경 리셋
    // ========================================
    public void ResetEnvironment()
    {
        DestroyAllPipes();
        
        if (pipeSpawner != null)
        {
            pipeSpawner.ResetTimer();
        }
    }
    
    // ========================================
    // 모든 파이프 제거
    // ========================================
    private void DestroyAllPipes()
    {
        PipeMovement[] pipes = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        foreach (var pipe in pipes)
        {
            Destroy(pipe.gameObject);
        }
    }
    
    // ========================================
    // 게임 속도 설정
    // ========================================
    public void SetTimeScale(float value)
    {
        timeScale = value;
        Time.timeScale = timeScale;
    }
    
    // ========================================
    // 학습 속도 증가
    // ========================================
    public void IncreaseTimeScale()
    {
        SetTimeScale(Mathf.Min(timeScale + 1f, 20f));
    }
    
    // ========================================
    // 학습 속도 감소
    // ========================================
    public void DecreaseTimeScale()
    {
        SetTimeScale(Mathf.Max(timeScale - 1f, 1f));
    }
}

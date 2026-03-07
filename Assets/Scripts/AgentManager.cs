using TMPro;
using UnityEngine;

/// <summary>
/// AgentManager - 에피소드 관리 및 Epsilon 감소 담당
///
/// [주요 역할]
/// 1. 싱글톤: AgentManager.Instance로 전역 접근
/// 2. 에피소드 관리: 게임오버 시 환경 리셋
/// 3. Epsilon 지수 감소: Flappy Bird 학습 특성에 최적화
/// 4. UI 갱신: 학습 현황 실시간 표시
///
/// [지수 감소를 선택한 이유 - Flappy Bird 특성 기반]
///
///  Flappy Bird는 초반에 파이프 통과 경험 자체가 매우 희귀하다.
///  에이전트가 올바른 패턴을 학습하려면 초반에 탐색을 충분히 확보해야 하고,
///  한번 패턴을 익히면 수렴이 빠른 특성이 있다.
///
///  선형 감소: 초반부터 탐색이 빠르게 줄어 파이프 통과 경험 부족 → 학습 불안정
///  지수 감소: 초반 탐색을 길게 유지 → 파이프 통과 경험 축적 → 이후 빠르게 수렴
///
///  ε = max(minEpsilon, initialEpsilon × decayRate^episode)
///
///  [decayRate 0.9994 기준 감소 곡선]
///  Episode    0 : ε ≈ 1.000
///  Episode 1000 : ε ≈ 0.549  (탐색 위주, 파이프 통과 경험 축적)
///  Episode 2000 : ε ≈ 0.301  (탐색·활용 균형)
///  Episode 4000 : ε ≈ 0.091  (학습된 패턴 강화)
///  Episode 5000 : ε ≈ 0.050  (minEpsilon 수렴, 활용 중심)
/// </summary>
public class AgentManager : MonoBehaviour
{
    // ==============================
    // 싱글톤
    // ==============================
    public static AgentManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==============================
    // Inspector 설정
    // ==============================
    [Header("에이전트 연결")]
    public PlayerAgent     agent;
    public TextMeshProUGUI infoText;

    [Header("학습 속도")]
    public float timeScale = 1f;

    [Header("Epsilon 설정")]
    public float minEpsilon  = 0.05f;
    [Tooltip("에피소드마다 곱해지는 감소율.\n" +
             "0.9994 권장: 약 5000회에서 minEpsilon에 수렴\n" +
             "※ 수렴 에피소드 계산: ln(minEpsilon/init) / ln(decayRate)")]
    public float decayRate   = 0.9994f;

    // ==============================
    // 내부 변수
    // ==============================
    private int   _episodeCount;
    private float _initialEpsilon;
    private int   _bestScore;

    // ==============================
    // 생명주기
    // ==============================
    void Start()
    {
        Time.timeScale  = timeScale;
        _episodeCount   = 0;
        _initialEpsilon = agent.GetEpsilon();
    }

    void Update()
    {
        UpdateUI();
    }

    // ==============================
    // 에피소드 관리
    // ==============================

    /// <summary>
    /// PlayerAgent의 OnCollisionEnter에서 호출.
    /// Epsilon 감소 → 환경 리셋 순서로 처리.
    /// </summary>
    public void EndEpisode()
    {
        _episodeCount++;
        DecayEpsilon();
        ResetEpisode();
    }

    /// <summary>
    /// 지수 감소: ε = max(minEpsilon, initialEpsilon × decayRate^episode)
    /// 초반 탐색을 충분히 유지하다가 경험이 쌓인 후 빠르게 수렴.
    /// </summary>
    void DecayEpsilon()
    {
        float newEpsilon = _initialEpsilon * Mathf.Pow(decayRate, _episodeCount);
        agent.SetEpsilon(Mathf.Max(minEpsilon, newEpsilon));
    }

    void ResetEpisode()
    {
        agent.Reset();
        DestroyAllPipes();
    }

    void DestroyAllPipes()
    {
        PipeMovement[] pipes = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        foreach (var pipe in pipes)
            Destroy(pipe.gameObject);
    }

    // ==============================
    // UI
    // ==============================
    void UpdateUI()
    {
        if (infoText == null) return;

        if (agent.Score > _bestScore)
            _bestScore = agent.Score;

        infoText.text = $"BestScore: {_bestScore}\n" +
                        $"Score: {agent.Score}\n"     +
                        $"Episode: {_episodeCount}\n" +
                        $"Epsilon: {agent.GetEpsilon():F3}";
    }

    // ==============================
    // 유틸리티
    // ==============================
    public void SetTimeScale(float value)
    {
        timeScale      = value;
        Time.timeScale = value;
    }
}
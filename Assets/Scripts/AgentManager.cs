using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance { get; private set; }

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public PlayerAgent agent;
    public TextMeshProUGUI infoText;

    public float timeScale = 1f;

    public float minEpsilon = 0.05f; // 최저 학습 확률 임게치
    public float decayRate = 0.999f; // 엡실론 감소 속도
    
    private int   _episodeCount;
    private float _initialEpsilon;
    private int   _bestScore;
    
    // 강화 학습이 시작을 위한 초기화
    void Start()
    {
        Time.timeScale = timeScale;
        _episodeCount = 0;

        _initialEpsilon = agent.Epsilon;
        
        // 에피소드
    }

    
    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (infoText == null) return;
        if (agent.Score > _bestScore)
        {
            _bestScore = agent.Score;
        }
        infoText.text = $"BestScore: {_bestScore}\n" +
                        $"Score: {agent.Score}\n" +
                        $"Episodes: {_episodeCount}\n" + 
                        $"Epsilon: {agent.Epsilon:F3}\n";
        
    }
    
    
    public void EndEpisode()
    {
        _episodeCount++;
        DecayEpsilon();
        ResetEpisode();
    }

    void DecayEpsilon()
    {
        float newEpsilon = _initialEpsilon * Mathf.Pow(decayRate,_episodeCount);
        // agent.Epsilon = Mathf.Max(minEpsilon, newEpsilon);
    }
    
    

    void ResetEpisode()
    {
        agent.Reset();
        DestroyAllPipes();
    }

    void DestroyAllPipes()
    {
        PipeMovement[] pipes = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        foreach (PipeMovement pipe in pipes)
        {
            Destroy(pipe.gameObject);
        }
    }
}

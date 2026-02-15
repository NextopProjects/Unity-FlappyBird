using UnityEngine;
using System.Collections.Generic;

public class AIAgent : MonoBehaviour
{
    // ===== Q-learning 데이터 (씬 재로드 시에도 유지) =====
    private static Dictionary<string, float[]> qTable = new Dictionary<string, float[]>();
    private static int totalEpisodes = 0;
    private static int bestScore = 0;
    private static float epsilon = 1.0f;
    
    // ===== 학습 파라미터 =====
    public float learningRate = 0.2f;        // 학습률 증가 (더 빠른 학습)
    public float discountFactor = 0.99f;     // 할인율 증가 (미래 보상 중시)
    public float epsilonDecay = 0.995f;
    public float minEpsilon = 0.01f;
    
    // ===== 게임 참조 =====
    private Rigidbody rb;
    private GameManager gameManager;
    
    // ===== 현재 에피소드 데이터 =====
    private string previousState;
    private int previousAction;
    private bool episodeEnded = false;
    
    // ===== UI 접근용 =====
    public int episodeCount => totalEpisodes;
    public int bestScoreValue => bestScore;
    public float epsilonValue => epsilon;
    public int qTableSize => qTable.Count;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        episodeEnded = false;
    }

    void FixedUpdate()
    {
        if (episodeEnded) return;
        
        // 상태 관찰
        string currentState = GetState();
        
        // Q-Table 초기화
        if (!qTable.ContainsKey(currentState))
        {
            qTable[currentState] = new float[2];
        }
        
        // 행동 선택
        int action = ChooseAction(currentState);
        
        // 행동 실행
        if (action == 1)
        {
            rb.linearVelocity = Vector3.up * 5f;
        }
        
        // Q-learning 업데이트 (매 프레임 - 학습 정확도 향상)
        if (previousState != null)
        {
            float reward = CalculateReward();
            UpdateQValue(previousState, previousAction, reward, currentState);
        }
        
        // 상태 저장
        previousState = currentState;
        previousAction = action;
    }

    string GetState()
    {
        GameObject pipe = FindClosestPipe();
        
        // 상태 공간 세분화 (정확도 향상)
        int playerY = Mathf.Clamp(Mathf.FloorToInt((transform.position.y + 5f) / 1.0f), 0, 9);
        int velocity = Mathf.Clamp(Mathf.FloorToInt((rb.linearVelocity.y + 10f) / 2.5f), 0, 7);
        
        int pipeDist = 8;
        int pipeHeight = 8;
        
        if (pipe != null)
        {
            float dist = pipe.transform.position.x - transform.position.x;
            pipeDist = Mathf.Clamp(Mathf.FloorToInt(dist / 2f), 0, 7);
            
            float heightDiff = pipe.transform.position.y - transform.position.y;
            pipeHeight = Mathf.Clamp(Mathf.FloorToInt((heightDiff + 8f) / 2f), 0, 7);
        }
        
        return $"{playerY}_{velocity}_{pipeDist}_{pipeHeight}";
    }

    GameObject FindClosestPipe()
    {
        PipeMovement[] pipes = FindObjectsOfType<PipeMovement>();
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var pipe in pipes)
        {
            if (pipe.transform.position.x < transform.position.x - 1.0f) continue;

            float dist = pipe.transform.position.x - transform.position.x;
            if (dist < minDist)
            {
                minDist = dist;
                closest = pipe.gameObject;
            }
        }
        return closest;
    }

    int ChooseAction(string state)
    {
        if (Random.value < epsilon)
        {
            return Random.value < 0.5f ? 0 : 1;
        }
        else
        {
            float[] qValues = qTable[state];
            return qValues[0] > qValues[1] ? 0 : 1;
        }
    }

    float CalculateReward()
    {
        float reward = 0.1f;  // 생존 보상
        
        // 높이 페널티 (중앙 유지 유도)
        float yPos = transform.position.y;
        if (Mathf.Abs(yPos) > 3f)
        {
            reward -= 0.3f;  // 너무 위/아래 있으면 큰 페널티
        }
        
        return reward;
    }

    void UpdateQValue(string state, int action, float reward, string nextState)
    {
        if (!qTable.ContainsKey(nextState))
        {
            qTable[nextState] = new float[2];
        }
        
        float currentQ = qTable[state][action];
        float maxNextQ = Mathf.Max(qTable[nextState][0], qTable[nextState][1]);
        
        // Q(s,a) ← Q(s,a) + α[r + γ·max Q(s',a') - Q(s,a)]
        qTable[state][action] = currentQ + learningRate * (reward + discountFactor * maxNextQ - currentQ);
    }

    public void OnPipePass()
    {
        if (previousState != null && qTable.ContainsKey(previousState))
        {
            UpdateQValue(previousState, previousAction, 15.0f, GetState());  // 보상 증가
        }
    }

    public void OnCollision()
    {
        if (episodeEnded) return;
        episodeEnded = true;
        
        if (previousState != null && qTable.ContainsKey(previousState))
        {
            UpdateQValue(previousState, previousAction, -20.0f, previousState);  // 페널티 증가
        }
        
        totalEpisodes++;
        epsilon = Mathf.Max(minEpsilon, epsilon * epsilonDecay);
        
        int currentScore = gameManager.score;  // 리플렉션 제거
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
        }
        
        Debug.Log($"[Episode {totalEpisodes}] Score: {currentScore} | Best: {bestScore} | Epsilon: {epsilon:F3}");
    }
}
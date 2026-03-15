using System;
using UnityEngine;

public class PlayerAgent : MonoBehaviour
{
    [Header("물리 설정")]
    public float jumpForce = 5f;
    
    [Header("행동 결정 간격")]
    public float decisionInterval = 0.5f; // 몇 초마다 Action Cooltime

    [Header("Q-Learning 파라미터")]
    public float learningRate = 0.3f;
    public float discountFactor = 0.5f;
    public float initialEpsilon = 0.1f;

    [Header("보상 설정")]
    public float surviveReward = 0.05f;
    public float pipePassReward = 100f;
    public float collisionPenalty = -1000f;

    [Header("갭 근접 보상 설정")]
    public float gapNearThreshold = 0.8f;
    public float gapNearReward = 0.5f;
    public float gapMidThreshold = 2.0f;
    public float gapMidReward = 0.1f;
    public float gapFarPenalty = -0.1f;
    
    [Header("행동 제약")]
    public float maxYPosition  =  4.5f;
    public float minYPosition  =  2.0f;
    public float maxUpVelocity =  1f;
    
    
    private Rigidbody  _rigidbody;
    
    private QLearning<(int, int, int)> _qLearning;
    
    private Vector3 _initialPosition;
    
    private (int, int, int) _currentState;
    private int _currentAction;
    private float _currentReward;
    private int _currentScore;
    
    private float _timeSinceLastDecision;
    private int _stepCount = 0;
    
    private bool _isDead = false;

    public float Epsilon => _qLearning.Epsilon;
    // public void  SetEpsilon(float value) => _qLearning.Epsilon = value;
    public int Score => _currentScore;
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _qLearning = new QLearning<(int, int, int)>(2, learningRate, discountFactor, initialEpsilon);
        _initialPosition = _rigidbody.position;
    }

    void FixedUpdate() // 일관성있게 초당 60번 호출 ( 변경가능 )
    {
        if (_isDead) return;
        
        // 0.5초 마다 에이전트을 내릴 수 있게 파이프라인 구성
        _timeSinceLastDecision += Time.fixedDeltaTime;
        if (_timeSinceLastDecision >= decisionInterval)
        {
            // Step();
            _timeSinceLastDecision = 0f;
        }
    }

    // AI Agent가 어떠한 행동을 할 건지 처리하는 구간
    void Step()
    {
        _stepCount++;
        
        // 1. 보상 계산
        CalculateReward();
        _currentReward += surviveReward;
        
        // 2. 다음 상태 예측
        var nextState = GetState();
        
        // 3. 현재 상태의 상황을 Q-Table에 Update
        _qLearning.Update(_currentState, _currentAction, 
            (int)_currentReward * 100, nextState );
        
        // 4. Action 선택 + 너무 튀지 않도록 행동 제약도 처리
        _currentAction = _qLearning.GetAction(nextState);
        _currentAction = ApplyActionConstraints(_currentAction); // 천장 하단 방지를 제한 장치

        // 5. 행동 실행
        if (_currentAction == 1)
        {
            Jump();
        }
        
        // 6. 현재 상태에 대한 것을 갱신
        _currentState  = nextState;
        _currentReward = 0f;

    }

    int ApplyActionConstraints(int action)
    {
        float y = transform.position.y;
        float velocityY = _rigidbody.linearVelocity.y;

        if (y > maxYPosition || velocityY > maxUpVelocity) return 0;
        if (y < minYPosition && velocityY < 0f) return 1;
        return action;
    } 

    void CalculateReward()
    {
        GameObject closestPipe = FindClosestPipe();
        if (closestPipe == null) return;

        float gapCenterY = GetGapCenterY(closestPipe);
        float absDy = Mathf.Abs(transform.position.y - gapCenterY);

        // 플레이와 파이프의 Y 좌표의 거리 near 기준 작은 경우 해당 보상
        if (absDy < gapNearThreshold) _currentReward += gapNearReward;
        // 플레이와 파이프의 Y 좌표의 거리 mid 기준 작은 경우 해당 보상
        else if (absDy < gapMidThreshold) _currentReward += gapMidReward;
        // 너무 멀다 패널티
        else _currentReward += gapFarPenalty;
        // 최대한 가깝게 파이프 Y 좌표가 가까워지도록 유도
    }

    float GetGapCenterY(GameObject closestPipe)
    {
        float topY = closestPipe.transform.GetChild(0).position.y;
        float bottomY = closestPipe.transform.GetChild(1).position.y;
        return (topY - bottomY) / 2f;

        return closestPipe.transform.GetChild(2).position.y;
    }
    
    // 게임 오버
    void OnCollisionEnter(Collision collision)
    {
        // 해당 에이전트 게임오버 마킹
        _isDead = true;
        _currentReward = collisionPenalty;
        
        _qLearning.Update(_currentState, _currentAction, 
            (int)(_currentReward * 100), _currentState);
        AgentManager.Instance.EndEpisode();
    }
    
    // 장애물 통과
    private void OnTriggerExit(Collider other)
    {
        _currentReward += pipePassReward;
        _currentScore++;
    }

    // Action : Jump
    void Jump()
    {
        _rigidbody.linearVelocity = Vector3.up * jumpForce;
    }

    public void Reset()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        transform.position        = _initialPosition;

        _isDead                = false;
        _currentReward         = 0f;
        _currentScore          = 0;
        _timeSinceLastDecision = 0f;

        _stepCount             = 0;
        
        _currentAction = 0;
    }

    // 파이프 갭의 중심의 Y거리의 차이
    // 파이프와 에이전트간의 X의 거리
    (int, int, int) GetState()
    {
        GameObject closestPipe = FindClosestPipe();

        int dxBin = 0; // 인접한 파이프와 플레어이 간의 x 거리
        int dyBin = 0; // 파이프의 중심저 플레어어의 y 걸
        int velBin = Mathf.RoundToInt(_rigidbody.linearVelocity.y); // 현재 상태의 속도

        if (closestPipe != null)
        {
            float gapCenterY = GetGapCenterY(closestPipe);
            
            float dx = closestPipe.transform.position.x - transform.position.x;
            float dy = gapCenterY - transform.position.y;
            
            dxBin = Mathf.RoundToInt(dx);
            dyBin = Mathf.RoundToInt(dy * 2f);
            
        }
        
        return (dxBin, dyBin, velBin);
    }

    GameObject FindClosestPipe()
    {
        PipeMovement[] pipes = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var pipe in pipes)
        {
            float dist = pipe.transform.position.x - transform.position.x;
            if ( dist > 0f && dist < minDist)
            {
                minDist = dist;
                closest = pipe.gameObject;
            }
        }
        return closest;
    }
}

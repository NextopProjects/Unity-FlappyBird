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
    private float _currentScore;
    
    private float _timeSinceLastDecision;
    private int _stepCount = 0;
    
    private bool _isDead = false;
    
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _qLearning = new QLearning<(int,int,int)>(2,0.3f, 0.0f, 1f);
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
        
        // 2. 다음 상태 예측
        
        // 3. 현재 상태의 상황을 Q-Table에 Update
        
        // 4. Action 선택 + 너무 튀지 않도록 행동 제약도 처리
        
        // 5. 행동 실행
        
        // 6. 현재 상태에 대한 것을 갱신
        
    }
    
    // 게임 오버
    void OnCollisionEnter(Collision collision)
    {
        // 해당 에이전트 게임오버 마킹
        _isDead = true;
        _currentReward = collisionPenalty;
        
        // QLearning
        // TODO: QLearning Qtable 게임오버에 대한 상태를 Update
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
    (int, int) GetState()
    {
        GameObject closestPipe = FindClosestPipe();
        float distY = 0f;
        if (closestPipe != null)
        {
            float gapCenterY = closestPipe.transform.position.y;
            distY = Math.Abs(transform.position.y - gapCenterY);
            
            
            // TODO: 260309 이어서 코드 작성해야하는 부분
        }
        return (0, 0);// TODO: 260309 이어서 코드 작성해야하는 부분
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

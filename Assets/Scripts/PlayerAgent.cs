using UnityEngine;

/// <summary>
/// PlayerAgent - Q-Learning 기반 Flappy Bird 에이전트
///
/// [상태 정의] - PlayerController 로직 기반
/// state = (dyBin, dxBin, velBin)
/// dyBin  : 갭 중심과의 Y 차이 (0.5 단위, -8~8  → 17개 bin)
/// dxBin  : 파이프까지 X 거리  (1.0 단위,  0~10 → 11개 bin)
/// velBin : Y 속도             (반올림 정수, -5~5 → 11개 bin)
///
/// [PlayerController → PlayerAgent 이식 항목]
/// - 상태 이산화 기준: dx 1.0 단위, dy 0.5 단위, vel 반올림 정수 (PlayerController 방식)
/// - 갭 중심 계산: 상/하 자식 평균 Y 유지 (PlayerAgent 방식 유지)
/// - 보상 Shaping: 구간별 보상 + 파이프 통과 보상 + 사망 페널티 (PlayerController 방식)
/// - 행동 결정: JumpCooldown → DecisionInterval (일정 간격마다 판단)
/// </summary>
public class PlayerAgent : MonoBehaviour
{
    [Header("물리 설정")]
    public float jumpForce = 5f;

    [Header("행동 결정 간격")]
    public float decisionInterval = 0.05f;

    [Header("Q-Learning 파라미터")]
    public float learningRate   = 0.3f;
    public float discountFactor = 0.9f;
    public float initialEpsilon = 1f;

    [Header("보상 설정")]
    public float surviveReward    =  0.05f;
    public float pipePassReward   =  100f;
    public float collisionPenalty = -1000f;

    [Header("갭 근접 보상 구간")]
    public float gapNearThreshold = 0.8f;
    public float gapNearReward    = 0.5f;
    public float gapMidThreshold  = 2.0f;
    public float gapMidReward     = 0.1f;
    public float gapFarPenalty    = -0.1f;

    [Header("행동 제약")]
    public float maxYPosition  =  4.5f;
    public float minYPosition  =  2.0f;
    public float maxUpVelocity =  1f;

    private Rigidbody    _rigidbody;
    private AgentManager _agentManager;

    private QLearning<(int, int, int)> _qLearning;

    private Vector3         _initialPosition;
    private (int, int, int) _currentState;
    private int             _currentAction;

    private bool  _isDead;
    private float _currentReward;
    private int   _currentScore;
    private float _timeSinceLastDecision;

    public int Score => _currentScore;

    // ==============================
    // 생명주기
    // ==============================
    void Start()
    {
        _rigidbody    = GetComponent<Rigidbody>();
        _agentManager = FindFirstObjectByType<AgentManager>();

        _qLearning = new QLearning<(int, int, int)>(
            actionCount:    2,
            learningRate:   learningRate,
            discountFactor: discountFactor,
            epsilon:        initialEpsilon
        );

        _initialPosition = transform.position;
        _currentState    = GetState();
        _isDead          = false;
    }

    void FixedUpdate()
    {
        if (_isDead) return;

        _timeSinceLastDecision += Time.fixedDeltaTime;
        if (_timeSinceLastDecision >= decisionInterval)
        {
            Step();
            _timeSinceLastDecision = 0f;
        }
    }

    void Step()
    {
        // 1. 보상 계산
        CalculateReward();
        _currentReward += surviveReward;

        // 2. 다음 상태 관측
        var nextState = GetState();

        // 3. Q-테이블 업데이트
        _qLearning.Update(_currentState, _currentAction, (int)(_currentReward * 100), nextState);

        // 4. 행동 선택 + 제약 적용
        _currentAction = _qLearning.GetAction(nextState);
        _currentAction = ApplyActionConstraints(_currentAction);

        // 5. 행동 실행
        if (_currentAction == 1)
        {
            Jump();
        }

        // 6. 상태·보상 갱신
        _currentState  = nextState;
        _currentReward = 0f;
    }

    // ==============================
    // 상태 정의 (PlayerController 이산화 기준 적용)
    // ==============================

    /// <summary>
    /// PlayerController 이산화 기준 이식:
    ///   dx: Mathf.FloorToInt(dx)         — 1.0 단위
    ///   dy: Mathf.FloorToInt(dy * 2)     — 0.5 단위
    ///   vel: Mathf.RoundToInt(velocity.y) — 반올림 정수
    /// 갭 중심 계산은 PlayerAgent의 GetGapCenterY() 유지
    /// </summary>
    (int, int, int) GetState()
    {
        GameObject closestPipe = FindClosestPipe();

        int dxBin  = 0;
        int dyBin  = 0;
        int velBin = Mathf.RoundToInt(_rigidbody.linearVelocity.y);

        if (closestPipe != null)
        {
            float gapCenterY = GetGapCenterY(closestPipe);

            float dx = closestPipe.transform.position.x - transform.position.x;
            float dy = gapCenterY - transform.position.y;

            dxBin = Mathf.FloorToInt(dx);
            dyBin = Mathf.FloorToInt(dy * 2f);
        }

        return (dxBin, dyBin, velBin);
    }

    GameObject FindClosestPipe()
    {
        PipeMovement[] pipes   = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        GameObject     closest = null;
        float          minDist = Mathf.Infinity;

        foreach (var pipe in pipes)
        {
            float dist = pipe.transform.position.x - transform.position.x;
            if (dist > 0f && dist < minDist)
            {
                minDist = dist;
                closest = pipe.gameObject;
            }
        }

        return closest;
    }

    float GetGapCenterY(GameObject pipe)
    {
        if (pipe.transform.childCount >= 2)
        {
            float topY    = pipe.transform.GetChild(0).position.y;
            float bottomY = pipe.transform.GetChild(1).position.y;
            return (topY + bottomY) / 2f;
        }
        return pipe.transform.position.y;
    }

    // ==============================
    // 행동
    // ==============================
    void Jump()
    {
        _rigidbody.linearVelocity = Vector3.up * jumpForce;
    }

    /// <summary>
    /// PlayerController의 천장 캠핑 방지 로직을 행동 제약으로 이식.
    /// 보상 페널티 대신 행동 자체를 강제해 Q값 왜곡을 방지한다.
    /// </summary>
    int ApplyActionConstraints(int action)
    {
        float y         = transform.position.y;
        float velocityY = _rigidbody.linearVelocity.y;

        if (y > maxYPosition || velocityY > maxUpVelocity)
            return 0;

        if (y < minYPosition && velocityY < 0f)
            return 1;

        return action;
    }

    // ==============================
    // 보상 (PlayerController Reward Shaping 이식)
    // ==============================

    /// <summary>
    /// PlayerController의 구간별 보상 방식 이식:
    ///   갭과의 거리(absDy)에 따라 3단계 보상/페널티 부여.
    ///   Inspector에서 각 구간 임계값·보상값 튜닝 가능.
    /// </summary>
    void CalculateReward()
    {
        GameObject closestPipe = FindClosestPipe();
        if (closestPipe == null) return;

        float gapCenterY = GetGapCenterY(closestPipe);
        float absDy      = Mathf.Abs(transform.position.y - gapCenterY);

        if      (absDy < gapNearThreshold) _currentReward += gapNearReward;
        else if (absDy < gapMidThreshold)  _currentReward += gapMidReward;
        else                               _currentReward += gapFarPenalty;
    }

    // ==============================
    // 충돌·트리거
    // ==============================
    void OnCollisionEnter(Collision collision)
    {
        _isDead        = true;
        _currentReward = collisionPenalty;

        _qLearning.Update(_currentState, _currentAction, (int)(_currentReward * 100), _currentState);
        _agentManager.EndEpisode();
    }

    void OnTriggerExit(Collider other)
    {
        _currentReward += pipePassReward;
        _currentScore++;
    }

    // ==============================
    // 에피소드 관리
    // ==============================
    public void Reset()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        transform.position        = _initialPosition;

        _isDead                = false;
        _currentReward         = 0f;
        _currentScore          = 0;
        _timeSinceLastDecision = 0f;

        _currentState  = GetState();
        _currentAction = 0;
    }

    public void  SetEpsilon(float value) => _qLearning.Epsilon = value;
    public float GetEpsilon()            => _qLearning.Epsilon;
}
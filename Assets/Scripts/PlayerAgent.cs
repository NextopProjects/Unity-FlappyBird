using UnityEngine;

/// <summary>
/// PlayerAgent - Q-Learning 기반 Flappy Bird 에이전트
/// 
/// [상태 정의]
/// state = (dyBin, dxBin, velBin)
/// dyBin  : 갭 중심과의 Y 차이 (0.5 단위, -4~4 → 17개)
/// dxBin  : 파이프까지 X 거리 (1.5 단위, 0~6 → 7개)
/// velBin : Y 속도 (0.5 단위, -3~3 → 13개)
/// </summary>
public class PlayerAgent : MonoBehaviour
{
    [Header("물리 설정")]
    public float jumpForce = 5f;
    public float jumpCooldownTime = 0.2f;

    [Header("Q-Learning 파라미터")]
    public float learningRate = 0.3f;
    public float discountFactor = 0.9f;
    public float initialEpsilon = 1f;

    [Header("보상 설정")]
    public float surviveReward = 0.01f;
    public float pipePassReward = 1f;
    public float collisionPenalty = -1f;
    public float gapProximityReward = 0.3f;

    [Header("행동 제약")]
    public float maxYPosition = 8f;
    public float minYPosition = 2f;
    public float maxUpVelocity = 1f;

    private Rigidbody _rigidbody;
    private AgentManager _agentManager;
    private QLearning<(int, int, int)> _qLearning;

    private Vector3 _initialPosition;
    private (int, int, int) _currentState;

    private bool _isDead;
    private float _currentReward;
    private int _currentScore;
    private float _jumpCooldown;

    public int Score => _currentScore;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _agentManager = FindFirstObjectByType<AgentManager>();

        _qLearning = new QLearning<(int, int, int)>(2, learningRate, discountFactor, initialEpsilon);

        _initialPosition = transform.position;
        _currentState = GetState();
        _isDead = false;
    }

    void FixedUpdate()
    {
        if (_isDead) return;

        _jumpCooldown -= Time.fixedDeltaTime;

        var nextState = GetState();

        int action = _qLearning.GetAction(nextState);
        action = ApplyActionConstraints(action);

        if (action == 1 && _jumpCooldown <= 0f)
        {
            Jump();
            _jumpCooldown = jumpCooldownTime;
        }

        CalculateReward();
        _currentReward += surviveReward;

        _qLearning.Update(_currentState, action, (int)(_currentReward * 100), nextState);

        _currentState = nextState;
        _currentReward = 0f;
    }

    // ==============================
    // 상태 정의
    // ==============================
    (int, int, int) GetState()
    {
        GameObject closestPipe = FindClosestPipe();

        int dyBin = 8;
        int dxBin = 6;
        int velBin = 6;

        float velocityY = _rigidbody.linearVelocity.y;
        float velRounded = Mathf.Round(velocityY * 2f) / 2f;
        velBin = Mathf.Clamp(Mathf.RoundToInt(velRounded / 0.5f), -6, 6) + 6;

        if (closestPipe != null)
        {
            float gapCenterY = GetGapCenterY(closestPipe);

            float dy = transform.position.y - gapCenterY;
            float dyRounded = Mathf.Round(dy * 2f) / 2f;
            dyBin = Mathf.Clamp(Mathf.RoundToInt(dyRounded / 0.5f), -8, 8) + 8;

            float dx = closestPipe.transform.position.x - transform.position.x;
            dxBin = Mathf.Clamp(Mathf.FloorToInt(dx / 1.5f), 0, 6);
        }

        return (dyBin, dxBin, velBin);
    }

    GameObject FindClosestPipe()
    {
        PipeMovement[] pipes = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var pipe in pipes)
        {
            float dist = pipe.transform.position.x - transform.position.x;
            if (dist > 0 && dist < minDist)
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
            float topY = pipe.transform.GetChild(0).position.y;
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

    int ApplyActionConstraints(int action)
    {
        float y = transform.position.y;
        float velocityY = _rigidbody.linearVelocity.y;

        if (y > maxYPosition || velocityY > maxUpVelocity)
            return 0;

        if (y < minYPosition && velocityY < 0f)
            return 1;

        return action;
    }

    // ==============================
    // 보상
    // ==============================
    void CalculateReward()
    {
        GameObject closestPipe = FindClosestPipe();

        if (closestPipe != null)
        {
            float gapCenterY = GetGapCenterY(closestPipe);
            float distToGap = Mathf.Abs(transform.position.y - gapCenterY);

            float gapReward = Mathf.Max(0, 1f - distToGap / 5f);
            _currentReward += gapReward * gapProximityReward;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        _isDead = true;
        _currentReward = collisionPenalty;

        _qLearning.Update(_currentState, 0, (int)(_currentReward * 100), _currentState);
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
        transform.position = _initialPosition;

        _isDead = false;
        _currentReward = 0f;
        _currentScore = 0;
        _jumpCooldown = 0f;

        _currentState = GetState();
    }

    public void SetEpsilon(float value) => _qLearning.Epsilon = value;
    public float GetEpsilon() => _qLearning.Epsilon;

}
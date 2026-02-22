using System;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ML-Agent 기반 Flappy Bird 에이전트
/// 
/// [Raycast 센서]
/// - 아래쪽: 바닥까지 거리 감지
/// - 위쪽: 천장까지 거리 감지
/// - 전방: 파이프 감지
/// 
/// [관측 (Observations)] - 8개
/// - 새의 Y 위치 (정규화)
/// - 새의 Y 속도 (정규화)
/// - 바닥까지 거리 (Raycast)
/// - 천장까지 거리 (Raycast)
/// - 파이프까지 X 거리
/// - 파이프 갭 중심 Y
/// - 위쪽 파이프까지 거리 (Raycast)
/// - 아래쪽 파이프까지 거리 (Raycast)
/// 
/// [행동 (Actions)]
/// - Discrete(2): 0 = 대기, 1 = 점프
/// 
/// [보상 (Rewards)]
/// - 생존: +0.01 (매 스텝)
/// - 파이프 통과: +1.0
/// - 충돌: -1.0 (에피소드 종료)
/// - 갭 근접: +0.2 × (1 - 거리/5)
/// - 바닥 근접 페널티: -0.1 (거리 3 이하)
/// - 천장 근접 페널티: -0.1 (거리 3 이하)
/// </summary>
public class FlappyBirdAgent : Agent
{
    [Header("물리 설정")]
    public float jumpForce = 5f;
    public float jumpCooldownTime = 0.2f;
    
    [Header("Raycast 설정")]
    public float rayDistance = 15f;
    public LayerMask groundLayer;
    public LayerMask pipeLayer;
    
    [Header("보상 설정")]
    public float surviveReward = 0.01f;
    public float pipePassReward = 1f;
    public float collisionPenalty = -1f;
    public float gapProximityReward = 0.2f;
    public float groundProximityPenalty = -0.1f;
    public float ceilingProximityPenalty = -0.1f;
    public float groundDangerDistance = 3f;
    public float ceilingDangerDistance = 3f;
    
    [Header("환경 설정")]
    public float minY = -5f;
    public float maxY = 15f;
    
    [Header("UI")]
    public TextMeshProUGUI infoText;
    
    private Rigidbody _rigidbody;
    private MLAgentManager _manager;
    private Vector3 _initialPosition;
    private bool _isDead;
    private int _currentScore;
    private float _jumpCooldown;
    
    public int Score => _currentScore;
    public bool IsDead => _isDead;
    
    public override void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _manager = FindFirstObjectByType<MLAgentManager>();
        _initialPosition = transform.position;
    }
    
    public override void OnEpisodeBegin()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        transform.position = _initialPosition;
        _isDead = false;
        _currentScore = 0;
        _jumpCooldown = 0f;
        
        DestroyAllPipes();
    }
    
    private void DestroyAllPipes()
    {
        PipeMovement[] pipes = FindObjectsByType<PipeMovement>(FindObjectsSortMode.None);
        foreach (var pipe in pipes)
        {
            Destroy(pipe.gameObject);
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        float normalizedY = (transform.position.y - minY) / (maxY - minY) * 2f - 1f;
        sensor.AddObservation(normalizedY);
        
        float normalizedVelocityY = _rigidbody.linearVelocity.y / 10f;
        sensor.AddObservation(normalizedVelocityY);
        
        float groundDistance = GetRayDistance(Vector3.down, groundLayer);
        sensor.AddObservation(groundDistance / rayDistance);
        
        float ceilingDistance = GetRayDistance(Vector3.up, groundLayer);
        sensor.AddObservation(ceilingDistance / rayDistance);
        
        GameObject closestPipe = FindClosestPipe();
        
        if (closestPipe != null)
        {
            float distX = closestPipe.transform.position.x - transform.position.x;
            sensor.AddObservation(distX / 20f);
            
            float gapCenterY = GetGapCenterY(closestPipe);
            float normalizedGapY = (gapCenterY - minY) / (maxY - minY) * 2f - 1f;
            sensor.AddObservation(normalizedGapY);
            
            float topPipeDist = GetRayDistance(Vector3.right, pipeLayer, true);
            sensor.AddObservation(topPipeDist / rayDistance);
            
            float bottomPipeDist = GetRayDistance(Vector3.right, pipeLayer, false);
            sensor.AddObservation(bottomPipeDist / rayDistance);
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
            sensor.AddObservation(1f);
            sensor.AddObservation(1f);
        }
    }
    
    private float GetRayDistance(Vector3 direction, LayerMask layer, bool checkAbove = false)
    {
        Vector3 origin = transform.position;
        
        if (direction == Vector3.right && checkAbove)
        {
            origin.y += 2f;
        }
        else if (direction == Vector3.right && !checkAbove)
        {
            origin.y -= 2f;
        }
        
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, layer))
        {
            return hit.distance;
        }
        
        return rayDistance;
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_isDead) return;
        
        _jumpCooldown -= Time.fixedDeltaTime;
        
        int action = actions.DiscreteActions[0];
        
        if (action == 1 && _jumpCooldown <= 0f)
        {
            Jump();
            _jumpCooldown = jumpCooldownTime;
        }
        
        AddReward(surviveReward);
        
        CalculateRaycastRewards();
        
        CalculateProximityReward();
        
        if (transform.position.y < minY + 0.5f || transform.position.y > maxY - 0.5f)
        {
            AddReward(collisionPenalty * 0.5f);
        }
    }
    
    private void CalculateRaycastRewards()
    {
        float groundDistance = GetRayDistance(Vector3.down, groundLayer);
        
        if (groundDistance < groundDangerDistance)
        {
            float penaltyStrength = 1f - (groundDistance / groundDangerDistance);
            AddReward(groundProximityPenalty * penaltyStrength);
        }
        
        float ceilingDistance = GetRayDistance(Vector3.up, groundLayer);
        
        if (ceilingDistance < ceilingDangerDistance)
        {
            float penaltyStrength = 1f - (ceilingDistance / ceilingDangerDistance);
            AddReward(ceilingProximityPenalty * penaltyStrength);
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Keyboard.current.spaceKey.isPressed ? 1 : 0;
    }
    
    private void Jump()
    {
        _rigidbody.linearVelocity = Vector3.up * jumpForce;
    }
    
    private void CalculateProximityReward()
    {
        GameObject closestPipe = FindClosestPipe();
        
        if (closestPipe != null)
        {
            float gapCenterY = GetGapCenterY(closestPipe);
            float distToGap = Mathf.Abs(transform.position.y - gapCenterY);
            
            float gapReward = Mathf.Max(0, 1f - distToGap / 5f);
            AddReward(gapReward * gapProximityReward);
        }
    }
    
    private GameObject FindClosestPipe()
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
    
    private float GetGapCenterY(GameObject pipe)
    {
        if (pipe.transform.childCount >= 2)
        {
            float topY = pipe.transform.GetChild(0).position.y;
            float bottomY = pipe.transform.GetChild(1).position.y;
            return (topY + bottomY) / 2f;
        }
        return pipe.transform.position.y;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (_isDead) return;
        
        _isDead = true;
        AddReward(collisionPenalty);
        EndEpisode();
        
        if (_manager != null)
            _manager.OnEpisodeEnd(_currentScore);
    }
    
    private void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (_isDead) return;
        
        AddReward(pipePassReward);
        _currentScore++;
    }
    
    private void Update()
    {
        if (infoText != null)
        {
            infoText.text = $"Score: {_currentScore}\n" +
                            $"Episode: {CompletedEpisodes}\n" +
                            $"Step: {StepCount}";
        }
    }
    
    public void ResetAgent()
    {
        OnEpisodeBegin();
    }
}

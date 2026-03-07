using System;
using UnityEngine;

public class PlayerAgent : MonoBehaviour
{
    public float jumpForce = 5f;
    
    private Rigidbody  _rigidbody;
    private QLearning _qLearning;
    private Vector3 _initialPosition;
    private bool _isDead = false;

    private float _currentReward;
    private float _currentScore;
    
    
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _qLearning = new QLearning(0.3f, 0.0f, 1f);
        _initialPosition = _rigidbody.position;
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

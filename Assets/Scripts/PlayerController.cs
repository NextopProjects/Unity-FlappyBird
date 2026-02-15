using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private GameManager _gameManager;
    private AIAgent _aiAgent;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gameManager = GameObject.Find("GameManager")
            .GetComponent<GameManager>();
        _aiAgent = GetComponent<AIAgent>();
    }

    // 충돌 시 게임 종료
    private void OnCollisionEnter(Collision collision)
    {
        _aiAgent.OnCollision();
        _gameManager.GameOver();
    }

    // 파이프 통과 시 점수와 보상 처리
    private void OnTriggerEnter(Collider other)
    {
        _gameManager.AddScore();
        _aiAgent.OnPipePass();
    }

    // 살아있는 동안 작은 보상 제공
    void FixedUpdate()
    {
        _aiAgent.AddLivingReward();
    }
}
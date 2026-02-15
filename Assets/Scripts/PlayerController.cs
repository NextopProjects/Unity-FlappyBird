using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private GameManager _gameManager;
    private AIAgent _aiAgent;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _aiAgent = GetComponent<AIAgent>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("게임 오버!");
        
        // AI 에이전트에게 충돌 알림
        _aiAgent.OnCollision();
        
        // 게임 매니저에게 게임오버 알림
        _gameManager.GameOver();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 점수 추가
        _gameManager.AddScore();
        
        // AI 에이전트에게 파이프 통과 보상 전달
        _aiAgent.OnPipePass();
    }
}
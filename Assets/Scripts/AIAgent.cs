using UnityEngine;
using System.Collections.Generic;

public class AIAgent : MonoBehaviour
{
    /*
        static 데이터는 씬이 다시 로드되어도 유지된다.
        학습 정보는 계속 누적되어야 하므로 static으로 선언한다.
    */

    // 상태별 행동 점수 저장 테이블
    private static Dictionary<string, float[]> qTable =
        new Dictionary<string, float[]>();

    // 총 학습 횟수
    private static int totalEpisodes = 0;

    // 탐험 확률 (점점 감소)
    private static float epsilon = 1.0f;

    // 학습 설정값
    public float learningRate = 0.1f;
    public float discountFactor = 0.9f;
    public float epsilonDecay = 0.995f;

    private Rigidbody rb;

    // 이전 상태와 행동은 보상 계산에 필요하다
    private string previousState;
    private int previousAction;

    private bool episodeEnded = false;

    // GameManager가 읽기만 할 수 있도록 제공
    public int episodeCount => totalEpisodes;
    public float epsilonValue => epsilon;
    public int qTableSize => qTable.Count;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 게임이 끝났으면 학습 중단
        if (episodeEnded) return;

        // 현재 상태 계산
        string currentState = GetState();

        // 처음 보는 상태라면 초기화
        if (!qTable.ContainsKey(currentState))
            qTable[currentState] = new float[2];

        // 행동 선택
        int action = ChooseAction(currentState);

        // 행동 실행 (1이면 점프)
        if (action == 1)
            rb.linearVelocity = Vector3.up * 5f;

        // 다음 보상 계산을 위해 저장
        previousState = currentState;
        previousAction = action;
    }

    // 살아있는 동안 작은 보상
    public void AddLivingReward(float reward = 0.1f)
    {
        if (previousState == null) return;

        UpdateQ(previousState, previousAction, reward, GetState());
    }

    // 파이프 통과 보상
    public void OnPipePass()
    {
        if (previousState == null) return;

        UpdateQ(previousState, previousAction, 10f, GetState());
    }

    // 충돌 패널티
    public void OnCollision()
    {
        if (previousState != null)
            UpdateQ(previousState, previousAction, -10f, previousState);

        episodeEnded = true;
        totalEpisodes++;
        epsilon *= epsilonDecay;
    }

    // 현재 상태를 문자열로 표현
    string GetState()
    {
        GameObject pipe = FindClosestPipe();

        int playerY = Mathf.RoundToInt(transform.position.y);
        int pipeDist = 0;
        int pipeHeight = 0;

        if (pipe != null)
        {
            pipeDist = Mathf.RoundToInt(
                pipe.transform.position.x - transform.position.x);

            pipeHeight = Mathf.RoundToInt(
                pipe.transform.position.y - transform.position.y);
        }

        return playerY + "_" + pipeDist + "_" + pipeHeight;
    }

    // 가장 가까운 파이프 탐색
    GameObject FindClosestPipe()
    {
        PipeMovement[] pipes = FindObjectsOfType<PipeMovement>();

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

    // epsilon-greedy 방식으로 행동 선택
    int ChooseAction(string state)
    {
        if (Random.value < epsilon)
            return Random.Range(0, 2);

        float[] q = qTable[state];
        return q[0] > q[1] ? 0 : 1;
    }

    // Q값 업데이트
    void UpdateQ(string state, int action, float reward, string nextState)
    {
        if (!qTable.ContainsKey(nextState))
            qTable[nextState] = new float[2];

        float currentQ = qTable[state][action];
        float maxNextQ = Mathf.Max(qTable[nextState][0], qTable[nextState][1]);

        qTable[state][action] =
            currentQ + learningRate *
            (reward + discountFactor * maxNextQ - currentQ);
    }
}
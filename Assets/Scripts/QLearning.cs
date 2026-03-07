using System.Collections.Generic;
using UnityEngine;

// 경험을 통해 최적을 행동을 학습할떄 사용하는 알고리즘
public class QLearning<TState>
{
    private Dictionary<TState, float[]> _qTable;
    
    private float _learningRate; // 학습률 (a)
    private float _discountFactor; // 할인율 (y)
    public float Epsilon; // 탐험률 (e)

    private int _actionCount; //행동 개수 (stay, jump)

    public QLearning(int actionCount, float learningRate, float discountFactor, float epsilon)
    {
        _qTable = new Dictionary<TState, float[]>();
        
        _learningRate = learningRate;
        _discountFactor = discountFactor;
        Epsilon = epsilon;
        _actionCount = actionCount;
    }
    
    private float[] GetOrCreate(TState state)
    {
        if (!_qTable.ContainsKey(state))
            _qTable[state] = new float[_actionCount];

        return _qTable[state];
    }
    
    public int GetAction(TState state)
    {
        float[] qValues = GetOrCreate(state);

        if (Random.value < Epsilon)
            return Random.Range(0, _actionCount);

        // 최대 Q값 찾기 (행동 개수 자유)
        int bestAction = 0;
        float max = qValues[0];

        for (int i = 1; i < _actionCount; i++)
        {
            if (qValues[i] > max)
            {
                max = qValues[i];
                bestAction = i;
            }
        }

        return bestAction;
    }

    public void Update(TState state, int action,int reward, TState nextState)
    {
        // 새로운 상태고 최초라면 Q-Value 생성하고 초기화
        if (!_qTable.ContainsKey(state))
            _qTable[state] = new float[_actionCount];
        if (!_qTable.ContainsKey(nextState))
            _qTable[nextState] = new float[_actionCount];
        
        // 다음 상태에서 점프를 했을떄 대기했을 때 값 중 가장 큰 값을 뽑아옴
        float maxNextQ = Mathf.Max(_qTable[nextState]);
        
        
        /// [Q-Learning 공식]
        /// Q(s,a) ← Q(s,a) + α × [R + γ × max(Q(s',a')) - Q(s,a)]
        _qTable[state][action] =
            _qTable[state][action] + _learningRate * (reward + _discountFactor * maxNextQ - _qTable[state][action]);
    }
    
    
}

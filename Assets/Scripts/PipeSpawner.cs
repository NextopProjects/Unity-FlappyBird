using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject PipePrefab;
    public float SpawnRateSec = 2f;
    public float RandomYMaxOffset = 10f;
    public float RandomYMinOffset = 0f;
    private float _timer = 0f;
    
    void Start()
    {
        
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > SpawnRateSec)
        {
            Instantiate(PipePrefab, new Vector3(10,Random.Range(RandomYMinOffset,RandomYMaxOffset),0),Quaternion.identity);
            _timer = 0f;
        }
    }
    
    public void ResetTimer()
    {
        _timer = 0f;
    }
}

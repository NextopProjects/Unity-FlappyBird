using UnityEngine;

public class PipeMovement : MonoBehaviour
{
    public float speed = 2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
        // Vector3.left == new Vector3(-1,0,0)   -1 * 2f  -2f
        if(transform.position.x < -15f) Destroy(this.gameObject);
    }
}

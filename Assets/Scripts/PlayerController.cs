using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    public float jumpForce = 5f;
    private GameManager _gameManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnJump()
    {
        // 위로 이동에 대한 1action
        // Debug.Log("Jump");
        _rigidbody.linearVelocity = Vector3.up * jumpForce;
        // _rigidbody.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("CollisionEnter");
        _gameManager.GameOver();
    }
    //
    // void OnCollisionStay(Collision collision)
    // {
    //     Debug.Log("OnCollisionStay");
    // }
    //
    // private void OnCollisionExit(Collision other)
    // {
    //     Debug.Log("OnCollisionExit");
    // }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("TODO: 점수 증가");
    }
}

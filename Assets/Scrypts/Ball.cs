using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 originPosition;
    private void Awake()
    {
        originPosition = transform.position;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Goal"))
        {
            // 점수 증가
            GameManager.Instance.score += 1;
            Debug.Log("Score: " + GameManager.Instance.score);
            RespawnBall();
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            RespawnBall();
        }
    }
    
    private void RespawnBall()
    {
        transform.position = originPosition;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}

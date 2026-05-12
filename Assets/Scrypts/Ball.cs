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
            DestroySquence();
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            DestroySquence();
        }
    }
    
    private void DestroySquence()
    {
        Destroy(gameObject);
    }
}

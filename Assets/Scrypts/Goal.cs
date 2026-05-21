using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            GameManager.Instance.score += 1;
            Debug.Log("Score: " + GameManager.Instance.score);
        }
    }
}   

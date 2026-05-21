using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Vector3 originPosition;
    public GameObject explosionEffect;
    public GameObject failEffect;
    
    public static event Action<bool> OnBallDestroyed;
    
    private void Awake()
    {
        originPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. 골대(Goal)에 부딪혔을 때
        if (other.CompareTag("Goal"))
        {
            // 부모 오브젝트(Basket)의 이름을 가져옵니다.
            string basketName = other.transform.parent.name;
            // 현재 공의 태그를 가져옵니다 (예: "RedBall")
            string ballTag = gameObject.tag;

            // 색상 일치 여부 확인 (공의 태그와 바구니 이름에 같은 색상 단어가 있는지)
            if (IsColorMatched(ballTag, basketName))
            {
                GameManager.Instance.score += 1;
                
                OnBallDestroyed?.Invoke(true); // "성공했다!" 알림
                
                GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            
                // 2. [꿀팁] 이펙트 색상을 공의 색상과 맞추기 (선택사항)
                var main = effect.GetComponent<ParticleSystem>().main;
                main.startColor = GetComponent<Renderer>().material.color;

                // 3. 이펙트 자동 삭제 (1초 뒤)
                Destroy(effect, 1f);
                DestroySequence();
            }
            else
            {
                GameObject effect = Instantiate(failEffect, transform.position, Quaternion.identity);
                Destroy(effect, 1f);
                OnBallDestroyed?.Invoke(false); // "실패했다..." 알림
            }

            DestroySequence();
        }
        // 2. 장외(OutOfBounds)로 나갔을 때
        else if (other.CompareTag("Ground"))
        {
            GameObject effect = Instantiate(failEffect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
            OnBallDestroyed?.Invoke(false); // 바닥에 떨어지면 실패
            DestroySequence();
        }
    }

// 색상을 비교해주는 간단한 헬퍼 함수
    private bool IsColorMatched(string ballTag, string basketName)
    {
        string[] colors = { "Red", "Blue", "Yellow", "Green" };

        foreach (string color in colors)
        {
            // 공 태그와 바구니 이름 둘 다에 같은 색상 단어가 들어있으면 true
            if (ballTag.Contains(color) && basketName.Contains(color))
            {
                return true;
            }
        }

        return false;
    }

    private void DestroySequence()
    {
        Destroy(gameObject);
    }
}

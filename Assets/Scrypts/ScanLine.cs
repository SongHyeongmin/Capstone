using UnityEngine;

public class ScanLine : MonoBehaviour
{
    [Header("설정")]
    public float speed = 2.0f;      // 선이 움직이는 속도
    public float minY = -5.0f;     // 화면 하단 끝 좌표
    public float maxY = 5.0f;      // 화면 상단 끝 좌표

    // 현재 선의 위치를 외부(판정 로직)에서 참조할 수 있게 프로퍼티로 선언
    public float CurrentY { get; private set; }

    void Update()
    {
        // 핵심 로직: 0부터 (maxY - minY) 사이를 왕복함
        float progress = Mathf.PingPong(Time.time * speed, maxY - minY);
        
        // 실제 좌표로 변환
        CurrentY = minY + progress;

        // 오브젝트 위치 업데이트
        transform.position = new Vector3(transform.position.x, CurrentY, transform.position.z);
    }
    
    
    
    void CheckNoteHit(Note note)
    {
        // 1. 스캔라인이 노츠 위치 근처인지 확인
        float distance = Mathf.Abs(gameObject.transform.position.y - note.transform.position.y);
    
        if (distance < 0.5f && !note.isProcessed) // 0.5f는 판정 범위(조절 가능)
        {
            // 2. 미디어파이프가 인식한 손가락 개수와 노츠의 숫자가 같은지 확인
            /*if (note.requiredFingerCount)
            {
                Debug.Log($"{note.requiredFingerCount}개 일치! PERFECT!");
                note.isProcessed = true; 
                // 여기서 이펙트 빵 터뜨리고 점수 추가하면 됨
            }*/
        }
    }
}

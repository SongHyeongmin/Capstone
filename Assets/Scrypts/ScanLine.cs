using System;
using UnityEngine;

public class ScanLine : MonoBehaviour
{
    [Header("설정")]
    public float speed = 2.0f;      // 선이 움직이는 속도
    public float minY = -5.0f;     // 화면 하단 끝 좌표
    public float maxY = 5.0f;      // 화면 상단 끝 좌표
    public HandDataManager handDataManager;
    public int currentInputFingerCount = 0;
    public bool isHandTouching = false; // 손이 선과 닿았는지 여부
    // 현재 선의 위치를 외부(판정 로직)에서 참조할 수 있게 프로퍼티로 선언
    public float CurrentY { get; private set; }
    // [추가] true면 위로 이동 중, false면 아래로 이동 중
    public bool IsMovingUp { get; private set; }
    
    private float lastY;

    void Update()
    {
        // 핵심 로직: 0부터 (maxY - minY) 사이를 왕복함
        float progress = Mathf.PingPong(Time.time * speed, maxY - minY);

        // 실제 좌표로 변환
        CurrentY = minY + progress;

        // 현재 Y와 이전 프레임의 Y를 비교해서 방향 판정
        IsMovingUp = CurrentY > lastY;
        lastY = CurrentY;
        // 오브젝트 위치 업데이트
        transform.position = new Vector3(transform.position.x, CurrentY, transform.position.z);
        switch (handDataManager.CurrentHandState)
        {
            case HandState.Open:
                currentInputFingerCount = 5;
                break;
            case HandState.ThumbFolded:
                currentInputFingerCount = 4;
                break;
            case HandState.IndexFolded:
                currentInputFingerCount = 3;
                break;
            case HandState.AllFolded:
                currentInputFingerCount = 0;
                break;
            default:
                currentInputFingerCount = -1; // 알 수 없는 상태
                break;
        }
    }


    void OnTriggerStay(Collider other)
    {
        // 부딪힌 오브젝트가 "Note" 태그를 가졌는지 확인
        if (other.CompareTag("Note"))
        {
            Note note = other.GetComponent<Note>();
            
            if (note != null && !note.isProcessed)
            {
                Debug.Log("note와 충돌! 판정 타이밍!");
                CheckScore(note);
            }
        }
    }

    void CheckScore(Note note)
    {
            
        if ((currentInputFingerCount == note.requiredFingerCount) && note.isHandTouching)
        {   
            Debug.Log($"<color=green>[PERFECT]</color> {note.requiredFingerCount}개 일치!");
            note.isProcessed = true;
            note.OnHit();
            Destroy(note.transform.parent.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Note") && other.GetComponent<Note>().isProcessed == false)
        {
            Destroy(other.transform.parent.gameObject);
        }
    }
}

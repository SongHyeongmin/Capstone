using UnityEngine;
using UnityEngine.XR.Hands;

public class XRHandSynchronizer : MonoBehaviour
{
    [Header("데이터 소스")]
    public HandDataManager dataManager;

    [Header("손가락 뼈대 배열 (손목부터 끝 순서대로)")]
    [Tooltip("순서대로 0번부터 20번까지 조인트를 할당하세요.")]
    public Transform[] handBones; 

    [Header("설정")]
    public float smoothSpeed = 20f;
    public Vector3 rotationOffset; // 모델마다 뼈 방향이 다를 때 조정용

    void Update()
    {
        if (dataManager != null && dataManager.IsHandDetected)
        {
            SyncBones();
        }
    }

    void SyncBones()
    {
        // 1. 전체 위치 동기화 (손목 - 0번)
        if (handBones.Length > 0 && handBones[0] != null)
        {
            handBones[0].position = Vector3.Lerp(handBones[0].position, new Vector3(dataManager.HandJoints[0].x,dataManager.HandJoints[0].y, -1.3f), 
                Time.deltaTime * smoothSpeed);
        }
        /*
        // 2. 각 마디 회전 동기화
        // 손가락 마디는 '현재 마디'에서 '다음 마디'를 바라보게 설정함
        for (int i = 0; i < handBones.Length; i++)
        {
            if (handBones[i] == null) continue;

            // 끝마디(Tip)가 아닌 경우에만 다음 마디를 향해 회전
            int nextJointIndex = GetNextJointIndex(i);
            if (nextJointIndex != -1)
            {
                Vector3 direction = dataManager.HandJoints[nextJointIndex] - dataManager.HandJoints[i];
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    // 모델 뼈의 기본 축 방향에 맞게 offset 적용
                    handBones[i].rotation = Quaternion.Slerp(handBones[i].rotation, targetRot * Quaternion.Euler(rotationOffset), Time.deltaTime * smoothSpeed);
                }
            }
        }*/
    }

    // MediaPipe 조인트 연결 구조에 따라 다음 마디 인덱스를 반환
    int GetNextJointIndex(int current)
    {
        // 손가락 끝번호들(4, 8, 12, 16, 20)은 다음 마디가 없음
        if (current % 4 == 0 && current != 0) return -1;
        
        // 보통 0(손목) -> 1, 5, 9, 13, 17로 연결되지만 
        // 간단하게는 현재 인덱스 + 1을 바라보게 함
        if (current == 0) return 1; // 손목은 엄지 방향을 일단 바라봄 (수정 필요할 수 있음)
        
        return current + 1;
    }
}
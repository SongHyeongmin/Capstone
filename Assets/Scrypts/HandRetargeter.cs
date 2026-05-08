using UnityEngine;

public class HandRetargeter : MonoBehaviour
{
    [Header("Data Source")]
    public HandDataManager dataManager;

    [Header("XR Hands Bones")]
    public Transform[] handBones; 

    [Header("World Movement Settings")]
    // 이 값을 300, 500 식으로 확 키워야 손이 화면 끝까지 따라옵니다.
    public float movementScale = 150f; 
    // 카메라로부터 얼마나 떨어뜨릴지 결정합니다.
    public float zOffset = 10f;

    [Header("Smoothing")]
    [Range(0, 1)]
    public float smoothSpeed = 0.2f;

    void LateUpdate() // Update보다 우선순위가 높은 LateUpdate를 사용합니다.
    {
        if (dataManager != null && dataManager.IsHandDetected)
        {
            // 모든 뼈대를 강제로 월드 좌표로 이동시킵니다.
            for (int i = 0; i < handBones.Length; i++)
            {
                if (handBones[i] == null) continue;

                // 1. 미디어파이프의 0~1 사이 좌표 가져오기
                Vector3 rawPos = dataManager.WorldHandJoints[i];

                // 2. 월드 좌표 계산 (-x는 거울 모드 반전)
                Vector3 targetWorldPos = new Vector3(
                    -rawPos.x * movementScale, 
                    rawPos.y * movementScale, 
                    rawPos.z * movementScale + zOffset
                );

                // 3. 부드럽게 이동 (지터링 방지)
                handBones[i].position = Vector3.Lerp(handBones[i].position, targetWorldPos, smoothSpeed);
            }
        }
    }
}
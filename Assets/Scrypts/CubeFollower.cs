using System;
using UnityEngine;

public class CubeFollower : MonoBehaviour
{
    [Header("연결 설정")]
    public HandDataManager dataManager; // 
    public int jointIndex = 8;          // 검지 끝 (8번)

    [Header("움직임 설정")]
    [Range(1f, 30f)]
    public float sensitivity = 50f;     // 거리 보정 후의 이동 배율
    public float smoothSpeed = 20f;      // 추적 속도
    public float filterSmoothing = 0.1f; // 부들거림 방지
    
    private Vector3 _startCubePos;
    private Vector3 _filteredPos;
    private bool _isFirstDetection = true;
    void Update()
    {
        if (dataManager != null && dataManager.IsHandDetected)
        {
            // 기준 길이 측정 사용자의 손목(0)에서 중지 시작점(9)까지의 실제 거리
            // 거리가 멀어지면 이 값도 작아지므로, 이를 분모로 쓰면 거리에 따른 크기 변화가 상쇄됩니다.
            float refLen = Vector3.Distance(dataManager.HandJoints[0], dataManager.HandJoints[9]);
            if (refLen < 0.01f) refLen = 0.01f; // 0 나누기 방지

            // 현재 손의 위치(m)를 기준 길이로 나누어 정규화
            Vector3 rawPos = dataManager.HandJoints[jointIndex];
            rawPos.z = 0;
            Vector3 normalizedPos = rawPos / refLen;
            Debug.Log(normalizedPos);

            // 로우 패스 필터
            _filteredPos = Vector3.Lerp(_filteredPos, normalizedPos, filterSmoothing);

            // 최종 위치 계산 = 기준점 + (보정된 이동량 * 감도)
            Vector3 targetPosition = _startCubePos + (_filteredPos * sensitivity);

            // 부드러운 이동 적용
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
        }
        else
        {
            _isFirstDetection = true; // 손을 놓치면 다시 기준을 잡도록 초기화
        }
        // 주먹 판정 로직 호출
        if (IsFist())
        {
            Debug.Log("주먹 인식됨");
        }
    }

    bool IsFist()
    {
        if (dataManager == null || !dataManager.IsHandDetected) return false;

        // 각 손가락 마디 사이의 벡터 각도 계산
        float indexAngle = Vector3.Angle(dataManager.HandJoints[6] - dataManager.HandJoints[5], dataManager.HandJoints[8] - dataManager.HandJoints[7]);
        float middleAngle = Vector3.Angle(dataManager.HandJoints[10] - dataManager.HandJoints[9], dataManager.HandJoints[12] - dataManager.HandJoints[11]);
        float ringAngle = Vector3.Angle(dataManager.HandJoints[14] - dataManager.HandJoints[13], dataManager.HandJoints[16] - dataManager.HandJoints[15]);

        // 세 손가락 이상이 120도 이상 꺾였는지 확인
        int foldedCount = 0;
        if (indexAngle > 120f) foldedCount++;
        if (middleAngle > 120f) foldedCount++;
        if (ringAngle > 120f) foldedCount++;

        return foldedCount >= 3;
    }
}
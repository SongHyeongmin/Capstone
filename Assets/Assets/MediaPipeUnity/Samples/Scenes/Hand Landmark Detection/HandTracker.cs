using UnityEngine;
using Mediapipe;
using Mediapipe.Unity;
using System.Collections.Generic;
public class HandTracker : MonoBehaviour
{ 
    // 가장 최근에 수신된 손 데이터를 저장할 변수
    private List<NormalizedLandmarkList> _currentHandLandmarks;

    // 미디어파이프에서 데이터가 들어오면 실행되는 콜백 함수
    public void OnHandLandmarksOutput(List<NormalizedLandmarkList> landmarks)
    {
        // 최신 데이터를 변수에 업데이트
        _currentHandLandmarks = landmarks;
    }

    void Update()
    {
        // 1. 데이터가 있는지 확인
        if (_currentHandLandmarks == null || _currentHandLandmarks.Count == 0)
        {
            Debug.Log("손 데이터가 없습니다.");
            return; 
        }

        // 2. 첫 번째 손의 데이터 가져오기
        var hand = _currentHandLandmarks[0];

        // 3. 8번 관절(검지 끝) 좌표 가져오기
        var tip = hand.Landmark[8];

        // 4. 월드 좌표로 변환 (아까 만든 메서드 활용)
        Vector3 worldPos = NormalizeToWorldPoint(tip.X, tip.Y, tip.Z);

        // 5. 로그 출력
        Debug.Log($"실시간 검지 좌표: {worldPos}");

        // [응용] 재활용 오브젝트를 손가락 끝 위치로 이동시키기
        // myObject.transform.position = worldPos;
    }

    private Vector3 NormalizeToWorldPoint(float x, float y, float z)
    {
        // 화면 좌표 변환 로직 (Y 반전 주의)
        Vector3 screenPoint = new Vector3(x * UnityEngine.Screen.width, (1f - y) * UnityEngine.Screen.height, z);
        screenPoint.z = 10f; // 카메라와의 거리
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }
}

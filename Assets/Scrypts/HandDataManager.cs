using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public class HandDataManager : MonoBehaviour
{
    public HandLandmarkerRunner runner;
    
    // 백그라운드에서 받은 생 데이터를 임시 저장할 변수
    private HandLandmarkerResult _latestResult;
    private bool _isDataNew = false;

    // 다른 스크립트에서 가져갈 최종 월드 좌표 배열 
    public Vector3[] HandJoints { get; private set; } = new Vector3[21];
    // 다른 스크립트에서 가져갈 최종 월드 좌표 배열 (실측 단위, 손목이 원점)
    public Vector3[] WorldHandJoints { get; private set; } = new Vector3[21];
    public bool IsHandDetected { get; private set; }

    void Start()
    {
        if (runner != null)
            runner.OnHandResultDetected += OnResultsReceived;
    }
    
    void OnResultsReceived(HandLandmarkerResult result)
    {
        _latestResult = result;
        _isDataNew = true;
    }

    void Update()
    {
        if (!_isDataNew) return;

        ProcessData(_latestResult);
        _isDataNew = false;
    }

    void ProcessData(HandLandmarkerResult result)
    {
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            IsHandDetected = false;
            return;
        }

        IsHandDetected = true;
        var landmarks = result.handLandmarks[0].landmarks;

        for (int i = 0; i < landmarks.Count; i++)
        {
            HandJoints[i] = ConvertToWorld(landmarks[i].x, landmarks[i].y, landmarks[i].z);
        }
        
        if (result.handWorldLandmarks != null && result.handWorldLandmarks.Count > 0)
        {
            var worldLandmarks = result.handWorldLandmarks[0].landmarks;
            for (int i = 0; i < worldLandmarks.Count; i++)
            {
                // MediaPipe -> Unity 변환
                WorldHandJoints[i] = new Vector3(-worldLandmarks[i].x, -worldLandmarks[i].y, -worldLandmarks[i].z);
            }
        }
        // Debug.Log($"<color=cyan>[Thumb]</color> World Pos: {HandJoints[4]}");
    }

    private Vector3 ConvertToWorld(float x, float y, float z)
    {
        Vector3 screenPoint = new Vector3(x * Screen.width, (1f - y) * Screen.height, 10f);
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }
}
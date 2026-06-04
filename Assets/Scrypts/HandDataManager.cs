using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public enum HandState { Open, ThumbFolded, IndexFolded, AllFolded, Unknown }

public class HandDataManager : MonoBehaviour
{
    public HandLandmarkerRunner runner;
    public ArduinoReceiver arduinoReceiver;
    
    // 백그라운드에서 받은 생 데이터를 임시 저장할 변수
    private HandLandmarkerResult _latestResult;
    private bool _isDataNew = false;
    
    public HandState CurrentHandState { get; private set; } = HandState.Open;
    private const int FOLDED_VALUE = 0;
    

    // 다른 스크립트에서 가져갈 최종 월드 좌표 배열 
    public Vector3[] HandJoints { get; private set; } = new Vector3[21];
    // 다른 스크립트에서 가져갈 최종 월드 좌표 배열 (실측 단위, 손목이 원점)
    public Vector3[] WorldHandJoints { get; private set; } = new Vector3[21];
    public bool IsHandDetected { get; private set; }

    public GameObject Thumb;
    public GameObject Index;
    public GameObject Middle;
    public GameObject Ring;
    public GameObject Little;
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
        
        UpdateHandState();
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
    
    void UpdateHandState()
    {
        if (arduinoReceiver == null || !arduinoReceiver.isConnected) return;

        // 아두이노 리시버에서 3개의 홀센서 값을 가져옴
        bool thumbFolded = (arduinoReceiver.HALL1 == FOLDED_VALUE);
        bool indexFolded = (arduinoReceiver.HALL2 == FOLDED_VALUE);
        bool ringFolded  = (arduinoReceiver.HALL3 == FOLDED_VALUE);

        // 상태 판별 로직
        if (thumbFolded && indexFolded && ringFolded) CurrentHandState = HandState.AllFolded;
        else if (thumbFolded && !indexFolded) CurrentHandState = HandState.ThumbFolded;
        else if (!thumbFolded && indexFolded) CurrentHandState = HandState.IndexFolded;
        else CurrentHandState = HandState.Open;
        
        // Debug.Log($"Current Hand State: {CurrentHandState}");
        ApplyFingerRotation(thumbFolded, indexFolded, ringFolded);
    }
    public void FoldFinger(GameObject finger)
    {
        Vector3 currentRotation = finger.transform.localEulerAngles;
        currentRotation.x = -50f;
        Quaternion targetRotation = Quaternion.Euler(currentRotation);
        finger.transform.localRotation = targetRotation;
    }
    public void  UnfoldFinger(GameObject finger)
    {
        Vector3 currentRotation = finger.transform.localEulerAngles;
        currentRotation.x = -50f;
        Quaternion targetRotation = Quaternion.Euler(currentRotation);
        finger.transform.localRotation = targetRotation;
    }
    
    private void ApplyFingerRotation(bool thumb, bool index, bool ring)
    {
        // 1. 엄지 처리
        if (thumb) FoldFinger(Thumb); else UnfoldFinger(Thumb);
        
        // 2. 검지 처리
        if (index) FoldFinger(Index); else UnfoldFinger(Index);
        
        // 3. 약지 처리
        if (ring) FoldFinger(Ring); else UnfoldFinger(Ring);
        
        if (ring) 
        {
            FoldFinger(Middle);
            FoldFinger(Little);
        }
        else 
        {
            UnfoldFinger(Middle);
            UnfoldFinger(Little);
        }
    }
}
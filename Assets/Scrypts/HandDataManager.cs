using UnityEngine;
using System.Collections; // 💡 코루틴용 필수!
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

public enum HandState { Open, ThumbFolded, IndexFolded, AllFolded, Unknown }

public class HandDataManager : MonoBehaviour
{
    public HandLandmarkerRunner runner;
    public ArduinoReceiver arduinoReceiver;
    
    private HandLandmarkerResult _latestResult;
    private bool _isDataNew = false;
    
    public HandState CurrentHandState { get; private set; } = HandState.Open;
    private const int FOLDED_VALUE = 0;

    public Vector3[] HandJoints { get; private set; } = new Vector3[21];
    public Vector3[] WorldHandJoints { get; private set; } = new Vector3[21];
    public bool IsHandDetected { get; private set; }

    public GameObject Thumb;
    public GameObject Index;
    public GameObject Middle;
    public GameObject Ring;
    public GameObject Little;

    [Header("💡 캘리브레이션 설정")]
    public bool isCalibrated = false;       // 영점이 잡혔는가?
    private Vector3 calibrationOffset = Vector3.zero; // 저장된 손목 기준점 (0번 관절)
    public string calibrationText = "";     // UI에 띄울 텍스트 (필요하면 TextMeshPro랑 연동해)

    void Start()
    {
        if (runner != null)
            runner.OnHandResultDetected += OnResultsReceived;
        StartCoroutine(CalibrationRoutine());
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

    // 💡 3초 동안 대기하며 손 위치를 고정시키는 코루틴
    private IEnumerator CalibrationRoutine()
    {
        isCalibrated = false;
        float timer = 3f;

        while (timer > 0f)
        {
            calibrationText = $"손의 위치를 잡으세요! ({timer:F1}초)";
            Debug.Log(calibrationText);
            
            timer -= Time.deltaTime;
            yield return null;
        }

        // 3초가 지난 시점에 손이 화면에 보이고 있다면, 그 순간의 0번 관절(손목)을 원점으로 지정!
        if (IsHandDetected)
        {
            // ConvertToWorld를 거친 날것의 손목 좌표를 오프셋으로 저장
            calibrationOffset = HandJoints[0]; 
            isCalibrated = true;
            calibrationText = "영점 조절 완료!";
            Debug.Log("<color=yellow>👍 캘리브레이션 완료! 원점 저장됨.</color>");
        }
        /*else
        {
            // 만약 3초 지났는데 손 안 대고 있었으면? 꼼수 부리지 말라고 재시작!
            calibrationText = "손이 감지되지 않아 재시도합니다...";
            Debug.LogWarning(calibrationText);
            yield return new WaitForSeconds(1f);
            StartCoroutine(CalibrationRoutine()); // 재귀 호출로 다시 시작
        }*/
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

        // 먼저 원본 좌표를 쭉 계산해 둔 뒤
        for (int i = 0; i < landmarks.Count; i++)
        {
            Vector3 rawWorldPos = ConvertToWorld(landmarks[i].x, landmarks[i].y, landmarks[i].z);
            
            // 💡 [핵심] 영점 조절이 완료되었다면, 모든 관절 좌표에서 '저장된 손목 원점'을 빼버림!
            // 이렇게 하면 내가 처음에 손목을 둔 위치가 유니티 월드상의 (0, 0, 0)이 됨.
            if (isCalibrated)
            {
                HandJoints[i] = rawWorldPos - calibrationOffset;
            }
            else
            {
                HandJoints[i] = rawWorldPos; // 캘리브레이션 중일 때는 그냥 생 데이터 보여줌
            }
        }
        
        if (result.handWorldLandmarks != null && result.handWorldLandmarks.Count > 0)
        {
            var worldLandmarks = result.handWorldLandmarks[0].landmarks;
            for (int i = 0; i < worldLandmarks.Count; i++)
            {
                WorldHandJoints[i] = new Vector3(-worldLandmarks[i].x, -worldLandmarks[i].y, -worldLandmarks[i].z);
            }
        }
    }

    private Vector3 ConvertToWorld(float x, float y, float z)
    {
        Vector3 screenPoint = new Vector3(x * Screen.width, (1f - y) * Screen.height, 10f);
        return Camera.main.ScreenToWorldPoint(screenPoint);
    }
    
    // ... 이하 아두이노 및 손가락 접기 로직(동일함) ...
    void UpdateHandState()
    {
        if (arduinoReceiver == null || !arduinoReceiver.isConnected) return;

        bool thumbFolded = (arduinoReceiver.HALL1 == FOLDED_VALUE);
        bool indexFolded = (arduinoReceiver.HALL2 == FOLDED_VALUE);
        bool ringFolded  = (arduinoReceiver.HALL3 == FOLDED_VALUE);

        if (thumbFolded && indexFolded && ringFolded) CurrentHandState = HandState.AllFolded;
        else if (thumbFolded && !indexFolded) CurrentHandState = HandState.ThumbFolded;
        else if (!thumbFolded && indexFolded) CurrentHandState = HandState.IndexFolded;
        else CurrentHandState = HandState.Open;
        
        ApplyFingerRotation(thumbFolded, indexFolded, ringFolded);
    }

    public void FoldFinger(GameObject finger)
    {
        if (finger == null) return;
        Vector3 currentRotation = finger.transform.localEulerAngles;
        currentRotation.x = -50f;
        finger.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void UnfoldFinger(GameObject finger)
    {
        if (finger == null) return;
        Vector3 currentRotation = finger.transform.localEulerAngles;
        currentRotation.x = 0f; // 앗! 저번에 수정한 0도 반영 완료
        finger.transform.localRotation = Quaternion.Euler(currentRotation);
    }
    
    private void ApplyFingerRotation(bool thumb, bool index, bool ring)
    {
        if (thumb) FoldFinger(Thumb); else UnfoldFinger(Thumb);
        if (index) FoldFinger(Index); else UnfoldFinger(Index);
        if (ring) FoldFinger(Ring); else UnfoldFinger(Ring);
        
        if (ring) { FoldFinger(Middle); FoldFinger(Little); }
        else { UnfoldFinger(Middle); UnfoldFinger(Little); }
    }
}
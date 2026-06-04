using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CubeFollower : MonoBehaviour
{
    [Header("연결 설정")]
    public HandDataManager dataManager;
    public int jointIndex = 8; // 검지 끝 (8번)

    [Header("움직임 설정")]
    [Range(1f, 100f)]
    public float sensitivity = 50f;
    public float smoothSpeed = 20f;
    public float filterSmoothing = 0.1f;

    [Header("거리 보정 설정")]
    [Tooltip("가장 편안한 위치에서의 refLen 값을 입력하세요 (기본 0.2)")]
    public float standardRefLen = 0.2f;

    private Rigidbody _rb;
    private Camera _mainCam;
    private Vector3 _initialCubePos;
    private Vector3 _initialHandPos;
    private Vector3 _filteredHandPos;
    private bool _isFirstDetection = true;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCam = Camera.main;

        // 물리 설정 초기화
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        // 데이터 매니저가 없거나 손이 감지되지 않으면 초기화 후 리턴
        if (dataManager == null || !dataManager.IsHandDetected)
        {
            _isFirstDetection = true;
            return;
        }

        // 1. 거리 측정 및 보정 인자 계산
        // 손목(0)에서 중지 시작점(9) 사이의 거리를 척도로 사용
        float refLen = Vector3.Distance(dataManager.HandJoints[0], dataManager.HandJoints[9]);
        if (refLen < 0.01f) refLen = 0.01f;

        // 사용자가 멀어지면(refLen 감소) 보정치가 커져서 움직임을 증폭함
        float compensationFactor = standardRefLen / refLen;

        // 2. 현재 손 위치 정규화
        Vector3 currentRawPos = dataManager.HandJoints[jointIndex];
        Vector3 currentNormalizedPos = currentRawPos / refLen;

        // 3. 첫 감지 시점의 원점 잡기
        if (_isFirstDetection)
        {
            InitializeReferencePoints(currentNormalizedPos);
            _isFirstDetection = false;
        }

        // 4. 로우 패스 필터 및 변위 계산
        _filteredHandPos = Vector3.Lerp(_filteredHandPos, currentNormalizedPos, filterSmoothing);
        Vector3 handMovement = _filteredHandPos - _initialHandPos;

        // 5. 최종 이동 위치 계산 및 적용
        MoveCube(handMovement, compensationFactor);
    }

    private void InitializeReferencePoints(Vector3 currentPos)
    {
        _initialCubePos = _rb.position;
        _initialHandPos = currentPos;
        _filteredHandPos = currentPos;
    }

    private void MoveCube(Vector3 movement, float compensation)
    {
        // 화면 방향 벡터 계산 (XZ 평면화)
        Vector3 camRight = _mainCam.transform.right;
        Vector3 camForward = _mainCam.transform.forward;

        camRight.y = 0;
        camForward.y = 0;
        camRight.Normalize();
        camForward.Normalize();

        // 최종 방향 및 이동량 계산
        float finalSensitivity = sensitivity * compensation;
        Vector3 moveDirection = (camRight * -movement.x) + (camForward * movement.y);

        // 목표 위치 설정 (높이는 시작 지점의 높이로 절대 고정)
        Vector3 targetPosition = _initialCubePos + (moveDirection * finalSensitivity);
        targetPosition.y = _initialCubePos.y;

        // 물리 기반의 부드러운 위치 추적
        Vector3 nextPos = Vector3.Lerp(_rb.position, targetPosition, Time.fixedDeltaTime * smoothSpeed);
        _rb.MovePosition(nextPos);
    }
}
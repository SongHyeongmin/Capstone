using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public AudioSource audioSource; 
    public AudioClip successClip;   // 성공 효과음 
    public AudioClip failClip;      // 실패 효과음 
    
    [Header("UI 연결")]
    public GameObject resultPanel; // 결과 판넬 (평소엔 비활성화)
    public TextMeshProUGUI totalText, successText, failText, accuracyText, timeText;
    
    public TextMeshProUGUI scoreText; // 점수 표시 텍스트 
    public TextMeshProUGUI timerText; // 타이머 표시 텍스트
    [Header("Game State")]
    public int score = 0;
    public bool isGameOver = false;

    [Header("Game Settings")]
    public float gameTimeLimit = 5f; // 게임 시간 제한 (초)
    public float gameTimer = 5f; // 1분 게임
    
    public int totalBallCount = 0;
    public int destroyedBallCount = 0;
    public int successBallCount = 0;
    
    public Spawner spawner;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        scoreText.text = "점수: " + score;
        resultPanel.SetActive(false); // 시작할 땐 꺼두기
    }
    private void OnEnable()
    {
        // 공이 파괴될 때 실행할 함수 연결
        Ball.OnBallDestroyed += HandleBallDestroyed;
    }

    private void OnDisable()
    {
        // 메모리 누수 방지를 위해 해제
        Ball.OnBallDestroyed -= HandleBallDestroyed;
    }

    void Update()
    {

        if (isGameOver)
        {
            if(Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("Main Scene");
            }
        }
        gameTimer -= Time.deltaTime;
        timerText.text = "남은 시간: " + gameTimer.ToString("F0");

        if (gameTimer <= 0)
            ShowResult();
    }
    private void HandleBallDestroyed(bool isSuccess)
    {
        totalBallCount++;
        if (isSuccess)
        {
            Debug.Log("환호성 소리 재생!");
            audioSource.PlayOneShot(successClip, 1.0f);
            scoreText.text = "점수: " + score;
            successBallCount++;
        }
        else
        {
            Debug.Log("실패 사운드 재생!");
            audioSource.PlayOneShot(failClip, 1.0f);
            destroyedBallCount++;
        }
        
        Invoke("RequestNewBall", 3.0f);
    }

    private void RequestNewBall()
    {
        spawner.SpawnBall(); // 스포너의 생성 함수 호출
    }
    
    public void ShowResult()
    {
        Time.timeScale = 0;
        isGameOver = true;
        resultPanel.SetActive(true); // 결과창 띄우기
        
        // 정확도 계산 (0으로 나누기 방지)
        float accuracy = totalBallCount > 0 ? (float)successBallCount / totalBallCount * 100f : 0f;

        // UI에 값 채우기
        totalText.text = $"총 공 갯수: {totalBallCount}개";
        successText.text = $"성공: {successBallCount}회";
        failText.text = $"실패: {destroyedBallCount}회";
        accuracyText.text = $"정확도: {accuracy:F1}%"; // 소수점 한자리
        timeText.text = $"플레이 타임: {gameTimeLimit:F0}초";

        Time.timeScale = 0; // 게임 일시정지
    }
}

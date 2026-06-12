using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
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
    
    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public TextMeshProUGUI readyText;
    public TextMeshProUGUI countdownText;
    
    public Spawner spawner;
    
    [Header("💡 핸드 트래킹 제어")]
    // 💡 지금 손을 인식해서 오브젝트를 움직여도 되는가? (다른 스크립트들이 훔쳐볼 변수)
    public bool isHandTrackingActive = false;

    public bool playIsReady = false;
    
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

        StartCoroutine(FadeInRoutine());
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
    
    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        Color color = fadeImage.color;
    
        // 1. 일단 시작하자마자 완전 깜깜한 검은색 화면으로 세팅하고 켜기
        color.a = 1f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(true);

        // 💡 [핵심 추가] 여기서 페이드인을 시작하기 전에 무조건 3초 동안 멍 때리며 대기!
        // 이 3초 동안 HandDataManager가 영점을 열심히 잡고 있을 거야.
        // yield return new WaitForSeconds(3.0f);
        // 2. 3초 대기가 끝나면 그제야 천천히 알파값을 깎으면서 투명하게 만듦 (페이드 인)
        float calibrationTime = 3.0f;
        while (calibrationTime > 0f)
        {
            if (countdownText != null)
            {
                // 소수점 버리고 정수(3, 2, 1)로만 보여주고 싶다면 :F0
                // 소수점 한자리(3.0, 2.5)까지 보여주고 싶다면 :F1
                countdownText.text = calibrationTime.ToString("F1"); 
            }

            calibrationTime -= Time.deltaTime; // 시간 깎기
            yield return null; // 1프레임 대기
        }

        // 3초 끝나면 타이머 텍스트는 깔끔하게 숨기기
        if (countdownText != null) 
            countdownText.gameObject.SetActive(false);
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null; 
        }
        // 3. 완전히 투명해지면 노트 클릭이나 손 인식을 방해하지 않게 오브젝트를 꺼둠
        fadeImage.gameObject.SetActive(false);
        playIsReady = true; // 이제 플레이 준비 완료!
        spawner.SpawnBall(); // 게임 시작하자마자 첫 공 스폰
        isHandTrackingActive = true;
    }
}

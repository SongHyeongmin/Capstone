using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

public class RythmGameManager : MonoBehaviour
{
    public static RythmGameManager Instance;

    [Header("게임 데이터")]
    public int totalNotes = 0;      // 전체 노트 갯수
    public int hitNotes = 0;        // 맞춘 노트
    public int missNotes = 0;       // 놓친 노트
    public float gameTime = 0f;     // 게임 진행 시간
    public bool isGameActive = false;
    
    
    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public TextMeshProUGUI readyText;
    public TextMeshProUGUI countdownText;
    
    
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        StartCoroutine(FadeInRoutine());
    }

    void Update()
    {
        if (isGameActive)
        {
            gameTime += Time.deltaTime;
        }
    }

    // 정확도 계산 (퍼센트)
    public float GetAccuracy()
    {
        int processedNotes = hitNotes + missNotes;
        if (processedNotes == 0) return 100f; // 아직 아무것도 안 했으면 100%로 간주
        
        return ((float)hitNotes / processedNotes) * 100f;
    }

    // 노트 판정 결과 전달
    public void AddHit()
    {
        hitNotes++;
        Debug.Log($"Hit! 현재 정확도: {GetAccuracy():F2}%");
    }

    public void AddMiss()
    {
        missNotes++;
        Debug.Log($"Miss... 현재 정확도: {GetAccuracy():F2}%");
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
        isGameActive = true;
    }
}

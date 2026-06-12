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
    public int comboCount = 0;       // 현재 콤보 수
    
    
    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public TextMeshProUGUI readyText;
    public TextMeshProUGUI countdownText;

    public TextMeshProUGUI comboText;
    
    [Header("🔥 콤보 연출 색상 세팅")]
    public Color defaultComboColor = Color.white;       // 기본 콤보 색상 (흰색)
    public Color bronzeComboColor = new Color(0.9f, 0.5f, 0.2f); // 2콤보 이상 (동색)
    public Color silverComboColor = new Color(0.1f, 0.75f, 0.75f); // 4콤보 이상 (하늘색)
    public Color goldComboColor = new Color(1f, 0.85f, 0f);     // 6콤보 이상 (금색)
    public Color fireComboColor = new Color(1f, 0.2f, 0.2f);    // 8콤보 이상 (빨간 네온)
    
    private Coroutine comboActionCoroutine;
    
    [Header("🔊 오디오 설정")]
    public AudioSource audioSource;
    public AudioClip bgmClip; // 인스펙터에서 네가 준비한 리듬게임 음악 파일(MP3/WAV)을 여기에 쏙!
    
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
        audioSource.PlayOneShot(bgmClip);
    }
    
    public void UpdateComboUI()
    {
        if (comboText == null) return;

        if (comboCount > 0)
        {
            comboText.gameObject.SetActive(true); // 콤보가 있을 때만 보여줌
            comboText.text = $"<size=120>{comboCount}</size>\n<size=80>COMBO</size>";

            
            if (comboCount >= 8) comboText.color = fireComboColor;
            else if (comboCount >= 6) comboText.color = goldComboColor;
            else if (comboCount >= 4) comboText.color = silverComboColor;
            else if (comboCount >= 2) comboText.color = bronzeComboColor;
            else comboText.color = defaultComboColor;

            //  기존에 돌던 연출 코루틴이 있다면 강제로 끄고 새로 시작 (콤보가 빠르게 바뀌어도 연출이 꼬이지 않게)
            if (comboActionCoroutine != null) StopCoroutine(comboActionCoroutine);
            comboActionCoroutine = StartCoroutine(JuicyComboAnimation());
        }
        else
        {
            // 0 콤보일 때는 화면을 깔끔하게 비워두거나 숨김 (리듬게임 국룰)
            comboText.gameObject.SetActive(false); 
        }
    }
    
    private IEnumerator JuicyComboAnimation()
    {
        RectTransform textRect = comboText.GetComponent<RectTransform>();
        if (textRect == null) yield break;

        // 회전이나 위치 변형을 초기화해두기
        textRect.localRotation = Quaternion.identity;

        float duration = 0.12f; // 연출이 일어나는 아주 짧은 시간 (리듬게임은 타이밍이 생명)
        float timer = 0f;
    
        // 💡 시작할 때 원래 크기의 1.5배로 화면 앞으로 튀어나오고, 살짝 삐딱하게 회전 펀치!
        Vector3 startScale = Vector3.one * 1.5f;
        Vector3 targetScale = Vector3.one;
    
        // 타격감을 주려고 콤보가 올라갈 때마다 좌우로 번갈아가며 살짝 킹받게 꺾어버림
        float randomZRotation = Random.Range(-5f, 5f); 
        textRect.localRotation = Quaternion.Euler(0, 0, randomZRotation);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // 부드럽게 줄어들게 Lerp 멕이기
            textRect.localScale = Vector3.Lerp(startScale, targetScale, progress);
        
            // 회전도 원래대로 스르륵 돌아옴
            textRect.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, randomZRotation), Quaternion.identity, progress);

            yield return null;
        }
    
        // 마지막 프레임에 원래 상태로 칼같이 고정
        textRect.localScale = targetScale;
        textRect.localRotation = Quaternion.identity;
    }
}

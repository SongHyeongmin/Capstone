using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
public class MainMenuController : MonoBehaviour
{
    [Header("페이드 설정")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    
    public void StartGame()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeOutRoutine("Game Scene"));
    }
    
    public void StartRythmGame()
    {
        Time.timeScale = 1f;
        StartCoroutine(FadeOutRoutine("Rythm Scene"));
    }

    public void QuitGame()
    {
        Application.Quit(); // 게임 종료 (빌드 후 작동)
    }
    
    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;
        Color color = fadeImage.color;
        
        // 시작은 완전 불투명한 검은색 (Alpha = 1)
        color.a = 1f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(true);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // 시간에 따라 알파값을 1에서 0으로 깎아나감
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null; // 1프레임 대기
        }

        // 완전히 투명해지면 클릭 방해 안 되게 오브젝트를 꺼둠
        fadeImage.gameObject.SetActive(false);
    }
    
    private IEnumerator FadeOutRoutine(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {   
            timer += Time.deltaTime;
            // 시간에 따라 알파값을 0에서 1로 채워나감
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }
}
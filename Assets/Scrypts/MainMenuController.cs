using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // 버튼에 연결할 함수
    public void StartGame()
    {
        Time.timeScale = 1f;
        
        SceneManager.LoadScene("Game Scene");
    }

    public void QuitGame()
    {
        Application.Quit(); // 게임 종료 (빌드 후 작동)
    }
}
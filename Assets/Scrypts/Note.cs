using UnityEngine;
using TMPro;
public class Note : MonoBehaviour
{
    public int requiredFingerCount; // 5, 4, 3, 0 중 하나
    public TextMeshPro noteText;    // 숫자를 표시할 텍스트 객체
    public bool isProcessed = false;

    // 생성될 때 숫자를 세팅해주는 함수
    public void SetNote(int count)
    {
        requiredFingerCount = count;
        if (noteText != null)
        {
            noteText.text = count.ToString();
        }
    }

    // 성공했을 때 효과
    public void OnHit()
    {
        isProcessed = true;
        // 여기서 색을 바꾸거나 파티클을 터뜨려!
        GetComponentInChildren<SpriteRenderer>().color = Color.gray; 
        Debug.Log($"노트 클리어: {requiredFingerCount}");
    }
}

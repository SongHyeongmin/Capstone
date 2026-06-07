using UnityEngine;
using TMPro;

public class Note : MonoBehaviour
{
    public int requiredFingerCount; // 5, 4, 3, 0 중 하나
    public TextMeshPro noteText; // 숫자를 표시할 텍스트 객체
    public bool isProcessed = false;
    public bool isHandTouching = false;

    public Material[] noteMaterial;
    public MeshRenderer noteMeshRenderer;

    [Header("이펙트 설정")] public GameObject hitEffectPrefab;
    public Color currentNoteColor;
    public void Start()
    {
        currentNoteColor = gameObject.GetComponent<Renderer>().material.color; 
    }

    public void SetNote(int count)
    {
        requiredFingerCount = count;
        if (noteText != null)
        {
            noteText.text = requiredFingerCount.ToString();
        }

        ApplyMaterial(requiredFingerCount);
    }

    private void ApplyMaterial(int count)
    {
        switch (count)
        {
            case 5:
                noteMeshRenderer.sharedMaterial = noteMaterial[0];
                break;
            case 4:
                noteMeshRenderer.sharedMaterial = noteMaterial[1];
                break;
            case 3:
                noteMeshRenderer.sharedMaterial = noteMaterial[2];
                break;
            case 0:
                noteMeshRenderer.sharedMaterial = noteMaterial[3];
                break;
            default:
                Debug.LogWarning("알 수 없는 손가락 개수: " + count);
                break;
        }
    }
    // 성공했을 때 효과
    public void OnHit()
    {
        isProcessed = true;
        GameObject effectObj = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            
        // 2. 파티클 컴포넌트 가져오기
        ParticleSystem ps = effectObj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 💡 [핵심] 파티클 메인 모듈의 시작 색상을 현재 노트 색상으로 강제 변경!
            var mainModule = ps.main;
            mainModule.startColor = currentNoteColor;
        }
        // 여기서 색을 바꾸거나 파티클을 터뜨려!
        // GetComponentInChildren<SpriteRenderer>().color = Color.gray; 
        Debug.Log($"노트 클리어: {requiredFingerCount}");
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            isHandTouching = true;
            Debug.Log("손이 노트와 닿음!");
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            isHandTouching = false; // 선과 노트가 떨어졌으니 false로 설정
            Debug.Log("note와 충돌 종료!");
        }
    }
}

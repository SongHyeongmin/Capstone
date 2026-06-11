using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    // 인스펙터에서 숫자와 머티리얼을 직관적으로 매칭하기 위한 구조체
    [System.Serializable]
    public struct NoteMaterialData
    {
        public int fingerCount;         // 5, 4, 3, 0 등
        public Material material;       // 해당 숫자에 적용할 머티리얼
    }

    [Header("참조")]
    public ScanLine scanLine;               

    [Header("노트 설정")]
    public GameObject notePrefab;          
    
    [Header("💡 숫자별 머티리얼 설정")]
    // 인스펙터에서 사이즈를 4로 만들고 5, 4, 3, 0에 맞춰 각각 머티리얼을 넣어줘!
    public NoteMaterialData[] noteMaterials;

    private int[] fingerCountPool = { 5, 4, 3, 0 };

    [Header("스폰 범위 설정")]
    public float minX = -6.0f;             
    public float maxX = 6.0f;              
    public float minY = -4.0f;             
    public float maxY = 4.0f;              

    [Header("💡 절대 안전거리 설정")]
    public float absoluteSafetyDistance = 2.0f; 

    [Header("자동 스폰 타이머")]
    public float spawnInterval = 2.0f;     
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval && RythmGameManager.Instance.isGameActive)
        {
            SpawnRandomNote();
            timer = 0f;
        }
    }

    public void SpawnRandomNote()
    {
        if (notePrefab == null || scanLine == null) return;

        float randomX = Random.Range(minX, maxX);
        float randomY = 0f;
        float currentLineY = scanLine.CurrentY;

        int attempts = 0;
        bool isValidPosition = false;

        while (!isValidPosition && attempts < 10)
        {
            attempts++;

            if (scanLine.IsMovingUp)
            {
                randomY = Random.Range(minY, currentLineY);
            }
            else
            {
                randomY = Random.Range(currentLineY, maxY);
            }

            if (Mathf.Abs(randomY - currentLineY) >= absoluteSafetyDistance)
            {
                isValidPosition = true; 
            }
        }
        
        if (!isValidPosition) return;

        randomY = Mathf.Clamp(randomY, minY, maxY);
        Vector3 spawnPosition = new Vector3(randomX, randomY, 0f);

        // 1. 숫자 랜덤 추출 (이미 하던 방식 그대로!)
        int chosenFingerCount = fingerCountPool[Random.Range(0, fingerCountPool.Length)];

        // 2. 프리팹 생성
        GameObject newNoteObj = Instantiate(notePrefab, spawnPosition, Quaternion.identity);
        Note noteComponent = newNoteObj.GetComponentInChildren<Note>();
        Debug.Log(chosenFingerCount);
        if (noteComponent != null)
        {
            noteComponent.requiredFingerCount = chosenFingerCount;
            newNoteObj.name = $"Note_{chosenFingerCount}개";
            noteComponent.SetNote(chosenFingerCount);
        }

        // 3. [핵심 추가] 정해진 숫자에 맞춰 머티리얼 입히기
    }
}
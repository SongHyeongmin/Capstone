using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform spawnPoint;
    [Header("공 설정")]
    public GameObject redBallPrefab;      // 공 프리팹
    public GameObject blueBallPrefab;     // 공 프리팹
	public GameObject yellowBallPrefab;   // 공 프리팹
	public GameObject greenBallPrefab;    // 공 프리팹

    [Header("생성 설정")]
    public float spawnInterval = 2.0f; // 생성 주기
    private float _timer;
    
    void Start()
    {
        SpawnBall();
    }
    
    public void SpawnBall()
    {
        // 랜덤으로 공 색상 선택
        int randomIndex = Random.Range(0, 4);
        GameObject prefabToSpawn = redBallPrefab;

        switch (randomIndex)
        {
            case 0:
                prefabToSpawn = redBallPrefab;
                break;
            case 1:
                prefabToSpawn = blueBallPrefab;
                break;
            case 2:
                prefabToSpawn = yellowBallPrefab;
                break;
            case 3:
                prefabToSpawn = greenBallPrefab;
                break;
        }
        Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        
    }
}

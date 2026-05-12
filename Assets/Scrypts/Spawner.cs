using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("공 설정")]
    public GameObject redBallPrefab;      // 공 프리팹
    public GameObject blueBallPrefab;     // 공 프리팹
	public GameObject yellowBallPrefab;   // 공 프리팹
	public GameObject greenBallPrefab;    // 공 프리팹

    [Header("생성 설정")]
    public float spawnInterval = 2.0f; // 생성 주기
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            SpawnBall();
            _timer = 0f;
        }
    }

    void SpawnBall()
    {
        bool isRed = Random.value > 0.5f;
        // 1. 현재 Spawner 위치에서 공 생성
        if (isRed)
        {
            GameObject newBall = Instantiate(redBallPrefab, transform.position, Quaternion.identity);
            // newBall.tag = "RedBall";
        }
        else
        {
            GameObject newBall = Instantiate(blueBallPrefab, transform.position, Quaternion.identity);
            // newBall.tag = "BlueBall";
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnCheckInterval = 2f;
    public float minDistanceFromPlayer = 20f;
    public float maxDistanceFromPlayer = 40f;
    public int maxZombiesInArea = 5;
    public int maxTotalZombies = 50;

    private float lastSpawnCheck = 0f;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (Time.time > lastSpawnCheck + spawnCheckInterval)
        {
            TrySpawnZombie();
            lastSpawnCheck = Time.time;
        }
    }

    void TrySpawnZombie()
    {
        if (GameObject.FindGameObjectsWithTag("Zombie").Length >= maxTotalZombies)
            return;

        Vector3 spawnPos = GetValidSpawnPosition();
        if (spawnPos != Vector3.zero)
        {
            Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            dir.y = 0f;
            Vector3 candidatePos = player.position + dir.normalized * Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);

            if (IsInPlayerView(candidatePos)) continue;
            if (CountZombiesNear(candidatePos, 15f) >= maxZombiesInArea) continue;

            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Vector3 groundedPos = hit.position + Vector3.up * 0.1f;

                // ✅ 플레이어보다 y + 10 이상 높은 위치는 무시
                if (groundedPos.y > player.position.y + 10f)
                    continue;

                return groundedPos;
            }
        }

        return Vector3.zero; // 실패
    }

    bool IsInPlayerView(Vector3 pos)
    {
        Vector3 dirToPos = pos - player.position;
        float angle = Vector3.Angle(player.forward, dirToPos);

        if (angle < 70f && Vector3.Distance(player.position, pos) < 50f)
        {
            Ray ray = new Ray(player.position + Vector3.up * 1.7f, dirToPos.normalized);
            if (!Physics.Raycast(ray, dirToPos.magnitude, LayerMask.GetMask("Default")))
                return true;
        }

        return false;
    }

    int CountZombiesNear(Vector3 pos, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(pos, radius);
        int count = 0;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Zombie")) count++;
        }
        return count;
    }
}
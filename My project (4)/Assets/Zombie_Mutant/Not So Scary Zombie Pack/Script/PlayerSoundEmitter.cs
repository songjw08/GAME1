using UnityEngine;

public class ZombieSignalEmitter : MonoBehaviour
{
    public float walkRadius = 5f;
    public float runRadius = 10f;
    public float gunshotRadius = 20f;
    public LayerMask zombieLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) // 총 쏘기 (좌클릭)
        {
            AlertZombies(transform.position, gunshotRadius);
        }

        if (Input.GetKey(KeyCode.W))
        {
            float radius = Input.GetKey(KeyCode.LeftShift) ? runRadius : walkRadius;
            AlertZombies(transform.position, radius);
        }
    }

    void AlertZombies(Vector3 position, float radius)
    {
        Collider[] zombies = Physics.OverlapSphere(position, radius, zombieLayer);
        foreach (var zombie in zombies)
        {
            zombie.GetComponent<ZombieAI>()?.SetTargetPosition(position);
        }

        Debug.Log($"좀비 호출: 반경 {radius} | 감지 수 {zombies.Length}");
    }
}
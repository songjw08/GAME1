using UnityEngine;

public class ZombieBarrel : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionEffect;  // 폭발 이펙트
    public float explosionRadius = 10f;  // 폭발 반경
    public float explosionForce = 10f;   // 폭발의 힘
    public float destroyDelay = 2f;      // 폭발 후 지연 시간 (이후 통을 파괴)

    private bool exploded = false;  // 폭발 여부를 체크하는 변수

    public static int remainingZombies = 10;  // 남은 좀비통 수를 추적하는 변수

    private void OnCollisionEnter(Collision collision)
    {
        // 태그가 "Zvirus"인 오브젝트와 충돌 시 폭발
        if (!exploded && collision.gameObject.CompareTag("Zvirus"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        exploded = true;

        // 폭발 이펙트 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // 남은 좀비통 수 감소
        remainingZombies--;

        // 일정 시간 후 통을 파괴
        Destroy(gameObject, destroyDelay);
    }
}
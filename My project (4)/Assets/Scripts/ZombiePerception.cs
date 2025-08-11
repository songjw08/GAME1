using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class ZombiePerception : MonoBehaviour
{
    [Header("Vision")]
    public float visionRadius = 25f;           // = gunshotRadius
    public float visionAngle = 90f;            // 부채꼴 각도 (좌우 합)
    public LayerMask visionObstructionMask;    // 벽 등
    public LayerMask playerMask;

    [Header("Hearing")]
    public float gunshotRadius = 25f;          // visionRadius와 동일
    public float runRadius = 15f;
    public float walkRadius = 6f;

    public Transform eyes; // 좀비 눈 위치(없으면 transform 사용)

    public event Action<Transform> OnPlayerSpotted;           // Chase로
    public event Action<Vector3> OnHeardSomething;            // Investigate로

    Transform player;
    bool playerInSight;

    void OnEnable()
    {
        NoiseEmitter.OnNoise += HandleNoise;
    }

    void OnDisable()
    {
        NoiseEmitter.OnNoise -= HandleNoise;
    }

    void Update()
    {
        TrySeePlayer();
    }

    void TrySeePlayer()
    {
        if (!player)
        {
            // 비싼 Find를 매프레임 하지 않도록, Start나 ZombieAI에서 주입하는 걸 권장
            var col = Physics.OverlapSphere(transform.position, visionRadius, playerMask);
            if (col.Length > 0) player = col[0].transform;
            else return;
        }

        Vector3 dirToPlayer = (player.position - GetEyes().position);
        float distance = dirToPlayer.magnitude;

        if (distance > visionRadius) { playerInSight = false; return; }

        float angle = Vector3.Angle(transform.forward, dirToPlayer.normalized);
        if (angle > visionAngle * 0.5f) { playerInSight = false; return; }

        // Raycast로 가려졌는지 체크
        if (Physics.Raycast(GetEyes().position, dirToPlayer.normalized, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (((1 << hit.collider.gameObject.layer) & playerMask) != 0)
            {
                // 플레이어다!
                playerInSight = true;
                OnPlayerSpotted?.Invoke(player);
            }
            else
            {
                playerInSight = false;
            }
        }
    }

    void HandleNoise(Vector3 noisePos, float radius)
    {
        float dist = Vector3.Distance(transform.position, noisePos);
        if (dist <= radius)
        {
            // 총소리/달리기/걷기 모든 소리 공통 진입
            // 단, 설계상 crouch는 radius = 0 또는 매우 작게 넘기도록
            OnHeardSomething?.Invoke(noisePos);
        }
    }

    Transform GetEyes() => eyes ? eyes : transform;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Vision cone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        Vector3 leftDir = Quaternion.Euler(0, -visionAngle * 0.5f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, visionAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * visionRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * visionRadius);

        // Hearing radii
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, gunshotRadius);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, runRadius);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, walkRadius);
    }
#endif
}

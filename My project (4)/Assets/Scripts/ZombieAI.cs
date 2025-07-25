using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    public enum State { Idle, Investigate, Chase, Attack }

    public float attackRange = 1.8f;
    public float loseSightTime = 3f; // 시야에서 놓치면 이 시간 뒤 Idle/Investigate로 전환
    public float investigateStopDistance = 1.5f;

    private State state = State.Idle;
    private NavMeshAgent agent;
    private ZombiePerception perception;
    private Transform player;
    private float lastSeenTime;
    private Vector3 lastHeardPos;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<ZombiePerception>();
    }

    void OnEnable()
    {
        perception.OnPlayerSpotted += HandlePlayerSpotted;
        perception.OnHeardSomething += HandleHeard;
    }

    void OnDisable()
    {
        perception.OnPlayerSpotted -= HandlePlayerSpotted;
        perception.OnHeardSomething -= HandleHeard;
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                // 대기 애니메이션 등
                break;

            case State.Investigate:
                if (Vector3.Distance(transform.position, lastHeardPos) <= investigateStopDistance)
                {
                    // 도착해서 둘러보기 -> 일정 시간 후 Idle로
                    StartCoroutine(BackToIdleAfter(2f));
                }
                break;

            case State.Chase:
                if (!player)
                {
                    if (Time.time - lastSeenTime > loseSightTime) state = State.Idle;
                    return;
                }

                agent.SetDestination(player.position);

                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attackRange)
                {
                    state = State.Attack;
                    // 공격 애니메이션 트리거
                }
                else
                {
                    // 아직 추격
                }

                if (Time.time - lastSeenTime > loseSightTime)
                {
                    // 마지막 본 곳으로 이동 후 Investigate로 전환하는 로직도 가능
                    state = State.Investigate;
                    lastHeardPos = player.position;
                    agent.SetDestination(lastHeardPos);
                    player = null;
                }
                break;

            case State.Attack:
                // 공격 애니메이션 이벤트에서 실제 데미지 처리
                if (player)
                {
                    float d = Vector3.Distance(transform.position, player.position);
                    if (d > attackRange)
                        state = State.Chase;
                }
                else state = State.Idle;
                break;
        }
    }

    void HandlePlayerSpotted(Transform p)
    {
        player = p;
        lastSeenTime = Time.time;
        state = State.Chase;
    }

    void HandleHeard(Vector3 pos)
    {
        // 이미 Chase 중이면 무시하거나, 시야 안이라면 우선 순위: Chase > Investigate
        if (state == State.Chase) return;

        lastHeardPos = pos;
        agent.SetDestination(lastHeardPos);
        state = State.Investigate;
    }

    IEnumerator BackToIdleAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (state == State.Investigate)
            state = State.Idle;
    }
}

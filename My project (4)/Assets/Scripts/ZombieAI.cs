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
    private bool isAggroed = false;// 한번 감지되면 true로 고정

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
                if (!player) return;

                agent.SetDestination(player.position);

                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attackRange)
                {
                    state = State.Attack;
                    return;
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
        isAggroed = true;              // 어그로 획득
        state = State.Chase;          // 바로 추적
    }

    void HandleHeard(Vector3 pos)
    {
        if (isAggroed) return;

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

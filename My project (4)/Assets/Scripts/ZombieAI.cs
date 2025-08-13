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
    private Animator animator;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<ZombiePerception>();
        animator = GetComponent<Animator>();
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
        if (agent.isStopped)
        {
            // 이동 정지
            agent.velocity = Vector3.zero;

            // 회전도 막고 싶다면
            agent.angularSpeed = 0f;

            // 혹시라도 Animator에서 걷기 애니메이션 꺼야 한다면:
            animator.SetBool("isWalking", false);
        }
        switch (state)
        {
            case State.Idle:
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", false);
                break;

            case State.Investigate:
                animator.SetBool("isWalking", true);
                animator.SetBool("isAttacking", false);
                if (Vector3.Distance(transform.position, lastHeardPos) <= investigateStopDistance)
                {
                    StartCoroutine(BackToIdleAfter(2f));
                }
                break;

            case State.Chase:
                animator.SetBool("isWalking", true);
                animator.SetBool("isAttacking", false);
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
                agent.isStopped = true;
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", true); // 공격 애니메이션 실행

                if (player)
                {
                    float d = Vector3.Distance(transform.position, player.position);
                    if (d > attackRange)
                    {
                        state = State.Chase;
                        animator.SetBool("isAttacking", false); // 공격 종료
                    }
                }
                else
                {
                    state = State.Idle;
                    animator.SetBool("isAttacking", false); // 공격 종료
                    agent.isStopped = false;
                }
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

using Akila.FPSFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    public enum State { Idle, Investigate, Chase, Attack }

    public float attackRange;
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

    // 공격 파라미터
    [SerializeField] private float attackDamage = 20f;      // 1회 공격 데미지
    [SerializeField] private float attackHitRadius = 0.7f;  // 히트 판정 반경
    [SerializeField] private Transform attackPoint;         // 손/입 등 임팩트 지점(빈 오브젝트)
                                                            // 보조 판정(스피어를 살짝 빗나간 경우)에 허용할 최대 거리
                                                            // 너무 쉽게 맞지 않도록 작은 값으로 시작해서 조절
    [SerializeField] private float fallbackDistance = 0.6f; // meters

    private bool hasDealtDamageThisSwing; // 한 번의 스윙에서 중복 히트 방지

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<ZombiePerception>();
        animator = GetComponent<Animator>();
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
        agent.stoppingDistance = attackRange - 0.1f;
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
                animator.applyRootMotion = true;
                break;

            case State.Investigate:

                if (Vector3.Distance(transform.position, lastHeardPos) <= investigateStopDistance)
                {
                    StartCoroutine(BackToIdleAfter(2f));
                }
                break;

            case State.Chase:
                Debug.Log("[ZombieAI] 플레이어 어그로 on");
                animator.applyRootMotion = false;
                agent.isStopped = false;
                animator.SetBool("isWalking", true);
                //animator.SetBool("isAttacking", false);
                if (!player) return;

                agent.SetDestination(player.position);

                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attackRange)
                {
                    Debug.Log("[ZombieAI] attackRange 안으로 진입");
                    state = State.Attack;
                    return;
                }
                break;

            case State.Attack:
                Debug.Log("[ZombieAI] state attack으로 전환");
                animator.applyRootMotion = true;
                animator.SetBool("isAttacking", true);
                animator.SetBool("isWalking", false);
                agent.isStopped = true; // 이동 멈추게 함

                if (player)
                {
                    float d = Vector3.Distance(transform.position, player.position);
                    if (d > attackRange)
                    {
                        state = State.Chase;
                        animator.SetBool("isAttacking", false);
                        agent.isStopped = false; //다시 추적 허용
                        animator.applyRootMotion = false;
                        break;
                    }
                    if (!perception.IsInFOV(player))
                    {
                        RotateTowards(player.position);
                    }
                }
                else
                {
                    state = State.Idle;
                    animator.SetBool("isAttacking", false);
                    agent.isStopped = false; //다시 이동 가능하게
                }
                break;
        }

    }
    public void OnAttackStart()
    {
        hasDealtDamageThisSwing = false;
    }

    public void OnAttackHit()
    {
        if (hasDealtDamageThisSwing) return;
        if (player == null) return;

        Vector3 origin = attackPoint
            ? attackPoint.position
            : transform.position + transform.forward * 0.8f + Vector3.up * 1.0f;

        // 1차: 오버랩 스피어로 맞은 콜라이더 중 플레이어 트랜스폼 계통만 필터
        Collider[] hits = Physics.OverlapSphere(origin, attackHitRadius, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            Transform t = hits[i].transform;
            if (t == player || t.IsChildOf(player))
            {
                var dmg = t.GetComponentInParent<Damageable>();
                if (dmg != null)
                {
                    dmg.Damage(attackDamage, gameObject);
                    hasDealtDamageThisSwing = true;
                    return;
                }
            }
        }

        // 2차: 레이어를 쓰지 않으므로 거리 기반 보조 판정
        float dist = Vector3.Distance(origin, player.position);
        if (dist <= fallbackDistance)
        {
            var dmg = player.GetComponentInParent<Akila.FPSFramework.Damageable>();
            if (dmg != null)
            {
                dmg.Damage(attackDamage, gameObject);
                hasDealtDamageThisSwing = true;
            }
        }

    }

    public void OnAttackEnd()
    {
        hasDealtDamageThisSwing = false;
    }

    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void HandlePlayerSpotted(Transform p)
    {
        if (state == State.Attack) return;
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
    public State CurrentState => state;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (attackPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackHitRadius);
        }
    }

#endif
}

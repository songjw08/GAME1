using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private bool isMoving = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void SetTargetPosition(Vector3 position)
    {
        agent.SetDestination(position);
        animator.SetBool("isWalking", true);
        isMoving = true;
    }

    void Update()
    {
        if (isMoving && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            animator.SetBool("isWalking", false);
            isMoving = false;
        }
    }
}
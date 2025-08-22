using System.Collections;
using UnityEngine;

public class ZombieIdleRandomizer : MonoBehaviour
{
    private Animator animator;
    private ZombieAI ai;

    private void Start()
    {
        animator = GetComponent<Animator>();
        ai = GetComponent<ZombieAI>();
        StartCoroutine(IdleCycle());
    }

    IEnumerator IdleCycle()
    {
        while (true)
        {
            if (ai.CurrentState == ZombieAI.State.Idle) // Idle 상태일 때만
            {
                int nextIndex = Random.Range(0, 4); // 0 ~ 3
                animator.SetInteger("idleIndex", nextIndex);
                animator.SetTrigger("changeIdle");

                yield return new WaitUntil(() =>
                    animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f &&
                    animator.IsInTransition(0) == false
                );
            }
            else
            {
                yield return null; // Idle이 아닐 때는 대기
            }
        }
    }
}

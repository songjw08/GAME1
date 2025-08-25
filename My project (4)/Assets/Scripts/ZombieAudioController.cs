using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ZombieAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZombieAI ai;

    [Header("Idle Audio")]
    [SerializeField] private AudioSource[] idleSources;
    [SerializeField] private float idleMinGap = 1.5f;
    [SerializeField] private float idleMaxGap = 3.0f;
    [SerializeField] private bool idleAvoidRepeat = true;
    [SerializeField] private bool stopOnExitIdle = true;

    [Header("Chase Audio")]
    [SerializeField] private AudioSource[] chaseSources;
    [SerializeField] private float chaseMinGap = 0.8f;
    [SerializeField] private float chaseMaxGap = 1.8f;
    [SerializeField] private bool chaseAvoidRepeat = true;
    [SerializeField] private bool stopOnExitChase = true;

    [Header("Attack Audio")]
    [SerializeField] private AudioSource[] attackSources;     
    [SerializeField] private float attackMinGap = 0.2f;       
    [SerializeField] private float attackMaxGap = 0.6f;
    [SerializeField] private bool attackAvoidRepeat = true;
    [SerializeField] private bool stopOnExitAttack = true;

    [Header("3D audio common")]
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f; // 1 = 3D
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    [SerializeField] private float dopplerLevel = 0f;

    [Header("Idle distances")]
    [SerializeField] private float idleMinDistance = 2.5f;
    [SerializeField] private float idleMaxDistance = 12f;

    [Header("Chase distances")]
    [SerializeField] private float chaseMinDistance = 3.0f;
    [SerializeField] private float chaseMaxDistance = 22f;

    [Header("Attack distances")]
    [SerializeField] private float attackMinDistance = 2.0f;
    [SerializeField] private float attackMaxDistance = 10f;

    private int lastIdleIndex = -1;
    private int lastChaseIndex = -1;
    private int lastAttackIndex = -1;

    private Coroutine routine;

    private void Awake()
    {
        if (ai == null) ai = GetComponent<ZombieAI>();
        ApplyAllAudio3D();
    }

    private void OnEnable()
    {
        routine = StartCoroutine(StateAudioLoop());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        StopAll(idleSources);
        StopAll(chaseSources);
    }

    private IEnumerator StateAudioLoop()
    {
        while (true)
        {
            if (ai == null)
            {
                yield return null;
                continue;
            }

            if (ai.CurrentState == ZombieAI.State.Idle)
            {
                yield return StartCoroutine(PlayCategoryLoop(
                    () => ai.CurrentState == ZombieAI.State.Idle,
                    idleSources,
                    idleMinGap, idleMaxGap,
                    idleAvoidRepeat,
                    () => lastIdleIndex,
                    v => lastIdleIndex = v,
                    stopOnExitIdle
                ));
            }
            else if (ai.CurrentState == ZombieAI.State.Chase)
            {
                yield return StartCoroutine(PlayCategoryLoop(
                    () => ai.CurrentState == ZombieAI.State.Chase,
                    chaseSources,
                    chaseMinGap, chaseMaxGap,
                    chaseAvoidRepeat,
                    () => lastChaseIndex,
                    v => lastChaseIndex = v,
                    stopOnExitChase
                ));
            }
            else if (ai.CurrentState == ZombieAI.State.Attack)
            {
                yield return StartCoroutine(PlayCategoryLoop(
                    () => ai.CurrentState == ZombieAI.State.Attack,
                    attackSources,
                    attackMinGap, attackMaxGap,
                    attackAvoidRepeat,
                    () => lastAttackIndex,
                    v => lastAttackIndex = v,
                    stopOnExitAttack
                ));
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator PlayCategoryLoop(
        System.Func<bool> statePredicate,
        AudioSource[] sources,
        float minGap, float maxGap,
        bool avoidRepeat,
        System.Func<int> getLastIndex,
        System.Action<int> setLastIndex,
        bool stopOnExit
    )
    {
        if (sources == null || sources.Length == 0)
        {
            yield return null;
            yield break;
        }

        while (statePredicate())
        {
            int idx = PickRandomIndex(sources.Length, avoidRepeat, getLastIndex());
            setLastIndex(idx);

            AudioSource src = sources[idx];
            if (src != null)
            {
                src.Play();

                while (statePredicate() && src.isPlaying)
                    yield return null;

                float gap = Random.Range(minGap, maxGap);
                float t = 0f;
                while (statePredicate() && t < gap)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }

        if (stopOnExit) StopAll(sources);
    }

    private int PickRandomIndex(int length, bool avoidRepeat, int lastIdx)
    {
        if (!avoidRepeat || length <= 1)
            return Random.Range(0, length);

        int tries = 0;
        int idx = lastIdx;
        while (tries < 8 && idx == lastIdx)
        {
            idx = Random.Range(0, length);
            tries++;
        }
        return idx;
    }

    private void StopAll(AudioSource[] sources)
    {
        if (sources == null) return;
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null && sources[i].isPlaying)
                sources[i].Stop();
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 인스펙터 값 바꾸면 즉시 반영
        ApplyAllAudio3D();
    }
#endif

    [ContextMenu("Apply 3D audio settings now")]
    private void ApplyAllAudio3D()
    {
        ConfigureCategory(idleSources, idleMinDistance, idleMaxDistance);
        ConfigureCategory(chaseSources, chaseMinDistance, chaseMaxDistance);
        ConfigureCategory(attackSources, attackMinDistance, attackMaxDistance);
    }

    private void ConfigureCategory(AudioSource[] sources, float minDist, float maxDist)
    {
        if (sources == null) return;

        float safeMin = Mathf.Max(0.01f, minDist);
        float safeMax = Mathf.Max(safeMin + 0.01f, maxDist);

        for (int i = 0; i < sources.Length; i++)
        {
            var src = sources[i];
            if (!src) continue;

            src.playOnAwake = false;
            src.spatialBlend = spatialBlend;
            src.rolloffMode = rolloffMode;
            src.dopplerLevel = dopplerLevel;
            src.minDistance = safeMin;
            src.maxDistance = safeMax;
        }
    }
    /*
    public void PlayAttackOneShot()
    {
        if (attackSources == null || attackSources.Length == 0) return;
        int idx = PickRandomIndex(attackSources.Length, attackAvoidRepeat, () => lastAttackIndex);
        lastAttackIndex = idx;
        var src = attackSources[idx];
        if (src != null) src.Play();
    }
    */
}

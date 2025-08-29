using UnityEngine;
using Akila.FPSFramework;

[RequireComponent(typeof(Damageable))]
[RequireComponent(typeof(ZombieAI))]
public class ZombieAggroOnDamage : MonoBehaviour
{
    private Damageable dmg;
    private ZombieAI ai;
    private float lastHealth;

    private void Awake()
    {
        dmg = GetComponent<Damageable>();
        ai = GetComponent<ZombieAI>();
        lastHealth = dmg.health;
    }

    private void Update()
    {
        // 체력이 줄어든 프레임을 감지
        if (dmg.health < lastHealth)
        {
            Transform attacker = dmg.damageSource ? dmg.damageSource.transform : null;

            // 공격자를 알면 그쪽으로 즉시 어그로
            ai.ForceAggro(attacker);

            // 다음 비교를 위해 갱신
            lastHealth = dmg.health;
            return;
        }

        // 체력이 회복됐거나 동일하면 기준값만 갱신
        if (dmg.health != lastHealth)
            lastHealth = dmg.health;
    }
}

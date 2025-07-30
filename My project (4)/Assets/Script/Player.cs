using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("UI Settings")]
    public Text zombieCountText;  // UI 텍스트 (남은 좀비통 수)

    private void Update()
    {
        // 남은 좀비통 수를 UI에 업데이트
        zombieCountText.text = "Remaining Zombies: " + ZombieBarrel.remainingZombies;

        // 남은 좀비통 수가 0이면 게임 승리
        if (ZombieBarrel.remainingZombies <= 0)
        {
            Debug.Log("You win! All zombies have been destroyed!");
        }
    }
}
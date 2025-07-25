using UnityEngine;

public class OilCan : MonoBehaviour
{
    // 플레이어를 퍼블릭으로 선언하여 Unity 에디터에서 할당 가능
    public GameObject player;

    // 기름통이 주워졌는지 여부를 저장하는 변수
    private bool isPickedUp = false;

    // 플레이어와 충돌했을 때 'F' 키로 기름통을 주울 수 있게 한다
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했을 때 (태그가 Player인 오브젝트)
        if (other.gameObject == player && !isPickedUp)
        {
            // 'F' 키를 눌렀을 때 기름통을 주운다
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 기름통을 주운 상태로 설정
                isPickedUp = true;

                // 기름통 오브젝트를 비활성화하여 주운 것처럼 보이게 함
                gameObject.SetActive(false);

                // 디버그 로그로 기름통을 주웠다고 출력
                Debug.Log("기름통을 주웠습니다!");
            }
        }
    }

    // 기름통이 주워졌는지 확인하는 함수
    public bool IsPickedUp()
    {
        return isPickedUp;
    }
}
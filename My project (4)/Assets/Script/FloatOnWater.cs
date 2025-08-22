using UnityEngine;

public class FloatOnWater : MonoBehaviour
{
    [Tooltip("물의 트랜스폼 (예: WaterPlane 2)")]
    public Transform waterSurface;

    [Tooltip("얼마나 물 위에 떠 있을지")]
    public float floatHeight = 0.5f;

    [Tooltip("떠오르는 속도")]
    public float floatLerpSpeed = 2f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (waterSurface == null)
        {
            Debug.LogWarning("Water Surface가 할당되지 않았습니다!");
        }
    }

    void Update()
    {
        if (waterSurface == null) return;

        // 플레이어 발 위치
        float playerFeetY = transform.position.y - (controller.height / 2f);

        // 물 표면 높이
        float targetY = waterSurface.position.y + floatHeight;

        // 물 위에 있으면
        if (playerFeetY < waterSurface.position.y && IsOverWater())
        {
            // 수직 위치를 부드럽게 올림 (y만 수정)
            Vector3 targetPosition = new Vector3(transform.position.x, targetY + (controller.height / 2f), transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * floatLerpSpeed);
        }
    }

    // 플레이어가 물 위에 있는지 감지
    private bool IsOverWater()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            return hit.transform == waterSurface;
        }
        return false;
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Collider2D가 필수임을 명시
public class TutorialTrashBagTrigger : MonoBehaviour
{
    private Collider2D col; // 이 오브젝트의 Collider2D 참조

    void Awake()
    {
        // Collider2D 컴포넌트 가져오기
        col = GetComponent<Collider2D>();
        // Collider가 Trigger로 설정되어 있는지 확인 (없으면 에러 로그)
        if (!col.isTrigger)
        {
            // col.isTrigger = true; // 자동으로 설정하거나, 에디터에서 설정하도록 경고
            Debug.LogWarning($"'{gameObject.name}'의 Collider2D가 Trigger로 설정되어 있지 않습니다. TutorialTrashBagTrigger가 제대로 작동하지 않을 수 있습니다.");
        }
    }

    // 다른 Collider가 이 트리거 영역에 들어왔을 때 호출됨
    void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 객체가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // Chapter2Manager에 플레이어가 이 특정 쓰레기봉투 근처에 있음을 알림
            Chapter2Manager.Instance?.OnPlayerNearTutorialTrashBagChanged(true);
            Debug.Log($"튜토리얼 쓰봉 트리거 진입: {other.name}");
        }
    }

    // 다른 Collider가 이 트리거 영역에서 나갔을 때 호출됨
    void OnTriggerExit2D(Collider2D other)
    {
        // 나간 객체가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // Chapter2Manager에 플레이어가 이 특정 쓰레기봉투에서 멀어졌음을 알림
            Chapter2Manager.Instance?.OnPlayerNearTutorialTrashBagChanged(false);
            Debug.Log($"튜토리얼 쓰봉 트리거 벗어남: {other.name}");
        }
    }
}
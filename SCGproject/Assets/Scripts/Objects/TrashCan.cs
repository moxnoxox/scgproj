using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Collider2D 필수
public class TrashCan : MonoBehaviour
{
    private bool isPlayerInside = false;     // 플레이어가 트리거 내부에 있는지
    private PlayerMove playerMove;           // 플레이어 스크립트 참조
    public static bool PlayerInside = false; // 전역 접근용 (PlayerMove에서 확인 가능)

    void Awake()
    {
        // 콜라이더가 없으면 자동 추가
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            Debug.LogWarning($"{gameObject.name}: Collider2D가 없어 BoxCollider2D를 추가했습니다.");
        }
        col.isTrigger = true; // 트리거로 설정
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            PlayerInside = true;

            playerMove = other.GetComponent<PlayerMove>();

            // 챕터2 매니저에 상태 전달 (튜토리얼 등)
            Chapter2Manager.Instance?.OnPlayerNearTrashCanChanged(true);

            Debug.Log("플레이어 쓰레기통 진입");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            PlayerInside = false;

            playerMove = null;

            // 챕터2 매니저에 상태 전달
            Chapter2Manager.Instance?.OnPlayerNearTrashCanChanged(false);

            Debug.Log("플레이어 쓰레기통 벗어남");
        }
    }

    // ❌ 드롭이나 파괴는 여기서 처리하지 않음
    // 이제 PlayerMove.DropHeldObject() 내부에서
    // TrashCan.PlayerInside 상태를 확인해 오브젝트를 Destroy()함
}

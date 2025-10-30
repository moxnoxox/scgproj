using UnityEngine;

[RequireComponent(typeof(Collider2D))] // Collider2D가 필수임을 명시
public class TrashCan : MonoBehaviour
{
    private bool isPlayerInside = false; // 플레이어가 트리거 내부에 있는지 여부
    private PlayerMove playerMove;       // 플레이어 스크립트 참조

    void Awake() {
        // 트리거 설정 확인 (없으면 추가하고 설정)
        var col = GetComponent<Collider2D>();
        if (col == null) {
             col = gameObject.AddComponent<BoxCollider2D>(); // 기본 BoxCollider2D 추가
             Debug.LogWarning($"{gameObject.name}: Collider2D가 없어 BoxCollider2D를 추가했습니다.");
        }
        col.isTrigger = true; // 트리거로 반드시 설정
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 객체가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            // 플레이어 스크립트 가져오기
            playerMove = other.GetComponent<PlayerMove>();
            // Chapter2Manager에 플레이어가 근처에 있음을 알림 (튜토리얼용)
            Chapter2Manager.Instance?.OnPlayerNearTrashCanChanged(true);
            Debug.Log("플레이어 쓰레기통 진입");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 충돌에서 벗어난 객체가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerMove = null; // 플레이어 참조 제거
            // Chapter2Manager에 플레이어가 벗어났음을 알림 (튜토리얼용)
            Chapter2Manager.Instance?.OnPlayerNearTrashCanChanged(false);
            Debug.Log("플레이어 쓰레기통 벗어남");
        }
    }

    void Update()
    {
        // 플레이어가 내부에 있고, PlayerMove 참조가 유효하며,
        // 플레이어가 무언가를 들고 있고(isHolding), 스페이스바를 눌렀을 때
        if (isPlayerInside && playerMove != null && playerMove.isHolding && Input.GetKeyDown(KeyCode.Space))
        {
            // 플레이어가 들고 있는 오브젝트 가져오기
            GameObject heldTrash = playerMove.heldObject;

            // 플레이어가 오브젝트를 내려놓도록 함 (PlayerMove의 함수 호출)
            playerMove.DropHeldObject(); // DropHeldObject는 heldObject를 null로 만듦

            // 내려놓은 쓰레기 오브젝트 파괴
            if (heldTrash != null) // DropHeldObject 호출 후에도 참조가 남아있는지 확인 (안전성)
            {
                Destroy(heldTrash);
                Debug.Log($"쓰레기통: '{heldTrash.name}' 오브젝트를 버렸습니다 (파괴).");
                // 버린 후 추가 효과 (예: 점수, 소리) 가능
            } else {
                 Debug.LogWarning("쓰레기통: 플레이어가 무언가를 들고 있었지만, 버릴 오브젝트 참조가 null입니다.");
            }

        }
    }
}
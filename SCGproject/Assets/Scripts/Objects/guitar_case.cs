using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class guitar_case : MonoBehaviour, IInteractable
{
    [Tooltip("플레이어가 근처일 때 Space 안내를 띄울지 여부")]
    public bool showSpaceHint = true;
    
    // (선택 사항) key_info_ch2 스크립트를 연결하면 상호작용 UI를 띄울 수 있습니다.
    // public key_info_ch2 keyInfoCh2; 

    private bool isPlayerNear = false;
    private bool firstInteracted = false; // 한 번만 상호작용하도록 체크
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = true;
        
        // (선택 사항) keyInfoCh2가 연결되어 있다면
        // if (keyInfoCh2 != null && !firstInteracted)
        // {
        //     keyInfoCh2.isObject = true;
        // }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = false;

        // (선택 사항) keyInfoCh2가 연결되어 있다면
        // if (keyInfoCh2 != null)
        // {
        //     keyInfoCh2.isObject = false;
        // }
    }

    void Update()
    {
        // 플레이어가 근처에 없거나 이미 상호작용했다면 아무것도 안 함
        if (!isPlayerNear || firstInteracted) return;
        // 입력 처리는 PlayerMove가 담당합니다. 이 스크립트는 범위 감지와 상태만 처리.
    }

    // PlayerMove의 상호작용 흐름으로 동작하도록 인터페이스 구현
    public void Interact(PlayerMove player)
    {
        if (firstInteracted) return;

        firstInteracted = true;

        // Chapter2Manager에 기타 케이스를 찾았다고 알림
        Chapter2Manager.Instance?.OnGuitarCaseFound();

        // 상호작용 UI 숨기기
        // if (keyInfoCh2 != null)
        //     keyInfoCh2.isObject = false;

        // 필요하면 스크립트 비활성화
        this.enabled = false;
    }
}
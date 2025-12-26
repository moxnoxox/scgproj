using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class String : MonoBehaviour
{
    [Tooltip("플레이어가 근처일 때 Space 안내를 띄울지 여부")]
    public bool showSpaceHint = true;
    
    // (선택 사항) key_info_ch2 스크립트를 연결하면 상호작용 UI를 띄울 수 있습니다.
    // public key_info_ch2 keyInfoCh2; 

    private bool isPlayerNear = false;
    private bool firstInteracted = false;
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 플래그를 true로 설정해 중복 상호작용 방지
            firstInteracted = true;
            
            // Chapter2Manager에 기타를 찾았다고 알림
            Chapter2Manager.Instance?.OnStringFound();
            partsUI.instance.OnStringUIEnable();
            Destroy(gameObject);
            // (선택 사항) 상호작용 UI 숨기기
            // if (keyInfoCh2 != null)
            // {
            //     keyInfoCh2.isObject = false;
            // }

            // (선택 사항) 첫 발견 대사 출력 (JSON에 "guitarCase_first" 키 추가 필요)
            // Chapter2Manager.Instance?.StartCoroutine("ShowMono", "guitarCase_first", 2f);
            
            // (선택 사항) 상호작용 후 이 스크립트를 비활성화
            // this.enabled = false; 
        }
    }
}
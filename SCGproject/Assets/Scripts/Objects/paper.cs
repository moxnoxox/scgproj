using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// IInteractable 인터페이스를 구현하여 Player와의 상호작용을 표준화합니다.
public class paper : MonoBehaviour, IInteractable
{
    public GameObject player;
    public player_power playerPower;
    public key_info keyInfo;
    public Image inside;
    public TextMeshProUGUI insideText;

    private bool isPlayerNear = false;
    // 'canInteract'는 GameManager가 상호작용을 허용할 때 true가 됩니다.
    private bool canInteract = false; 
    private bool hasBeenInteracted = false;

    void Start()
    {
        playerPower = player.GetComponent<player_power>();
        inside.enabled = false;
        insideText.enabled = false;
    }
    
    // GameManager가 특정 시점에 호출하여 상호작용을 활성화합니다.
    public void EnableInteraction()
    {
        canInteract = true;
        Debug.Log("Paper interaction has been enabled.");
    }

    void Update()
    {
        if (!canInteract || hasBeenInteracted || player == null) return;

        float xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        bool currentlyNear = xdiff < 1f;

        if (currentlyNear != isPlayerNear)
        {
            isPlayerNear = currentlyNear;
            if(keyInfo != null) keyInfo.isObject = isPlayerNear;
        }
    }

    // PlayerMove에서 Space 키를 누르면 이 함수가 호출됩니다.
    // IInteractable 인터페이스 규약에 따라 PlayerMove를 전달받습니다.
    public void Interact(PlayerMove playerMove)
    {
        if (playerMove == null) return;

        // 거리 기반 근접 체크를 다시 한 번 수행(안전성)
        float xdiff = Mathf.Abs(this.transform.position.x - playerMove.transform.position.x);
        if (xdiff >= 1f) return;

        if (!canInteract || hasBeenInteracted) return;

        Debug.Log("Player interacted with the paper.");
        hasBeenInteracted = true;
        if (keyInfo != null) keyInfo.isObject = false;

        // GameManager에 종이를 읽었음을 직접 알립니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPaperRead();
        }

        StartCoroutine(DisplayInside());
    }

    IEnumerator DisplayInside()
    {
        inside.enabled = true;
        insideText.enabled = true;
        yield return new WaitForSeconds(5f);
        inside.enabled = false;
        insideText.enabled = false;
        
        playerPower.DecreasePower(40);
        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class computer_ch2 : MonoBehaviour, IInteractable
{
    public GameObject player;
    private float xdiff;
    public SpriteRenderer spriteRenderer;
    public Sprite computerOn;
    public Sprite computerOff;
    public player_power playerPower;
    public key_info_ch2 keyInfoCh2;
    private Collider2D col;
    public bool isFirstInteract = false;

    public CanvasGroup canvasGroup;

    void Start()
    {
        if (keyInfoCh2 != null) keyInfoCh2.isObject = false;
        if (player != null) playerPower = player.GetComponent<player_power>();
        col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        if (xdiff < 1f)
        {
            spriteRenderer.sprite = computerOn;
            if (keyInfoCh2 != null) keyInfoCh2.isObject = true;
        }
        else if(xdiff < 1.1f)
        {
            spriteRenderer.sprite = computerOff;
            if (keyInfoCh2 != null) keyInfoCh2.isObject = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (keyInfoCh2 != null) keyInfoCh2.isObject = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (keyInfoCh2 != null) keyInfoCh2.isObject = false;
    }

    // PlayerMove에서 상호작용 호출 시 실행
    public void Interact(PlayerMove playerMove)
    {
        if (playerMove == null) return;
        float dist = Mathf.Abs(this.transform.position.x - playerMove.transform.position.x);
        if(dist < 1f)
        {
            if (!isFirstInteract)
            {
                Chapter2Manager.Instance?.OnLaptopOpened();
                isFirstInteract = true;
            }
            else
            {
                ComputerController.Instance.StartComputer();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class computer : MonoBehaviour, IInteractable
{
    public GameObject player;
    public SpriteRenderer spriteRenderer;
    public Sprite computerOn;
    public Sprite computerOff;
    public player_power playerPower;
    public key_info keyInfo;
    public Image home;
    public bool isHomeClosed = false;

    private bool isPlayerNear = false;
    private bool hasBeenInteracted = false;

    private bool isPlayerNear = false;
    private bool hasBeenInteracted = false;

    void Start()
    {
        if (home != null) home.enabled = false;
        // Player reference is now passed via Interact method, but we still need playerPower.
        if (player != null) playerPower = player.GetComponent<player_power>();
    }

    void Update()
    {
        if (player == null) return;

        float xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        bool currentlyNear = xdiff < 1f;

        if (currentlyNear != isPlayerNear)
        {
            isPlayerNear = currentlyNear;
            spriteRenderer.sprite = isPlayerNear ? computerOn : computerOff;
            if (keyInfo != null) keyInfo.isObject = isPlayerNear && !hasBeenInteracted;
        }
    }

    public void Interact(PlayerMove player)
    {
        if (!isPlayerNear || hasBeenInteracted) return;

        hasBeenInteracted = true;
        if (keyInfo != null) keyInfo.isObject = false;

        StartCoroutine(ShowHomeImage());
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onComputerChecked();
        }
    }

    IEnumerator ShowHomeImage()
    {
        if (home != null) home.enabled = true;
        MonologueManager.Instance.ShowMonologuesSequentially(new List<string> { "파일 정리하기 너무 귀찮아..." }, 3f);
        yield return new WaitForSeconds(3.0f);
        if (home != null) home.enabled = false;
        isHomeClosed = true;
        if (playerPower != null) playerPower.DecreasePower(10);
    }
}


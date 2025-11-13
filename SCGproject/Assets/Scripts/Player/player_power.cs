using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class player_power : MonoBehaviour
{
    public int maxPower = 100;
    public int currentPower;
    public Image powerSlider;
    public PlayerMove playerMove;
    public bool noPower = false;
    private bool triggeredOnce = false; // 자동리턴 중복 방지

    void Start()
    {
        currentPower = 10;
        UpdatePowerUI();
    }

    void Update()
    {
        noPower = currentPower <= 0;

        if (GameManager.Instance != null)
        {
            string state = GameManager.Instance.GetCurrentScenarioState();
            if (state == "PaperReaction" || state == "MirrorScene") return;
        }

        if (noPower && !triggeredOnce)
        {
            triggeredOnce = true;
            if (playerMove != null && !playerMove.autoMoveActive)
            {
                Debug.Log("★ 파워0 자동리턴 호출");
                playerMove.StartCoroutine(playerMove.AutoReturnToBed(false));
            }
        }
    }

    void UpdatePowerUI()
    {
        if (powerSlider != null)
            powerSlider.fillAmount = Mathf.Clamp01((float)currentPower / maxPower);
    }

    public void DecreasePower(int amount)
    {
        currentPower = Mathf.Max(currentPower - amount, 0);
        UpdatePowerUI();
    }

    public void IncreasePower(int amount)
    {
        currentPower = Mathf.Min(currentPower + amount, maxPower);
        if (currentPower > 0) triggeredOnce = false;
        UpdatePowerUI();
    }
}

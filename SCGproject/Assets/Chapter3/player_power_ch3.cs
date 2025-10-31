using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class player_power_ch3 : MonoBehaviour
{
    public int maxPower = 100;
    public int currentPower;
    public Image powerSlider;
    public PlayerMove_ch3 playerMove_ch3;
    public bool noPower = false;
    void Start()
    {
        currentPower = 80;
        UpdatePowerUI();
    }

    void Update()
    {
        noPower = currentPower <= 0;
        if(noPower) GameManager.Instance.autoMove = true;
        UpdatePowerUI();

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
        UpdatePowerUI();
    }
}


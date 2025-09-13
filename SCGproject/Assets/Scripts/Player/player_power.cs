using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_power : MonoBehaviour
{
    public int maxPower = 100;
    public int currentPower;
    public UnityEngine.UI.Slider powerSlider;
    public PlayerMove playerMove;
    void Start()
    {
        currentPower = maxPower;
        powerSlider.maxValue = 10;
        powerSlider.value = 10;
    }
    void Update()
    {
        if(currentPower <= 0) {
            playerMove.noPower = true;
        }
        else {
            playerMove.noPower = false;
        }
        powerSlider.value = currentPower/10;
    }
    public void DecreasePower(int amount)
    {
        currentPower -= amount;
    }
    public void IncreasePower(int amount)
    {
        currentPower += amount;
        if (currentPower > maxPower)
        {
            currentPower = maxPower;
        }
    }
}


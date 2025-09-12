using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_power : MonoBehaviour
{
    public int maxPower = 100;
    public int powerDecreaseRate = 1;
    public int currentPower;
    public UnityEngine.UI.Slider powerSlider;
    public PlayerMove playerMove;
    void Start()
    {
        currentPower = maxPower;
        powerSlider.maxValue = maxPower;
        powerSlider.value = currentPower;
    }
    void Update()
    {
        powerSlider.value = (float)currentPower/maxPower;
        if(currentPower <= 0) {
            playerMove.noPower = true;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestManagerCh2 : MonoBehaviour
{
    public TextMeshProUGUI quest1;
    public TextMeshProUGUI quest2;
    public TextMeshProUGUI quest3;
    public TextMeshProUGUI check1;
    public TextMeshProUGUI check2;
    public TextMeshProUGUI check3;
    void Start()
    {
        check1.enabled = false;
        check2.enabled = false;
        check3.enabled = false;
        quest1.text = "???";
        quest2.text = "???";
        quest3.text = "???";
    }
    void Update()
    {
        
    }

    public void UpdateGuitarQuest()
    {
        quest1.text = "기타 찾기";
        check1.enabled = false;
    }
    public void CompleteGuitarQuest()
    {
        check1.enabled = true;
    }
    public void UpdateComputerQuest()
    {
        quest2.text = "컴퓨터로 USB 내용 확인하기";
        check2.enabled = false;
    }
    public void CompleteComputerQuest()
    {
        check2.enabled = true;
    }
    public void GuitarPartsFind()
    {
        quest3.text = "기타 부품 찾기";
        check3.enabled = false;
    }
}

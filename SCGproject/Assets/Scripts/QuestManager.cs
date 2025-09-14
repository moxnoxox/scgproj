using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
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
    }
    void Update()
    {
        if (GameManager.Instance.getreplCount() >= 2)
        {
            check1.enabled = true;
        }
        if (GameManager.Instance.getComputerChecked())
        {
            check2.enabled = true;
        }
        if (GameManager.Instance.getreplCount() >= 2 && GameManager.Instance.getComputerChecked())
        {
            quest3.text = "거울 확인하기";
        }
        if (GameManager.Instance.getMirrorChecked())
        {
            check3.enabled = true;
        }
    }
}

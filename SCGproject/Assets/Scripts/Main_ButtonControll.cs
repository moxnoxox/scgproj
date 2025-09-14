using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_ButtonControll : MonoBehaviour
{
    public void onClick_Start()
    {
        SceneController.Loadscene("SampleScene");
    }
}

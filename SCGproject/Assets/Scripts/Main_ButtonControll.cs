using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main_ButtonControll : MonoBehaviour
{
    public SceneController sceneController;
    public void onClick_Start()
    {
        sceneController.loadScene("SampleScene");
    }
}

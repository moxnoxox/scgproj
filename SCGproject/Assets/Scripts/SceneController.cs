using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class SceneController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI loadingText;
    static string nextscene;
    public static void Loadscene(string scenename)
    {
        nextscene = scenename;
        SceneManager.LoadScene("Loading");
    }

    void Start()
    {
        StartCoroutine(Loadsceneprosess());
    }
    private float remainloadtime = 3.0f;
    private float timer;
    IEnumerator Loadsceneprosess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextscene);
        op.allowSceneActivation = false;
        while(!op.isDone)
        {
            yield return null;
            if(op.progress < 0.9f)
            {
                loadingText.text = ((int)(op.progress*100)).ToString()+"%";
            }
            else if(op.progress < 1)
            {
                timer += Time.deltaTime;
                loadingText.text = ((int) Mathf.Lerp(90f, 100f, timer / remainloadtime)).ToString()+"%";
            }
            if(timer >= remainloadtime)
            {
                op.allowSceneActivation = true;
                yield break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class SceneController : MonoBehaviour
{
    [SerializeField] Slider loadingbar;
    [SerializeField] TextMeshProUGUI loadprocess;
    public static string process;
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
            if(loadingbar.value < 0.9f)
            {
                loadingbar.value = op.progress;
            }
            else if(loadingbar.value < 1)
            {
                timer += Time.deltaTime;
                loadingbar.value = Mathf.Lerp(0.9f, 1f, timer / remainloadtime);
            }
            else
            {
                op.allowSceneActivation = true;
                yield break;
            }
            loadprocess.text = Convert.ToString(Mathf.RoundToInt(loadingbar.value * 100)) + "%";
        }
    }
}

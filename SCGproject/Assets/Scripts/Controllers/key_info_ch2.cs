using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class key_info_ch2 : MonoBehaviour
{
    public bool isObject;
    public Image space;
    private UnityEngine.Vector3 pos;

    void Start()
    {
        space.enabled = false;
    }


    void Update()
    {
        if (isObject)
        {
            space.enabled = true;
        }
        else
        {
            space.enabled = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class key_info : MonoBehaviour
{
    public bool isBed;
    public bool isObject;
    public bool is_starting;
    public bool is_click;
    public Image ad;
    public Image click;
    public Image S;
    public Image space;
    public CameraController camcon;
    private UnityEngine.Vector3 pos;

    void Start()
    {
        S.enabled = false;
        ad.enabled = false;
        click.enabled = false;
        space.enabled = false;
    }


    void Update()
    {
        if (isBed)
        {
            S.enabled = true;
        }
        else
        {
            S.enabled = false;
        }
        if (isObject)
        {
            space.enabled = true;
        }
        else
        {
            space.enabled = false;
        }
        if (is_starting)
        {
            ad.enabled = true;
        }
        else
        {
            ad.enabled = false;
        }
        if (is_click)
        {
            click.enabled = true;
        }
        else
        {
            click.enabled = false;
        }
        
    }
}

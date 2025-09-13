using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class key_info : MonoBehaviour
{
    public bool isBed;
    public bool isObject;
    public bool is_starting;
    public Image ad;
    public Image click;
    public Image S;
    public Image space;
    // Start is called before the first frame update
    void Start()
    {
        S.enabled = false;
        ad.enabled = false;
        click.enabled = false;
        space.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isBed) {
            S.enabled = true;
        }
        else {
            S.enabled = false;
        }
        if(isObject) {
            space.enabled = true;
        }
        else {
            space.enabled = false;
        }
        if(is_starting) {
            ad.enabled = true;
        }
        else {
            ad.enabled = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testphoneanime : MonoBehaviour
{
    public PhonePanelController ppc;
    public Animator anim;
    void Update()
    {
        if (ppc.IsOpen)
        {
            anim.SetBool("isPhone", true);
        }
        else
        {
            anim.SetBool("isPhone", false);
        }
    }
}

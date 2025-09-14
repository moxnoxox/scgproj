using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class blanket_controller : MonoBehaviour
{
    public GameObject player;
    public Animator player_anim;

    void Update()
    {
        float xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x - 1.4f);
        if (xdiff < 0.5f)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (player_anim.GetBool("isSleep") == true)
                {
                    player_anim.SetBool("isSleep", false);

                }
                else
                {
                    player_anim.SetBool("isSleep", true);
                    player_anim.SetBool("isWalking", false);
                    GameManager.Instance.onBedding();
                }
            }
        }
    }
}

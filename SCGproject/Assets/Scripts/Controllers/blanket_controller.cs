using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class blanket_controller : MonoBehaviour
{
    public GameObject player;
    public Animator player_anim;
    public PlayerMove playermove;
    void Start()
    {
        playermove = player.GetComponent<PlayerMove>();
    }
    void Update()
    {
        if (playermove.canInput == false) return;
        float xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x - 1.4f);
        if (xdiff < 0.5f)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (player_anim.GetBool("isSleep") == true)
                {
                    player_anim.SetBool("isSleep", false);
                    StartCoroutine(motionWait());
                }
                else
                {
                    player_anim.SetBool("isSleep", true);
                    player_anim.SetBool("isWalking", false);
                    GameManager.Instance.onBedding();
                    StartCoroutine(motionWait());
                }
            }
        }
    }

    IEnumerator motionWait()
    {
        if (playermove.movable)
        {
            playermove.movable = false;
            yield return new WaitForSeconds(1.5f);
            playermove.movable = true;
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
    }
}

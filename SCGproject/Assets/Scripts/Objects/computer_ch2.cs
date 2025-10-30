using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class computer_ch2 : MonoBehaviour
{
    public GameObject player;
    private float xdiff;
    public SpriteRenderer spriteRenderer;
    public Sprite computerOn;
    public Sprite computerOff;
    public player_power playerPower;
    public key_info_ch2 keyInfoCh2;

    void Start()
    {
        keyInfoCh2.isObject = false;
        playerPower = player.GetComponent<player_power>();
    }

    // Update is called once per frame
    void Update()
    {
        xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        if (xdiff < 1f)
        {
            spriteRenderer.sprite = computerOn;
            keyInfoCh2.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Chapter2Manager.Instance.OnLaptopOpened();
            }
        }
        else if(xdiff < 1.1f)
        {
            spriteRenderer.sprite = computerOff;
            keyInfoCh2.isObject = false;
        }
    }
}

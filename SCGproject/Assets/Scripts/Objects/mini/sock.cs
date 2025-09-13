using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sock : MonoBehaviour
{
    public GameObject player;
    private float xdiff;
    public player_power playerPower;
    public key_info keyInfo;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        playerPower = player.GetComponent<player_power>();
    }

    // Update is called once per frame
    void Update()
    {
        xdiff = Mathf.Abs(transform.position.x - player.transform.position.x);
        if (xdiff < 1f)
        {
            keyInfo.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerPower.IncreasePower(5);
            }
        }
        else if(xdiff < 1.01f)
        {
            keyInfo.isObject = false;
        }
    }
}

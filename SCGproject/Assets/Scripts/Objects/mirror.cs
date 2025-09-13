using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mirror : MonoBehaviour
{
    public GameObject player;
    private float xdiff;
    public player_power playerPower;
    public key_info keyInfo;
    // Start is called before the first frame update
    void Start()
    {
        playerPower = player.GetComponent<player_power>();
    }

    // Update is called once per frame
    void Update()
    {
        xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        if (xdiff < 1f)
        {
            keyInfo.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerPower.DecreasePower(10);
            }
        }
        else{
            keyInfo.isObject = false;
        }
    }
}

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
        if(GameManager.Instance.getreplCount() < 2) return;
        if(GameManager.Instance.getComputerChecked() == false) return;
        xdiff = Mathf.Abs(transform.position.x - player.transform.position.x);
        if (xdiff < 1f)
        {
            keyInfo.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerPower.DecreasePower(100);
                GameManager.Instance.onMirrorChecked();
            }
        }
        else if(xdiff < 1.1f)
        {
            keyInfo.isObject = false;
        }
    }
}

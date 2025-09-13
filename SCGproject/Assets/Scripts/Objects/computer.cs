using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class computer : MonoBehaviour
{
    public GameObject player;
    private float xdiff;
    public SpriteRenderer spriteRenderer;
    public Sprite computerOn;
    public Sprite computerOff;
    public player_power playerPower;
    public key_info keyInfo;
    public Image home;

    void Start()
    {
        home.enabled = false;
        playerPower = player.GetComponent<player_power>();
    }

    // Update is called once per frame
    void Update()
    {
        xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        if (xdiff < 1f)
        {
            spriteRenderer.sprite = computerOn;
            keyInfo.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(ShowHomeImage());
            }
        }
        else
        {
            spriteRenderer.sprite = computerOff;
            keyInfo.isObject = false;
        }
    }

    IEnumerator ShowHomeImage()
    {
        home.enabled = true;
        yield return new WaitForSeconds(2.0f);
        home.enabled = false;
        playerPower.DecreasePower(10);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class paper : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    private float xdiff;
    public player_power playerPower;
    public key_info keyInfo;
    public UnityEngine.UI.Image inside;
    public TextMeshProUGUI insideText;
    // Start is called before the first frame update
    void Start()
    {
        playerPower = player.GetComponent<player_power>();
        inside.enabled = false;
        insideText.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        xdiff = Mathf.Abs(this.transform.position.x - player.transform.position.x);
        if (xdiff < 1f && GameManager.Instance.canInput)
        {
            GameManager.Instance.OnObjectActivated();
            
            keyInfo.is_starting = false;
            keyInfo.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(DisplayInside());
            }
        }
    }
    IEnumerator DisplayInside()
    {
        while(GameManager.Instance.papaermonologueDone == false)
        {
            yield return null;
        }
        inside.enabled = true;
        insideText.enabled = true;
        GameManager.Instance.onPaperOpened();
        yield return new WaitForSeconds(5f);
        inside.enabled = false;
        insideText.enabled = false;
        
        playerPower.DecreasePower(40);
        StartCoroutine(WaitandDisable());
    }
    IEnumerator WaitandDisable()
    {
        yield return new WaitForSeconds(0.5f);
        keyInfo.isObject = false;
        Destroy(this.gameObject);
    }
}

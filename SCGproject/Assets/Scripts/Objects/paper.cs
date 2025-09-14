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
        if (xdiff < 1f)
        {
            GameManager.Instance.OnObjectActivated();
            keyInfo.isObject = true;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(DisplayInside());
                GameManager.Instance.onPaperOpened();
                Debug.Log("종이쪼가리 열었음");
                playerPower.DecreasePower(40);
            }
        }
    }
    IEnumerator DisplayInside()
    {
        inside.enabled = true;
        insideText.enabled = true;
        yield return new WaitForSeconds(4f);
        inside.enabled = false;
        insideText.enabled = false;
        StartCoroutine(WaitandDisable());
    }
    IEnumerator WaitandDisable()
    {
        yield return new WaitForSeconds(1f);
        keyInfo.isObject = false;
        Destroy(this.gameObject);
    }
}

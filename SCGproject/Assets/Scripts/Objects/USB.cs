using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class USB : MonoBehaviour
{
    [Tooltip("이 USB의 인덱스. 1, 2, 3 중 하나")]
    public int usbIndex = 1;

    [Tooltip("플레이어가 근처일 때 Space 안내를 띄울지 여부")]
    public bool showSpaceHint = true;

    private bool isPlayerNear = false;
    private bool firstInteracted = false;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNear = true;
        Chapter2Manager.Instance?.OnPlayerNearUSB();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNear = false;
    }

    void Update()
    {
        if (!isPlayerNear) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!firstInteracted)
            {
                firstInteracted = true;
                Chapter2Manager.Instance?.OnUSBInteracted(usbIndex);
                Chapter2Manager.Instance?.PlayUSBFirstDialogue(usbIndex);
                switch (usbIndex)
                {
                    case 1:
                        usbUI.usb1Active();
                        break;
                    case 2:
                        usbUI.usb2Active();
                        break;
                    case 3:
                        usbUI.usb3Active();
                        break;
                }
                Destroy(this.gameObject);
            }
            else
            {
                Chapter2Manager.Instance?.PlayUSBRereadHint();
            }
        }
    }
}


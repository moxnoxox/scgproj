using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private bool isPlayerInside = false;
    private PlayerMove playerMove;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerMove = other.GetComponent<PlayerMove>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerMove = null;
        }
    }

    void Update()
    {
        if (isPlayerInside && playerMove != null && playerMove.isHolding && Input.GetKeyDown(KeyCode.Space))
        {
            GameObject trash = playerMove.heldObject;

            // 내려놓기 처리
            playerMove.DropHeldObject();

            // 파괴
            Destroy(trash);
            Debug.Log("쓰레기통에 버림: 오브젝트 삭제됨");
        }
    }
}

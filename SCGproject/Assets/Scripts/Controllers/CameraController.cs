using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    float __zPos = -10f;

    private void Awake()
    {
        if (gameObject.CompareTag("MainCamera"))
        {
            DontDestroyOnLoad(gameObject); // Player 오브젝트가 파괴되지 않도록 설정
        }
    }
    void LateUpdate()
    {
        Vector3 newPosition = player.transform.position;
        newPosition.z = __zPos;
        newPosition.y = newPosition.y + 1.7f;
        transform.position = newPosition;
    }
}

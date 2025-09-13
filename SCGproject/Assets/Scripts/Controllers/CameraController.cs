using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    float __zPos = -10f;
    public bool gameStart;
    private float zoomOutSize = 5f; // 줌아웃 시 카메라 사이즈
    private float normalSize = 3f;  // 기본 카메라 사이즈
    private float zoomSpeed = 1f;   // 줌아웃 속도
    private float moveSpeed = 1f;   // 카메라 이동 속도

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = normalSize;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = player.transform.position;
        targetPosition.z = __zPos;
        if (gameStart)
            targetPosition.y += 1.7f;
        else {
            targetPosition.x = 0.4f;
        }

        // 부드럽게 카메라 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

        // 줌아웃 처리
        if (cam != null)
        {
            float targetSize = gameStart ? zoomOutSize : normalSize;
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
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

    private float smoothTimer = 0f;
    private float smoothDuration = 2.0f; // 부드러운 이동 지속 시간
    private float leftMaxX = -5.73f;
    private float rightMaxX = 5.8f;
    private UnityEngine.Vector3 varPosition;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = normalSize;
    }

    void LateUpdate()
    {
        if (gameStart)
            smoothTimer += Time.deltaTime;
        if (smoothTimer < smoothDuration)
        {
            Smooth();
        }
        else if (GameManager.Instance.BedDepressed)
        {
            ZoomIn();
        }
        else
        {
            Fast();
        }
    }

    void Smooth()
    {
        UnityEngine.Vector3 targetPosition = player.transform.position;
        targetPosition.z = __zPos;
        if (gameStart)
            targetPosition.y += 1.9f;
        else
        {
            targetPosition.x = 0.4f;
        }

        // 부드럽게 카메라 이동
        transform.position = UnityEngine.Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);

        // 줌아웃 처리
        if (cam != null)
        {
            float targetSize = gameStart ? zoomOutSize : normalSize;
            cam.orthographicSize = MoveTowards(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
        }
    }
    void ZoomIn()// smooth 반대과정
    {
        UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(0.4f, player.transform.position.y - 0.3f, __zPos);

        transform.position = UnityEngine.Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed * 0.7f);
        if (cam != null)
        {
            float targetSize = 3f;
            cam.orthographicSize = MoveTowards(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
        }
    }

    void Fast()
    {
        UnityEngine.Vector3 targetPosition = player.transform.position;
        targetPosition.z = __zPos;
        targetPosition.y += 1.7f;
        if (targetPosition.x < leftMaxX)
        {
            varPosition.x = targetPosition.x - leftMaxX;
            targetPosition.x = leftMaxX;
        }
        else if (targetPosition.x > rightMaxX)
        {
            varPosition.x = targetPosition.x - rightMaxX;
            targetPosition.x = rightMaxX;
        }
        else
        {
            varPosition.x = 0;
        }
        transform.position = targetPosition;
    }

    float MoveTowards(float current, float target, float maxDelta)
    {
        if (Mathf.Abs(target - current) <= maxDelta)
            return target;
        return current + Mathf.Sign(target - current) * maxDelta;
    }

    public UnityEngine.Vector3 getVarpos()
    {
        return varPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class PaperHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    //paperpuzzlecontroller에서 startpuzzle 실행 시에만 실행
    //todo: paperpuzllecontroller의 pieces를 가져와서 드래그로 위위치옮기기, 더블클릭으로 회전(90도))
    //todo: 퍼즐 드래그앤드롭 시 퍼즐 조각이 특정 범위 내에 들어오면 자동으로 자리 지정
    //todo: 퍼즐은 unityengine.ui.image, 9개, 위치는 3x3 격자, 퍼즐 조각은 100x100
    public Vector3 originalPosition;
    public Transform[] correctPosition;
    public int pieceIndex; // 퍼즐 조각 인덱스 (0~8)
    public bool isCorrectPosition = false;
    
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f;
    
    // 드래그는 100x100사이즈
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!PaperpuzzleController.Instance.isPuzzleActive) return;
        originalPosition = transform.position;

        transform.SetAsLastSibling(); // 드래그 시작 시 가장 위로 이동

    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!PaperpuzzleController.Instance.isPuzzleActive) return;
        transform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!PaperpuzzleController.Instance.isPuzzleActive) return;
        float distance;
        
        for (int i = 0; i < correctPosition.Length; i++)
        {
            distance = Vector3.Distance(transform.position, correctPosition[i].position);
            if (distance < 50f)
            {
                // 위치에 스냅(부드럽게)
                StartCoroutine(SnapToPosition(correctPosition[i].position));
                isCorrectPosition = (i == pieceIndex && transform.rotation == correctPosition[i].rotation); // 올바른 위치인지 확인
                break;
            }
        }
    }
    void Start()
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                correctPosition[(i + 1) * 3 + (j + 1)].rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        originalPosition = transform.position;
        isCorrectPosition = false;
    }

    // 클릭 이벤트 처리 (더블클릭으로 회전)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!PaperpuzzleController.Instance.isPuzzleActive) return;
        
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            // 더블클릭 감지시 회전
            StartCoroutine(RotateCoroutine());
        }
        lastClickTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(!PaperpuzzleController.Instance.isPuzzleActive) return;
    }
    // 퍼즐 조각 위치 스냅(부드럽게)
    IEnumerator SnapToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    // 퍼즐 조각 90도 회전(부드럽게)
        IEnumerator RotateCoroutine()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 0, -90f);
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
    }
}

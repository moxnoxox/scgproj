using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackInputManager : MonoBehaviour
{
    private static Stack<Action> backHandlers = new Stack<Action>();
    
    // esc 키 입력 감지
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && backHandlers.Count > 0)
        {
            backHandlers.Peek()?.Invoke(); 
            // 가장 위에 있는 핸들러 실행
        }
    }

    // 핸들러 등록
    public static void Register(Action handler)
    {
        if (!backHandlers.Contains(handler))
            backHandlers.Push(handler);
    }

    // 핸들러 제거
    public static void Unregister(Action handler)
    {
        if (backHandlers.Count == 0)
            return;

        if (backHandlers.Peek() == handler)
            backHandlers.Pop();
    }

    // 현재 핸들러가 있는지 확인
    public static bool HasHandler => backHandlers.Count > 0;
}


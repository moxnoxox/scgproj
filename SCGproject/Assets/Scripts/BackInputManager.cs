using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Required for .Last()

public class BackInputManager : MonoBehaviour
{
    // Using a List instead of a Stack makes unregistering more robust.
    // It allows removing a handler even if it's not the last one added.
    private static List<Action> backHandlers = new List<Action>();
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && backHandlers.Count > 0)
        {
            // Invoke the last handler in the list.
            backHandlers.Last()?.Invoke(); 
        }
    }
    
    public static void TriggerBack()
    {
        if (backHandlers.Count > 0)
        {
            backHandlers.Last()?.Invoke();
        }
    }

    public static void Register(Action handler)
    {
        if (!backHandlers.Contains(handler))
        {
            backHandlers.Add(handler);
            Debug.Log($"[BackInput] Register: {handler.Method}, StackCount={backHandlers.Count}");
        }
    }

    // Unregistering from a List is safer because it removes the specific handler
    // regardless of its position.
    public static void Unregister(Action handler)
    {
        if (backHandlers.Remove(handler))
        {
            Debug.Log($"[BackInput] Unregister: {handler.Method}, StackCount={backHandlers.Count}");
        }
    }

    public static bool HasHandler => backHandlers.Count > 0;

    public static void ClearAll()
    {
        backHandlers.Clear();
    }
}


using UnityEngine;
using System.Collections.Generic;

public class PhoneDataManager : MonoBehaviour
{
    public static PhoneDataManager Instance;

    public List<ChatRoom> chatRooms = new List<ChatRoom>();

    void Awake() {
        Debug.Log("PhoneDataManager Awake()");
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}

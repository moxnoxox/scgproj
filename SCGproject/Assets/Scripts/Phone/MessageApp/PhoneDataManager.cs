using UnityEngine;
using System.Collections.Generic;

public class PhoneDataManager : MonoBehaviour
{
    public static PhoneDataManager Instance;

    public List<ChatRoom> chatRooms = new List<ChatRoom>();
    public Dictionary<string, Sprite> roomSpriteMap = new Dictionary<string, Sprite>();


    void Awake() {
        Debug.Log("PhoneDataManager Awake()");
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void MoveRoomToTop(ChatRoom room)
    {
        if (room == null) return;
        if (chatRooms.Remove(room))
            chatRooms.Insert(0, room);
    }

}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChatRoomButtonManager : MonoBehaviour
{
    [Header("실제 채팅방 버튼들")]
    public List<Button> roomButtons;   // JSON 기반 방 버튼들

    [Header("더미 채팅방 버튼들")]
    public List<Button> dummyButtons;  // JSON과 무관한 더미 버튼들

    [Header("데이터 로더")]
    public ChatRoomLoader loader; // JSON 받아옴

    [Header("앱 매니저")]
    public ChatAppManager chatAppManager; // 채팅방 열 때 호출

    private List<ChatRoom> loadedRooms;

    private void Start()
    {
        loadedRooms = loader.loadedRooms;

        // 실제 방 버튼 연결
        for (int i = 0; i < roomButtons.Count; i++)
        {
            int index = i; // 람다 캡처 문제 방지
            roomButtons[i].onClick.AddListener(() =>
            {
                if (index < loadedRooms.Count)
                {
                    chatAppManager.OpenChatRoomWithData(loadedRooms[index]);
                }
                else
                {
                    Debug.LogWarning($"잘못된 인덱스 {index}");
                }
            });
        }

        // 더미 방 버튼 연결
        foreach (var dummy in dummyButtons)
        {
            dummy.onClick.AddListener(() =>
            {
                MonologueManager.Instance.ShowMonologuesSequentially(
                    new List<string> { "여긴 별 볼일 없어..." },
                    3f, 0f // 3초 동안 보여주고, 0초 쉬고
                );
            });
        }
    }
}

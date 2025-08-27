using UnityEngine;

public class ChatAppManager : MonoBehaviour
{
    [Header("패널들")]
    public GameObject chatListPanel;       // 채팅방 리스트 화면
    public GameObject chatRoomPrefab;      // 채팅방 프리팹 (ChatRoomPanel)
    public Transform ChatAppPanel;         // ChatRoomPanel이 들어갈 부모

    private GameObject currentRoomPanel;   // 현재 열려있는 채팅방 인스턴스
    private ChatManager chatManager;       // 현재 방의 ChatManager

    private void OnEnable()
    {
        // 시작할 때 리스트 열기
        OpenChatList();
    }

    private void OnDisable()
    {
        // 방 닫기 핸들러가 남아있으면 정리
        BackInputManager.Unregister(OnBackPressedFromRoom);
    }

    public void OpenChatList()
    {
        chatListPanel.SetActive(true);

        if (currentRoomPanel != null)
        {
            Destroy(currentRoomPanel);
            currentRoomPanel = null;
            chatManager = null;
        }
    }

    // 방 열기
    public void OpenChatRoomWithData(ChatRoom roomData)
    {
        chatListPanel.SetActive(false);

        if (currentRoomPanel != null)
            Destroy(currentRoomPanel);

        currentRoomPanel = Instantiate(chatRoomPrefab, ChatAppPanel);
        chatManager = currentRoomPanel.GetComponent<ChatManager>();

        if (chatManager != null)
            chatManager.SetCurrentRoom(roomData);

        currentRoomPanel.SetActive(true);

        // 방 상태 ESC 처리 (방 닫고 리스트로)
        BackInputManager.Register(OnBackPressedFromRoom);
    }

    // 버튼- 리스트 돌아가기 (추후 추가 예정)
    public void BackToList()
    {
        CloseRoom();
    }

    // ESC: 채팅방 닫고 리스트로
    private void OnBackPressedFromRoom()
    {
        CloseRoom();
        BackInputManager.Unregister(OnBackPressedFromRoom); // 자기 Pop
    }

    private void CloseRoom()
    {
        if (currentRoomPanel != null)
        {
            Destroy(currentRoomPanel);
            currentRoomPanel = null;
            chatManager = null;
            chatListPanel.SetActive(true);
        }
    }
}

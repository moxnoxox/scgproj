using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter2Manager : MonoBehaviour
{
    public static Chapter2Manager Instance;
    public bool autoMove = false;

    // 플레이어, UI, 기타 등 연결될 오브젝트들
    public PlayerMove playerMove;
    public player_power playerPower;
    public UnityEngine.UI.Image notification;

    private enum ScenarioState
    {
        StartContact,
        ShowPhoto,
        DecideToSearch,
        TrashBagApproach,
        USBApproach,
        USBInteraction,
        LaptopOpened,
        FileSortGameStart,
        FileSortGameComplete,
        GuitarBodyFound,
        GuitarCaseFound,
        PaperPuzzleStart,
        PaperPuzzleComplete,
        AllGuitarPartsFound,
        EndingTransition
    }

    private ScenarioState scenarioState;

    // 조건 체크용 변수들
    private bool playerNearTrashBag = false;
    private bool playerNearUSB = false;
    private bool usbInteracted = false;
    private bool laptopOpened = false;
    private bool fileSortGameDone = false;
    private bool guitarBodyFound = false;
    private bool guitarCaseFound = false;
    private bool paperPuzzleDone = false;
    private bool guitarPartsAllFound = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(ScenarioFlow());
    }

    IEnumerator ScenarioFlow()
    {
        scenarioState = ScenarioState.StartContact;

        // 1. 버스커에게 온 사진 확인 (에너지 +10)
        scenarioState = ScenarioState.ShowPhoto;
        Debug.Log("1. 버스커가 기타 사진을 보냄");
        yield return ShowMonologue("showPhoto");
        playerPower.IncreasePower(10);

        // 2. 기타 찾기 다짐
        // 독백
        scenarioState = ScenarioState.DecideToSearch;
        Debug.Log("2.1 기타를 찾아야겠다고 생각함");

        // 카톡 답장 
        Debug.Log("2.2 카톡 답장을 보냄");

        yield return ShowMonologue("decideSearch");

        // 3. 할 일 퀘스트(기타(본체) 찾기) 추가 (할 일 퀘스트 추가 알림이 필요할듯)
        Debug.Log("3. 퀘스트 추가: 기타 찾기");

        // 4. 2단 쓰봉 앞으로 갔을때 space 안내 출력
        scenarioState = ScenarioState.TrashBagApproach;
        Debug.Log("4. 쓰레기봉지 앞 도착 대기 중...");
        while (!playerNearTrashBag)
        {
            yield return null;
        }
        Debug.Log("쓰레기봉지 앞에 도착함. 스페이스 안내 출력");

        // 5. usb 앞으로 갔을때 space 안내 출력
        scenarioState = ScenarioState.USBApproach;
        Debug.Log("5. USB 근처 접근 대기 중...");
        while (!playerNearUSB)
        {
            yield return null;
        }
        Debug.Log("USB 앞에 도착함. 상호작용 안내 출력");

        // 6. usb 획득 시(할 일 퀘스트 완료) 대사 출력 + 할 일 퀘스트(노트북 열기) 추가
        scenarioState = ScenarioState.USBInteraction;
        Debug.Log("6. USB 상호작용 대기 중...");
        while (!usbInteracted)
        {
            yield return null;
        }
        Debug.Log("USB 획득. 퀘스트 완료 + 노트북 퀘스트 추가");
        playerPower.IncreasePower(10);

        // 7. usb 내용 확인 위해 노트북 실행 시 미니게임 진행(파일정리)
        scenarioState = ScenarioState.LaptopOpened;
        Debug.Log("7. 노트북 열기 대기 중...");
        while (!laptopOpened)
        {
            yield return null;
        }
        Debug.Log("노트북 열림 → 미니게임(파일정리) 시작");

        scenarioState = ScenarioState.FileSortGameStart;
        Debug.Log("미니게임(파일정리) 실행");
        while (!fileSortGameDone)
        {
            yield return null;
        }
        scenarioState = ScenarioState.FileSortGameComplete;
        Debug.Log("노트북 미니게임 완료! 에너지 +10");
        playerPower.IncreasePower(10);

        // 8. 기타 본체 찾을 시(할 일 퀘스트 완료) 할 일 퀘스트(다른 파츠 찾기) 추가
        scenarioState = ScenarioState.GuitarBodyFound;
        Debug.Log("8. 기타 본체 찾기 대기 중...");
        while (!guitarBodyFound)
        {
            yield return null;
        }
        Debug.Log("기타 본체 발견! 퀘스트 추가: 줄, 조율기, 케이스 찾기");

        // 9. 기타 케이스 찾을 시 미니게임(종이조각 맞추기)
        scenarioState = ScenarioState.GuitarCaseFound;
        Debug.Log("9. 기타 케이스 찾기 대기 중...");
        while (!guitarCaseFound)
        {
            yield return null;
        }
        Debug.Log("기타 케이스 찾음 → 미니게임(종이조각 맞추기) 시작");

        scenarioState = ScenarioState.PaperPuzzleStart;
        Debug.Log("미니게임(종이조각 맞추기) 실행 대기 중...");
        while (!paperPuzzleDone)
        {
            yield return null;
        }
        scenarioState = ScenarioState.PaperPuzzleComplete;
        Debug.Log("미니게임 완료! 갤러리 해금 + 에너지 +10");
        playerPower.IncreasePower(10);

        // 10. 모든 기타 파츠 발견 시 챕터 3으로 넘어감
        Debug.Log("10. 기타 파츠 전부 찾는 중...");
        while (!guitarPartsAllFound)
        {
            yield return null;
        }
        Debug.Log("모든 기타 파츠 발견! 에너지 +40");
        playerPower.IncreasePower(40);

        // 챕터3로 전환
        scenarioState = ScenarioState.EndingTransition;
        Debug.Log("챕터2 종료 → 챕터3로 전환 준비");
    }

    // 콘솔 출력용 임시 모노로그
    IEnumerator ShowMonologue(string key)
    {
        Debug.Log("MONO: " + key);
        yield return new WaitForSeconds(2f);
    }

    // 외부에서 상태 바꾸는 함수들
    public void OnPlayerNearTrashBag() => playerNearTrashBag = true;
    public void OnPlayerNearUSB() => playerNearUSB = true;
    public void OnUSBInteracted() => usbInteracted = true;
    public void OnLaptopOpened() => laptopOpened = true;
    public void OnFileSortGameDone() => fileSortGameDone = true;
    public void OnGuitarBodyFound() => guitarBodyFound = true;
    public void OnGuitarCaseFound() => guitarCaseFound = true;
    public void OnPaperPuzzleDone() => paperPuzzleDone = true;
    public void OnGuitarPartsAllFound() => guitarPartsAllFound = true;
}


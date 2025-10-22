using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter2Manager : MonoBehaviour
{
    public static Chapter2Manager Instance;
    public bool autoMove = false;

    // 플레이어, UI, 기타 연결 오브젝트들
    public PlayerMove playerMove;
    public player_power playerPower;
    public UnityEngine.UI.Image notification;

    // 모놀로그 데이터
    private Dictionary<string, List<string>> monoData;

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
        // 자유형식 JSON 파싱
        TextAsset monoJson = Resources.Load<TextAsset>("MonologueData/Mono2");
        if (monoJson != null)
        {
            monoData = JsonUtility.FromJson<Wrapper>(WrapJson(monoJson.text)).dict;
            Debug.Log($"Mono2.json 로드 완료. 총 {monoData.Count}개의 키를 읽음");
        }
        else
        {
            monoData = new Dictionary<string, List<string>>();
            Debug.LogWarning("Mono2.json을 찾을 수 없습니다. 경로를 확인하세요.");
        }

        StartCoroutine(ScenarioFlow());
    }

    // JsonUtility가 Dictionary를 바로 읽지 못하므로 Wrapper로 감싸서 처리
    [System.Serializable]
    private class Wrapper
    {
        public Dictionary<string, List<string>> dict;
    }

    private string WrapJson(string json)
    {
        return "{\"dict\":" + json + "}";
    }

    IEnumerator ScenarioFlow()
    {
        scenarioState = ScenarioState.StartContact;

        // 1. 버스커에게 온 사진 확인 (에너지 +10)
        scenarioState = ScenarioState.ShowPhoto;
        Debug.Log("1. 버스커가 기타 사진을 보냄");
        yield return ShowMono("showPhoto", 2f);
        playerPower.IncreasePower(10);

        // 2. 기타 찾기 다짐
        scenarioState = ScenarioState.DecideToSearch;
        Debug.Log("2. 기타를 찾아야겠다고 생각함");
        yield return ShowMono("decideSearch", 2f);

        // 3. 퀘스트 추가
        Debug.Log("3. 퀘스트 추가: 기타 찾기");

        // 4. 쓰레기봉지 접근 대기
        scenarioState = ScenarioState.TrashBagApproach;
        Debug.Log("4. 쓰레기봉지 앞 도착 대기 중...");
        while (!playerNearTrashBag) yield return null;
        Debug.Log("쓰레기봉지 앞에 도착함. 스페이스 안내 출력");

        // 5. USB 접근 대기
        scenarioState = ScenarioState.USBApproach;
        Debug.Log("5. USB 근처 접근 대기 중...");
        while (!playerNearUSB) yield return null;
        Debug.Log("USB 앞에 도착함. 상호작용 안내 출력");

        // 6. USB 상호작용
        scenarioState = ScenarioState.USBInteraction;
        Debug.Log("6. USB 상호작용 대기 중...");
        while (!usbInteracted) yield return null;
        Debug.Log("USB 획득. 퀘스트 완료 + 노트북 퀘스트 추가");
        playerPower.IncreasePower(10);

        // 7. 노트북 실행 (파일정리 미니게임)
        scenarioState = ScenarioState.LaptopOpened;
        Debug.Log("7. 노트북 열기 대기 중...");
        while (!laptopOpened) yield return null;
        Debug.Log("노트북 열림 → 미니게임(파일정리) 시작");

        scenarioState = ScenarioState.FileSortGameStart;
        while (!fileSortGameDone) yield return null;
        scenarioState = ScenarioState.FileSortGameComplete;
        Debug.Log("미니게임 완료! 에너지 +10");
        playerPower.IncreasePower(10);

        // 8. 기타 본체 찾기
        scenarioState = ScenarioState.GuitarBodyFound;
        Debug.Log("8. 기타 본체 찾기 대기 중...");
        while (!guitarBodyFound) yield return null;
        Debug.Log("기타 본체 발견! 퀘스트 추가: 줄, 조율기, 케이스 찾기");

        // 9. 기타 케이스 찾기 (종이조각 미니게임)
        scenarioState = ScenarioState.GuitarCaseFound;
        Debug.Log("9. 기타 케이스 찾기 대기 중...");
        while (!guitarCaseFound) yield return null;
        Debug.Log("기타 케이스 찾음 → 미니게임 시작");

        scenarioState = ScenarioState.PaperPuzzleStart;
        while (!paperPuzzleDone) yield return null;
        scenarioState = ScenarioState.PaperPuzzleComplete;
        Debug.Log("미니게임 완료! 갤러리 해금 + 에너지 +10");
        playerPower.IncreasePower(10);

        // 10. 모든 기타 파츠 발견 시
        Debug.Log("10. 기타 파츠 전부 찾는 중...");
        while (!guitarPartsAllFound) yield return null;
        Debug.Log("모든 기타 파츠 발견! 에너지 +40");
        playerPower.IncreasePower(40);

        scenarioState = ScenarioState.EndingTransition;
        Debug.Log("챕터2 종료 → 챕터3로 전환 준비");
    }

    // -----------------------------
    // 모놀로그 관련
    // -----------------------------
    IEnumerator ShowMono(string key, float showTime)
    {
        if (monoData.ContainsKey(key))
        {
            MonologueManager.Instance.ShowMonologuesSequentially(monoData[key], showTime);
        }
        else
        {
            Debug.LogWarning($"대사 키 {key}를 Mono2.json에서 찾을 수 없습니다.");
        }

        yield return new WaitForSeconds(monoData.ContainsKey(key) ? monoData[key].Count * showTime : 0f);
    }

    public void PlayUSBFirstDialogue(int usbIndex)
    {
        string key = $"usb{usbIndex}_first";
        Debug.Log($"USB {usbIndex} 대사 재생 ({key})");

        if (monoData.ContainsKey(key))
            StartCoroutine(ShowMono(key, 2f));
        else
            Debug.LogWarning($"대사 키 {key}가 Mono2.json에 없습니다.");
    }

    public void PlayUSBRereadHint()
    {
        MonologueManager.Instance.ShowMonologuesSequentially(
            new List<string> { "이미 확인한 USB다. 노트북에서 확인해보면 될 것 같다." },
            2f
        );
    }

    // -----------------------------
    // 외부에서 상태 갱신용 함수
    // -----------------------------
    public void OnPlayerNearTrashBag() => playerNearTrashBag = true;
    public void OnPlayerNearUSB() => playerNearUSB = true;
    public void OnUSBInteracted(int usbIndex) => usbInteracted = true;
    public void OnLaptopOpened() => laptopOpened = true;
    public void OnFileSortGameDone() => fileSortGameDone = true;
    public void OnGuitarBodyFound() => guitarBodyFound = true;
    public void OnGuitarCaseFound() => guitarCaseFound = true;
    public void OnPaperPuzzleDone() => paperPuzzleDone = true;
    public void OnGuitarPartsAllFound() => guitarPartsAllFound = true;
}

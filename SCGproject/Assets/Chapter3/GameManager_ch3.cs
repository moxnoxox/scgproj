using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class GameManager_ch3 : MonoBehaviour
{
    public static GameManager_ch3 Instance;

    [Header("참조 오브젝트")]
    public PlayerMove_ch3 playerMove;
    public Transform choicePanel;
    public GameObject choiceButtonPrefab;
    public Image backgroundImage;
    public Image illustrationImage;   // 일러스트 표시용
    public Image fadeImage;           // 암전용 이미지 (검은색, 알파 0)
    public MonologueManager_ch3 monologueManager;
    public player_power_ch3 playerPower;

    [Header("NPC 및 트리거")]
    public Transform npcTarget;
    public float triggerDistance = 2f;

    // JSON 캐싱(Wrapper → Dict)
    private Dictionary<string, List<DialogueLine>> monoData;

    // 선택지 상태
    private bool choiceSelected;
    private int selectedIndex;

    [Header("엔딩 관련")]
    public GameObject endingVideoView;  // RawImage로 만든 영상 출력 영역
    public VideoPlayer endingVideoPlayer;
    public TextMeshProUGUI endingLine1;
    public TextMeshProUGUI endingLine2;
    public TextMeshProUGUI endingLine3;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        LoadMonoJson();
        StartCoroutine(ScenarioFlow());
        if (endingVideoView != null)
        endingVideoView.SetActive(false);
    }

    // =========================
    // JSON 로드 (Wrapper 방식)
    // =========================
    private void LoadMonoJson()
    {
        TextAsset monoJson = Resources.Load<TextAsset>("MonologueData/Mono3");
        if (monoJson == null)
        {
            Debug.LogError("[GameManager_ch3] Mono3.json 파일을 찾을 수 없습니다 (Resources/MonologueData/Mono3.json)");
            monoData = new Dictionary<string, List<DialogueLine>>();
            return;
        }

        try
        {
            var wrapper = JsonUtility.FromJson<MonoDataWrapper>(monoJson.text);
            if (wrapper == null)
            {
                Debug.LogError("[GameManager_ch3] JsonUtility 파싱 실패 (wrapper == null)");
                monoData = new Dictionary<string, List<DialogueLine>>();
                return;
            }

            monoData = wrapper.ToDictionary();
            Debug.Log($"[GameManager_ch3] JSON 로드 완료. 키 개수: {monoData.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameManager_ch3] JSON 로드 중 예외: {e.Message}");
            monoData = new Dictionary<string, List<DialogueLine>>();
        }
    }

    // =========================
    // 시나리오 흐름
    // =========================
    private IEnumerator ScenarioFlow()
    {
        // 초기 상태
        playerMove.movable = false;
        playerMove.canInput = false;
        if (illustrationImage) illustrationImage.gameObject.SetActive(false);
        if (fadeImage)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        // 1) 시작 2초 후 첫 대사
        yield return new WaitForSeconds(2f);
        yield return Show("intro_1", 3f);

        // 플레이어 이동 허용
        playerMove.movable = true;
        playerMove.canInput = true;

        // 2) NPC 접근까지 대기
        yield return new WaitUntil(() =>
            npcTarget != null &&
            Vector2.Distance(playerMove.transform.position, npcTarget.position) <= triggerDistance
        );

        // 입력 잠금
        playerMove.movable = false;
        playerMove.canInput = false;

        // 3) 만남 대사 + 일러스트 + 대사
        StartCoroutine(ShowIllustration("meetguitar"));
        yield return Show("meet_1", 4.5f);
        yield return Show("meet_2", 3f);
        // ================선택지
        yield return ShowChoices(new List<string>
        {
            "> 하루종일…? 언제부터 있던 거야?",
            "> 어디서 나타난 거야?"
        });
        int meetChoice = GetChoiceResult();

        // 결과 분기
        if (meetChoice == 0)
        {
            // ‘하루종일…? 언제부터 있던 거야?’ 선택 시
            yield return monologueManager.ShowDialogueLines(new List<DialogueLine>
            {
                new DialogueLine { speaker = "[해랑]", text = "너……설마 여기서 계속 기다리고 있었어? \n언제부터?" },
                new DialogueLine { speaker = "[인영]", text = "음…아마도 1시간은 훌쩍 넘겼을걸?" },
                new DialogueLine { speaker = "[인영]", text = "죽어도 집에만 있겠다는 애가 오랜만에 나온다는데, \n기타만 덜렁 현관에 냅두고 다시 들어갈지 누가 알아?" },
            }, 3f);
        }
        else
        {
            // ‘어디서 나타난 거야?’ 선택 시
            yield return monologueManager.ShowDialogueLines(new List<DialogueLine>
            {
                new DialogueLine { speaker = "[해랑]", text = "어디 있었어?" },
                new DialogueLine { speaker = "[인영]", text = "솔직히 말하면… 너 또 도망갈까봐 \n스토커마냥 풀숲에 숨어있으려고 했는데" },
                new DialogueLine { speaker = "[인영]", text = "그래도 네가 도망갈 것 같아서, \n그냥 건물 뒤 벤치에 누워 있었어." },
                new DialogueLine { speaker = "[해랑]", text = "그건 그거대로 무서운데…" },
            }, 3f);
        }
        // ================선택지 끝
        yield return Show("meet_3", 3f);


        // 4) 기타 전달
        yield return StartCoroutine(ShowIllustration("guitar_fixed_illust"));
        yield return Show("giveGuitar", 3f);
        // ===============선택지
        yield return ShowChoices(new List<string>
        {
            "> 지금까지 있었던 일을 말한다.",
            "> 오늘 있었던 일을 말한다.",
            "> 앞으로의 일을 말한다."
        });

        int guitarChoice = GetChoiceResult();

        // 선택지 분기 
        if (guitarChoice == 0)
        {
            // ‘지금까지 있었던 일을 말한다.’ 선택 시
            yield return monologueManager.ShowDialogueLines(new List<DialogueLine>
            {
                new DialogueLine{ speaker="[해랑]", text="요새 일이 너무 바빠서 도저히 내 시간이 안 나더라고. 처음엔 \n일을 하면 할수록 내 능력을 인정받는 것 같아서 뿌듯했는데…" },
                new DialogueLine{ speaker="[해랑]", text="난 워커홀릭은 안 될 사람인가봐. 특히 내 주변 사람을 \n챙길 여유도 없다는 게 너무 힘들더라고." },
                new DialogueLine{ speaker="[해랑]", text="너도 포함해서……" },
            }, 3f);
        }
        else if (guitarChoice == 1)
        {
            // ‘오늘 있었던 일을 말한다.’ 선택 시
            yield return monologueManager.ShowDialogueLines(new List<DialogueLine>
            {
                new DialogueLine{ speaker="[해랑]", text="사실… 지금까지 다 회피한다고 집 청소도 안 하고 살았거든? \n그러다 너 기타 찾는다고 오랜만에 집 청소를 했더니" },
                new DialogueLine{ speaker="[해랑]", text="옛날 물건들도 찾고, 기타도 고치고… \n신기한 일들이 많았어." },
                new DialogueLine{ speaker="[해랑]", text="그냥, 요즘 내가 뭘 좋아하는지도 잊고 살았는데… \n다시 찾으니 기쁘더라." },
            }, 3f);
        }
        else
        {
            // ‘앞으로의 일을 말한다.’ 선택 시
            yield return monologueManager.ShowDialogueLines(new List<DialogueLine>
            {
                new DialogueLine{ speaker="[해랑]", text="꿈도 목표도 잊고 살았는데… 오늘 계획했던 일 \n하나하나 해결해 나가니까 재밌긴 하더라고." },
                new DialogueLine{ speaker="[해랑]", text="앞으로 사소한 일이라도 다시 계획해 보려고 해.\n원동력이 생기는 느낌…?" },
                new DialogueLine{ speaker="[해랑]", text="물론, 네가 말한 것처럼 \n이제 밖으로도 나가보고……" },
            }, 3f);
        }
        // ==================선택지 끝
        yield return Show("giveGuitar_2", 3f);


        // 5) 기타 돌려줌
        StartCoroutine(ShowIllustration("guitar_fixed_illust"));
        yield return Show("giveGuitar_3", 3f);

        // 6) 에너지 변동
        if (playerPower != null) playerPower.IncreasePower(20);
        yield return Show("filledEnergy", 3f);

        // 7) 소리치는 대사(흔들림) → 후속 대사
        yield return Show("shout", 2f, shake:true);
        yield return Show("afterShout", 3f);

        // 8) 암전 → 배경 전환
        yield return StartCoroutine(FadeOut(1.5f));
        yield return new WaitForSeconds(1.5f);
        SetBackgroundToIllustration("End");
        yield return StartCoroutine(FadeIn(1.5f));

        // 9) 대사
        yield return Show("bus", 3f);

        // 10) 선택지
        yield return ShowChoices(new List<string>
        {
            "> 모두와 함께 연습했던 <b>연습 스튜디오</b>",
            "> 첫 버스킹했던 <b>바다</b>",
            "> 발길 끊었던 <b>상담센터</b>",
        });
        int result = GetChoiceResult();
        PlayerPrefs.SetInt("ch3_choiceResult", result);

        // 11) 마무리 대사
        yield return Show("afterChoose", 3f);

        // -------엔딩---------
        yield return StartCoroutine(PlayEndingSequence());

    }
    private IEnumerator PlayEndingSequence()
    {   
        endingVideoPlayer.Prepare();

        while (!endingVideoPlayer.isPrepared)
        {
            Debug.Log("Preparing...");
            yield return null;
        }

        Debug.Log("Prepared True! Playing now.");

        // 1) 엔딩 영상 재생
        if (endingVideoView != null)
            endingVideoView.SetActive(true);   // 영상 표시 UI 활성화
            Debug.Log("ui 활성화 완료");

        if (endingVideoPlayer != null)
        {
            Debug.Log("VideoPlayer Ready? " + endingVideoPlayer.isPrepared);
            endingVideoPlayer.Play();

            // 영상 재생 끝날 때까지 대기
            while (endingVideoPlayer.isPlaying){
                Debug.Log("Video frame: " + endingVideoPlayer.frame);
                yield return null;
            }

            Debug.Log("영상 재생 완료");
        }
        else
        {
            // 영상이 없으면 그냥 3초 대기
            yield return new WaitForSeconds(3f);
            Debug.Log("영상이 존재하지 않음");
        }

        yield return StartCoroutine(FadeOut(1.5f));  
        // 영상 끝났으면 RawImage 끄기
        if (endingVideoView != null)
            endingVideoView.SetActive(false);

        // backgroundImage를 검정으로 만들기 
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.sprite = Resources.Load<Sprite>("Illustrations/black"); 
        backgroundImage.color = Color.black;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeIn(1.0f));

        // 3) 엔딩 분기 불러오기
        int endingIndex = PlayerPrefs.GetInt("ch3_choiceResult", 0);

        if (endingIndex == 0)
            yield return StartCoroutine(Ending_Studio());
        else if (endingIndex == 1)
            yield return StartCoroutine(Ending_Sea());
        else
            yield return StartCoroutine(Ending_Counseling());

        // 4) 공통 엔딩 메시지
        yield return StartCoroutine(FadeOut(2f));
        backgroundImage.gameObject.SetActive(true);
        backgroundImage.sprite = Resources.Load<Sprite>("Illustrations/black"); 
        backgroundImage.color = Color.black;
        yield return StartCoroutine(ShowFinalThanks());

        // 5) 타이틀 씬으로 돌아가기
        SceneManager.LoadScene("Chapter1");
    }

    private IEnumerator Ending_Studio()
    {
        // 암전 상태에서 대사만
        yield return new WaitForSeconds(2f);

        yield return monologueManager.ShowDialogueLines(
            new List<DialogueLine>{
                new DialogueLine{ speaker="[해랑]", text="이 방이었나…? 맞는 것 같은데." },
                new DialogueLine{ speaker="[인영]", text="왜 불이 켜져 있지?\n별일이네, 여기 완전 외진 곳인데. 누가 있나?" },
            }, 3f);

        // 2초 후 일러스트 등장
        yield return StartCoroutine(FadeOut(0.5f));
        SetBackgroundToIllustration("ending_studio");
        yield return StartCoroutine(FadeIn(1.0f));

        // 대사 이어짐
        yield return monologueManager.ShowDialogueLines(
            new List<DialogueLine>{
                new DialogueLine{ speaker="[???]", text="! 누구…… 해랑이, 인영이?" },
                new DialogueLine{ speaker="[인영]", text="엥, 민주야?? 여기서 다 만나네!\n너 아직도 여기서 연습해?" },
                new DialogueLine{ speaker="[민주]", text="너희 맞지? 오랜만이다! \n그럼~ 고등학교 때 맨날 너네랑 여기만 들락날락거리니까,"}, 
                new DialogueLine{ speaker="[민주]", text ="대학교도 졸업학기인데 아직도 못 벗어나고 있다…\n지금은 곧 동아리 공연이라서 연습하는 중!"},
                new DialogueLine{ speaker="[해랑]", text ="너 밴드 동아리 들어갔어? 난 바쁠 것 같아서 한동안 놓고\n지냈어. 그래도……다시 하고 싶어져서 한번 시작해 보려고."},
                new DialogueLine{ speaker="[민주]", text ="좋다! 넌 잘 치니까 또 금방 늘 거야. \n아, 나 아직 그것도 있다? 우리 맨 처음 연습곡 악보!"},
                new DialogueLine{ speaker="[인영]", text ="진짜? 악보만 있으면 당장도 칠 수 있지.\n여기 악기도 많은데 쳐 볼래?"},
                new DialogueLine{ speaker="[해랑]", text ="그래. 지금은……하루종일 기타만 쳐도 좋을 것 같아."}
            }, 3f);

        // USB3 조건
        if (PlayerPrefs.GetInt("usb3_used", 0) == 1)
        {
            yield return monologueManager.ShowDialogueLines(
                new List<DialogueLine>{
                    new DialogueLine{ speaker="[해랑]", text="아, 그러고 보니 집에서 이걸 발견했는데…" },
                }, 2.7f);

            yield return ShowChoices(new List<string>
        {
            "> USB를 건넨다."
        });
            yield return ShowIllustration("usb");

            yield return monologueManager.ShowDialogueLines(
                new List<DialogueLine>{
                    new DialogueLine{ speaker="[해랑]", text="우리 합주할 때 녹음해 뒀던 거더라.\n합주 끝나면 무조건 회식 갔었는데, 기억나?" },
                    new DialogueLine{ speaker="[민주]", text="헐, 이게 아직도 있었어? 당연하지! \n보통 여기 바로 옆 고깃집 갔었지 않아?" },
                    new DialogueLine{ speaker="[인영]", text="맛있겠다~ 아, 안 되겠어. \n딱 저녁시간대인데 밥 먹고 올래? 사장님도 우리 알아보시려나?"},
                    new DialogueLine{ speaker="[해랑]", text="하하, 알아보시면 서비스 달라고 하자."},
                    new DialogueLine{ speaker="[민주]", text="좋아! 볶음밥까지 다 긁어먹고 \n에너지 얻어서 5곡 연속으로 하기다!"}
                }, 3f);
        }
    }

    private IEnumerator Ending_Sea()
    {
        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(FadeOut(0.5f));
        SetBackgroundToIllustration("ending_beach");
        yield return StartCoroutine(FadeIn(1.0f));

        yield return monologueManager.ShowDialogueLines(
            new List<DialogueLine>{
                new DialogueLine{ speaker="[인영]", text="여기쯤이었나? 맞지?" },
                new DialogueLine{ speaker="[해랑]", text="응. 하나도 안 변했네… \n버스킹 자리 때문에 다른 밴드랑 싸우지 않았어?" },
                new DialogueLine{ speaker="[인영]", text="맞다! 그래도 잘 해결돼서 다행이었어." },
                new DialogueLine{ speaker="[해랑]", text="그러게. 그때도 사람 많고 날씨 좋았는데… \n여기 오니까 다 기억난다." },
                new DialogueLine{ speaker="[인영]", text="그렇네. 아무리 생각해도, 바닷가에서 하는 건 \n진짜 탁월한 선택이었어." },
                new DialogueLine{ speaker="[해랑]", text="응. 멍 때리기 좋아……\n잡생각도 없어지는 것 같고. 종종 와야겠어." },
                new DialogueLine{ speaker="[인영]", text="뭐야, 혼자만 오려고? 나도 꼭 불러줘야 해!"},
                new DialogueLine{ speaker="[해랑]", text="하하, 당연하지. 된다면… 같이 버스킹했던 애들도 다 불러볼까?" },
                new DialogueLine{ speaker="[인영]", text="너무 좋아!"}
            }, 3f);

        // USB2 분기
        if (PlayerPrefs.GetInt("usb2_used", 0) == 1)
        {
            yield return monologueManager.ShowDialogueLines(
                new List<DialogueLine>{ 
                    new DialogueLine{ speaker="[해랑]", text="아, 그러고 보니 집에서 이걸 발견했는데…" }
                }, 3f);

             yield return ShowChoices(new List<string>
            {
                "> USB를 건넨다."
            });

            yield return ShowIllustration("usb2");

            yield return monologueManager.ShowDialogueLines(
                new List<DialogueLine>{
                    new DialogueLine{ speaker="[해랑]", text="우리 첫 버스킹 연습할 때 녹음해 뒀던 거더라." },
                    new DialogueLine{ speaker="[인영]", text="진짜?! 나도 들을래! 너무 웃길 것 같아." },
                    new DialogueLine{ speaker="[해랑]", text="어. 우리 코드 이상한 건 이때부터더라고. \n……나중에 우리 집 오면 같이 듣자." },
                    new DialogueLine{ speaker="[인영]", text="진심이지? 무르기 없어. 나 캘박한다!" },
                }, 3f);
        }
    }

    private IEnumerator Ending_Counseling()
    {
        yield return new WaitForSeconds(2f);

        yield return monologueManager.ShowDialogueLines(
            new List<DialogueLine>{
                new DialogueLine{ speaker="[해랑]", text="진짜 괜찮겠어?" },
                new DialogueLine{ speaker="[인영]", text="괜찮다니까~ 기타 치면서 기다릴게!" },
                new DialogueLine{ speaker="[인영]", text="난 집 갔다 생각하고 편하게 다녀오셔." },
                new DialogueLine{ speaker="[해랑]", text="…그래. 고마워." }
            }, 3f);

        yield return StartCoroutine(FadeOut(0.5f));
        SetBackgroundToIllustration("ending_counseling");
        yield return StartCoroutine(FadeIn(1.0f));

        yield return monologueManager.ShowDialogueLines(
            new List<DialogueLine>{
                new DialogueLine{ speaker="[상담사]", text="안녕하세요, 해랑 씨. 처음 상담 오신 거죠?" },
                new DialogueLine{ speaker="[상담사]", text="오기까지 쉽지 않았을 텐데, 잘 오셨어요." },
                new DialogueLine{ speaker="[해랑]", text="네… 사실 요새 거의 집에서만 지냈거든요.\n밖에 나오는 것 자체가 오랜만이에요." },
                new DialogueLine{ speaker="[상담사]", text="원래 시작하는 걸 가장 어려워 하세요. \n해랑 씨는 이미 좋은 단계에 있으신 거예요." },
                new DialogueLine{ speaker="[해랑]", text="친구가 손을 잡고 끌다시피 해서요……\n혼자였으면 못 왔을 거예요" },
                new DialogueLine{ speaker="[해랑]", text="그래도……아직은 좀 자신감이 없는 것 같아요.\n솔직히 여기 온 것도 반쯤 충동적이었어서…" },
                new DialogueLine{ speaker="[상담사]", text="그런 불안은 당연해요. 자연스러운 현상이죠. \n더 안 보면 그만인 사람이다, 생각하고 편히 말씀해 주세요." },
                new DialogueLine{ speaker="[해랑]", text="……저, 오늘 정말 많은 일이 있었거든요.\n사실 어제까지도 업무라고 하면 되게 많았고……" },
                new DialogueLine{ speaker="[해랑]", text="그래서…묵혀둔 말이 정말 많아요." },
                new DialogueLine{ speaker="[상담사]", text="그래요, 해랑 씨. 시간은 얼마든지 있으니 \n서두르지 않아도 괜찮아요" },
                new DialogueLine{ speaker="[상담사]", text="천천히, 마음 가는대로 언제든지 들려주세요." },
                new DialogueLine{ speaker="[해랑]", text="감사해요. 음…그러니까……" },
                new DialogueLine{ speaker="[해랑]", text="제가 되게 옛날에 좋아했던 기타를 \n오랜만에 발견했었는데요…" }
            }, 3f);
    }

    private IEnumerator ShowFinalThanks()
    {
        // 처음엔 비활성화 껴두기
        endingLine1.gameObject.SetActive(false);
        endingLine2.gameObject.SetActive(false);
        endingLine3.gameObject.SetActive(false);

        // 문구 넣기
        endingLine1.text = "THX FOR PLAYING!";
        endingLine2.text = "모두의 마음속 짐이 쌓여지지 않기를,";
        endingLine3.text = "한결 가벼워진 걸음으로 세상을 바라볼 수 있기를 바랍니다";

        // 1) 첫 줄 페이드인
        yield return FadeInText(endingLine1, 1.2f);
        yield return new WaitForSeconds(0.6f);

        // 2) 두 번째 줄 페이드인
        yield return FadeInText(endingLine2, 1.2f);
        yield return new WaitForSeconds(0.6f);

        // 3) 세 번째 줄 페이드인
        yield return FadeInText(endingLine3, 1.2f);

        // 4) 전체 메시지 5초 유지
        yield return new WaitForSeconds(5f);

        // 끄기
        endingLine1.gameObject.SetActive(false);
        endingLine2.gameObject.SetActive(false);
        endingLine3.gameObject.SetActive(false);
    }



    // 편의 함수: 키로 대사 출력
    private IEnumerator Show(string key, float showTime, bool shake = false)
    {
        if (monoData == null || !monoData.ContainsKey(key) || monoData[key] == null || monoData[key].Count == 0)
        {
            Debug.LogWarning($"[GameManager_ch3] 키 '{key}' 없음 혹은 빈 배열");
            yield break;
        }
        yield return StartCoroutine(monologueManager.ShowDialogueLines(monoData[key], showTime, shake));
    }

    // =========================
    // 일러스트 & 페이드
    // =========================
    private IEnumerator ShowIllustration(string illustName)
    {
        if (illustrationImage == null) yield break;

        Sprite sprite = Resources.Load<Sprite>($"Illustrations/{illustName}");
        if (sprite == null)
        {
            Debug.LogWarning($"일러스트 {illustName}을(를) 찾을 수 없음");
            yield break;
        }

        illustrationImage.sprite = sprite;
        illustrationImage.SetNativeSize();                      // 원본 크기로 맞추기
        illustrationImage.rectTransform.anchoredPosition = Vector2.zero; // 중앙 정렬
        illustrationImage.color = new Color(1, 1, 1, 0);
        illustrationImage.gameObject.SetActive(true);

        // 🔹 페이드인
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            illustrationImage.color = new Color(1, 1, 1, Mathf.Clamp01(t));
            yield return null;
        }

        // 🔹 3초 유지
        yield return new WaitForSeconds(3f);

        // 🔹 페이드아웃
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(t);
            illustrationImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        // 🔹 비활성화
        illustrationImage.gameObject.SetActive(false);
    }


    private void SetBackgroundToIllustration(string illustName)
    {
        if (backgroundImage == null) return;

        Sprite sprite = Resources.Load<Sprite>($"Illustrations/{illustName}");
        if (sprite == null) return;

        backgroundImage.sprite = sprite;
        backgroundImage.color = Color.white;
        backgroundImage.SetNativeSize();
        backgroundImage.rectTransform.anchoredPosition = Vector2.zero;
        backgroundImage.gameObject.SetActive(true);

        if (illustrationImage) illustrationImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut(float duration)
    {
        if (fadeImage == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / duration));
            yield return null;
        }
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadeImage == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1 - Mathf.Clamp01(t / duration));
            yield return null;
        }
    }

    private IEnumerator FadeInText(TextMeshProUGUI tmp, float duration)
    {
        tmp.gameObject.SetActive(true);

        Color c = tmp.color;
        c.a = 0;
        tmp.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            tmp.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
    }

    // =========================
    // 선택지
    // =========================
    public IEnumerator ShowChoices(List<string> options)
    {
        choiceSelected = false;
        selectedIndex = -1;

        if (monologueManager != null && monologueManager.dialoguePanel != null)
        {
            monologueManager.dialoguePanel.SetActive(true);
            if (monologueManager.nameText != null)
                monologueManager.nameText.gameObject.SetActive(true);
            if (monologueManager.dialogueText != null)
                monologueManager.dialogueText.gameObject.SetActive(false);
        }

        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        choicePanel.gameObject.SetActive(true);

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel);

            // 🔹 프리팹이 비활성화 상태로 복제된 경우 강제 활성화
            btnObj.SetActive(true);

            // 🔹 Button과 TMP 강제 Enable
            var button = btnObj.GetComponent<Button>();
            if (button != null) button.enabled = true;

            var text = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.enabled = true;
                text.gameObject.SetActive(true);
                text.text = options[i];
            }

            if (button != null)
                button.onClick.AddListener(() => OnChoiceSelected(index));
        }

        yield return new WaitUntil(() => choiceSelected);

        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        choicePanel.gameObject.SetActive(false);

        if (monologueManager != null && monologueManager.dialoguePanel != null)
            monologueManager.dialoguePanel.SetActive(false);
    }


    private void OnChoiceSelected(int index)
    {
        Debug.Log($"[선택지 클릭 감지됨] index = {index}");
        selectedIndex = index;
        choiceSelected = true;
    }

    public int GetChoiceResult()
    {
        return selectedIndex;
    }
}

// ----------------------------
// JSON Wrapper & Line 타입
// ----------------------------
[Serializable]
public class MonoDataWrapper
{
    public List<DialogueLine> intro_1;
    public List<DialogueLine> meet_1;
    public List<DialogueLine> meet_2;
    public List<DialogueLine> meet_3;
    public List<DialogueLine> giveGuitar;
    public List<DialogueLine> giveGuitar_2;
    public List<DialogueLine> giveGuitar_3;
    public List<DialogueLine> filledEnergy;
    public List<DialogueLine> shout;
    public List<DialogueLine> afterShout;
    public List<DialogueLine> bus;
    public List<DialogueLine> afterChoose;

    public Dictionary<string, List<DialogueLine>> ToDictionary()
    {
        var d = new Dictionary<string, List<DialogueLine>>();
        if (intro_1 != null) d["intro_1"] = intro_1;
        if (meet_1 != null) d["meet_1"] = meet_1;
        if (meet_2 != null) d["meet_2"] = meet_2;
        if (meet_3 != null) d["meet_3"] = meet_3;
        if (giveGuitar != null) d["giveGuitar"] = giveGuitar;
        if (giveGuitar_2 != null) d["giveGuitar_2"] = giveGuitar_2;
        if (giveGuitar_3 != null) d["giveGuitar_3"] = giveGuitar_3;
        if (filledEnergy != null) d["filledEnergy"] = filledEnergy;
        if (shout != null) d["shout"] = shout;
        if (afterShout != null) d["afterShout"] = afterShout;
        if (bus != null) d["bus"] = bus;
        if (afterChoose != null) d["afterChoose"] = afterChoose;
        return d;
    }
}

[Serializable]
public class DialogueLine
{
    public string speaker;
    public string text;
}

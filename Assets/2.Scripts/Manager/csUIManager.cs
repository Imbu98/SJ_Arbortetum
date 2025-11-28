using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class csUIManager : MonoBehaviour
{
    public static csUIManager Instance { get { return _Instance; } }
    private static csUIManager _Instance;

    public GameObject startScreen; // 시작화면
    public csMainScreen mainScreen; // 메인화면
    public GameObject mapScreen; // 지도 
    public GameObject missionPopup; // 미션창
    public GameObject speechToTextScreen; // 음성 언어 입력 화면
    public csQuizScreen quizScreen; // 퀴즈창
    public GameObject settingScreen; // 설정창
    public GameObject QnA_Screen; // 수목원 QnA창
    public GameObject stampTour_Screen; // 수목원 스탬프투어




    private GameObject currentScreen;
    private GameObject currentPanel;


    

    [Header("AIText")]
    public TextMeshProUGUI mainScreenAIText;   // 메인 화면 텍스트
    private Coroutine typingRoutine;
    private bool isTyping = false;
    [SerializeField] private Button skipButton;
    [SerializeField] private ScrollRect scrollRect;

    private float heightLimit = 400f;

    [Header("AutoRecommend")]
    private bool isInMainScreen;
    private Coroutine timerRoutine;

    //Back Button Management
    private class BackStackEntry
    {
        public object key;            // UI별로 구분하는 키
        public UnityAction action;    // 뒤로가기로 실행할 동작
    }

    private Stack<BackStackEntry> backStack = new Stack<BackStackEntry>();

    private bool bBlocked = false; // 뒤로가기 버튼 막기 여부

  


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)&&bBlocked==false)
        {
            ExecuteBack();
        }
    }


    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    void Start()
    {

    }

    // 화면 전환 메서드
    public void ChangeScreen(GameObject newScreen)
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }
        newScreen.SetActive(true);
        currentScreen = newScreen;
    }

    // 메인 화면 내의 패널 전환 메서드
    public void ChangePanel(GameObject newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        newPanel.SetActive(true);
        currentPanel = newPanel;
    }

    private void TogglePopup(GameObject popup, bool show, bool returnToMain = true)
    {
        if (show)
        {
            popup.SetActive(true);
            if(timerRoutine!=null)
            {
                StopCoroutine(timerRoutine);
            }
             Push(popup,() => TogglePopup(popup, false, returnToMain));
        }
        else
        {
            popup.SetActive(false);
            if (returnToMain)
                SetIsInMainScreen(true);

            Remove(popup);
        }
        csSoundManager.Instance.HashPlayEffectSound("1_Touch_basic");

    }



    public void PopupMap(bool show)
    {
        if (!IsInsideArboretum())
        {
            NotInsideInArboretum();
            return;
        }

        TogglePopup(mapScreen, show);
    }

    public void PopupMission(bool show)
    {
        if(show)
        {
            if (!IsInsideArboretum())
            {
                NotInsideInArboretum();
                return;
            }
            if (csMissionManager.Instance.aiCreatedMissions.missions.Count == 0)
            {
                SetAIChatText("AI와 대화를 통해 경로를 추천받아보세요");
                return;
            }
        }

        TogglePopup(missionPopup, show);
    }

    public void PopupStampTour(bool show)
    {
        if (show)
        {
            if (!IsInsideArboretum())
            {
                NotInsideInArboretum();
                return;
            }
        }

        TogglePopup(stampTour_Screen, show);
    }
    public void PopupQnA_Screen(bool show)
    {
        TogglePopup(QnA_Screen, show);
    }

    public void OnClickedQuizButton(bool show)
    {
        PopupQuizScreen(show,QuizGenerationType.ObserveQuiz);
    }

    public void PopupQuizScreen(bool show,QuizGenerationType quizGenerationType=QuizGenerationType.None)
    {
        quizScreen.currentQuizGenerationType = quizGenerationType;

        switch (quizGenerationType)
        {
            case QuizGenerationType.ObserveQuiz:
                {
                    if (csSingleton.Instance.savedQuizList.quizDataWrapperList.Count == 0)
                    {
                        SetAIChatText("식물을 관찰하여 퀴즈를 생성해보세요");
                        return;
                    }
                    TogglePopup(quizScreen.gameObject, show);
                    break;
                }
            case QuizGenerationType.StampTourQuiz:
                {
                    TogglePopup(quizScreen.gameObject, show);
                    break;
                }
            default:
                {
                    TogglePopup(quizScreen.gameObject, show);
                    break;
                }
        }
    }

    public void PopupSpeechToText(bool show)
    {
        TogglePopup(speechToTextScreen, show, false);
    }

    public void PopupSettingScreen(bool show)
    {
        TogglePopup(settingScreen, show);
    }
    private bool IsInsideArboretum()
    {
        return csMapManager.Instance.IsInsideBoundary(
            csMapManager.Instance.MyGPS.Latitude,
            csMapManager.Instance.MyGPS.Longitude
        );
    }

    

    public void SetAIChatText(string text)
    {
        // 이전 타이핑 코루틴이 있으면 중지
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeTextRoutine(text));
    }

    
    public IEnumerator PlayAIChatSequence(List<string> messages, System.Action onComplete = null)
    {
        foreach (var msg in messages)
        {
            // 문장 출력
            SetAIChatText(msg);

            // 현재 문장이 타이핑되는 동안 대기
            while (isTyping)
                yield return null;

            // 문장의 마지막 상태(스킵 여부 포함) 후 0.1초 정도 텀
            yield return new WaitForSeconds(0.1f);
        }

        onComplete?.Invoke();
    }

    private IEnumerator TypeTextRoutine(string text)
    {
        isTyping = true;
        mainScreenAIText.text = "";

        float delay = 0.05f;

        // 화면 터치 시 전체 텍스트 즉시 표시되도록 이벤트 등록
        bool isSkipped = false;
        Action skipAction = () => { isSkipped = true; };
        RegisterSkip(skipAction);

        foreach (char c in text)
        {
            if (isSkipped)
                break;

            mainScreenAIText.text += c;

            CheckScrollActivation();

            yield return new WaitForSeconds(delay);
        }

        // 스킵 시 전체 텍스트 즉시 표시
        mainScreenAIText.text = text;

        CheckScrollActivation();

        UnregisterSkip(skipAction);
        isTyping = false;
        typingRoutine = null;
    }

    private void CheckScrollActivation()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainScreenAIText.transform.parent.GetComponent<RectTransform>());

        float contentHeight = scrollRect.content.sizeDelta.y;

        if (contentHeight <= heightLimit)
        {
            scrollRect.GetComponent<RectTransform>().sizeDelta= new Vector2(
                scrollRect.GetComponent<RectTransform>().sizeDelta.x,
                contentHeight
            );
        }
        else
        {
            scrollRect.verticalNormalizedPosition= 0f; // 맨 아래로 스크롤
        }
    }

    public void ResetAIChatText()
    {
        string resetText = $"{csSingleton.Instance.strPlayerNickName}님 무엇을 도와드릴까요?";

        SetAIChatText(resetText);
    }

    public void NotInsideInArboretum()
    {
        string notInsideArboretumText = $"{csSingleton.Instance.strPlayerNickName}님, 해당 기능은 수목원 내에서만 사용가능한 기능입니다";

        SetAIChatText(notInsideArboretumText);
    }

    // 메인화면을 띄울 때, 메인화면으로 돌아갈 때 자동 미션 추천 타이머를 실행시키기 위한 함수

    public void SetIsInMainScreen(bool value)
    {
        isInMainScreen = value;

        if (!isInMainScreen)
            return;

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        timerRoutine = StartCoroutine(StartRecommendTimer());
    }

    private IEnumerator StartRecommendTimer()
    {
        float timer = csSingleton.Instance.fRecommendTimer;

        if (timer <= 0f) yield break;

        while (timer > 0f)
        {

            yield return new WaitForSeconds(1f);
            timer--;
        }
        ShowRandomScreenBasedOnMissionStatus();
    }
        private void ShowRandomScreenBasedOnMissionStatus()
    {
        bool isMissionAvailable = csMissionManager.Instance.E_missonStatus == MissionStatus.None;
        bool isInArboreteum = IsInsideArboretum();

        int rand = UnityEngine.Random.Range(0, 2);
        // 0 = Mission
        // 1 = Quiz

        if (rand == 0 && isMissionAvailable && isInArboreteum)
        {
            // 미션 가능 & 미션 선택됨
            PopupMission(true);
        }
        else
        {
            // 미션 불가능 → 무조건 퀴즈
            // 선택이 퀴즈였어도 정상 실행
            PopupQuizScreen(true);
        }
    }

    // /// <summary>
    /// UI 오픈 시 뒤로가기 Action을 등록  
    /// 이 때 key는 보통 UI Script의 this
    /// </summary>
    public void Push(object key, UnityAction action)
    {
        backStack.Push(new BackStackEntry { key = key, action = action });
    }

    /// <summary>
    /// 뒤로가기 버튼 동작
    /// </summary>
    public void ExecuteBack()
    {
        Debug.Log("Execute Back Button");

        if (backStack.Count > 0)
        {
            backStack.Pop()?.action?.Invoke();
            return;
        }
        // 스택이 비어있고 현재 mianScreen일 때 종료 팝업 띄우기
        if (currentScreen == mainScreen.gameObject)
        {
            Debug.Log("PopUpQuit");

            csPopupPanel.Instance.PopupQuitApplication(csUIManager.Instance.QuitApplication);
        }
    }

    /// <summary>
    /// 해당 UI(key)의 등록된 Action을 스택에서 삭제  
    /// (뒤로가기 말고 다른 버튼으로 닫을 때 사용)
    /// </summary>
    public void Remove(object key)
    {
        if (backStack.Count == 0)
            return;

        Stack<BackStackEntry> temp = new Stack<BackStackEntry>();

        // key가 아닌 것만 임시 스택에 저장
        while (backStack.Count > 0)
        {
            var entry = backStack.Pop();
            if (!entry.key.Equals(key))
                temp.Push(entry);
        }

        // 다시 backStack 순서 그대로 복원
        while (temp.Count > 0)
        {
            backStack.Push(temp.Pop());
        }
    }

    public void BlockBackButton(bool block)
    {
        bBlocked = block;
    }

    public void QuitApplication()
    {
        
       csSaveLodeManager.Instance.SaveSet();

        Application.Quit();
           
    }


    //AIMainText스킵을 사용할거면 사용
    private void RegisterSkip(Action onSkip)
    {
        skipButton.onClick.AddListener(onSkip.Invoke);
    }

    private void UnregisterSkip(Action onSkip)
    {
        skipButton.onClick.RemoveListener(onSkip.Invoke);
    }

}

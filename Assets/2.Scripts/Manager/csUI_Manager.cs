using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csUI_Manager : MonoBehaviour
{
    public static csUI_Manager Instance { get { return _Instance; } }
    private static csUI_Manager _Instance;

    public GameObject startScreen; // 시작화면
    public GameObject mainScreen; // 메인화면
    public GameObject mapScreen; // 지도 
    public GameObject missionPopup; // 미션창
    public GameObject speechToTextScreen; // 음성 언어 입력 화면
    public GameObject quizScreen; // 퀴즈창
    public GameObject settingScreen; // 설정창

    private GameObject currentScreen;
    private GameObject currentPanel;


    //[SerializeField] private Button skipButton;
    [Header("SpeechToText")]
    public TextMeshProUGUI mainScreenAIText;   // 메인 화면 텍스트
    private Coroutine typingRoutine;
    private bool isTyping = false;

    [Header("AutoRecommend")]
    private bool isInMainScreen;
    private Coroutine timerRoutine;




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
            
        }
        else
        {
            popup.SetActive(false);
            if (returnToMain)
                SetIsInMainScreen(true);
        }
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
        if (!IsInsideArboretum())
        {
            NotInsideInArboretum();
            return;
        }

        TogglePopup(missionPopup, show);
    }

    public void PopupQuizScreen(bool show)
    {
        TogglePopup(quizScreen, show);
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
        //Action skipAction = () => { isSkipped = true; };
        //RegisterSkip(skipAction);

        foreach (char c in text)
        {
            if (isSkipped)
                break;

            mainScreenAIText.text += c;
            yield return new WaitForSeconds(delay);
        }

        // 스킵 시 전체 텍스트 즉시 표시
        mainScreenAIText.text = text;

        //UnregisterSkip(skipAction);
        isTyping = false;
        typingRoutine = null;
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


    // AIMainText스킵을 사용할거면 사용
    //private void RegisterSkip(Action onSkip)
    //{
    //    skipButton.onClick.AddListener(onSkip.Invoke);
    //}

    //private void UnregisterSkip(Action onSkip)
    //{
    //    skipButton.onClick.RemoveListener(onSkip.Invoke);
    //}

}

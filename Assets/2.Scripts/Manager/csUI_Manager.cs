using System;
using System.Collections;
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


    private GameObject currentScreen;
    private GameObject currentPanel;


    //[SerializeField] private Button skipButton;
    public TextMeshProUGUI mainScreenAIText;   // 메인 화면 텍스트
    private Coroutine typingRoutine;
    private bool isTyping = false;

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

    // 지도 나타내기
    public void PopupMap(bool bShow)
    {
        mapScreen.SetActive(bShow);
    }

    // 미션창 나타내기

    public void PopupMission(bool bShow)
    {
        missionPopup.SetActive(bShow);
    }

    public void PopupSpeechToText(bool bShow)
    {
        speechToTextScreen.SetActive(bShow);
    }

    public void PopupQuizScreen(bool bShow)
    {
        quizScreen.SetActive(bShow);
    }


    public void SetAIChatText(string text)
    {
        // 이전 타이핑 코루틴이 있으면 중지
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeTextRoutine(text));
    }


    private IEnumerator TypeTextRoutine(string text)
    {
        isTyping = true;
        mainScreenAIText.text = "";

        float delay = 0.1f;

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
    

    //private void RegisterSkip(Action onSkip)
    //{
    //    skipButton.onClick.AddListener(onSkip.Invoke);
    //}

    //private void UnregisterSkip(Action onSkip)
    //{
    //    skipButton.onClick.RemoveListener(onSkip.Invoke);
    //}

}

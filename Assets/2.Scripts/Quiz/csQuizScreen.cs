using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class csQuizScreen : MonoBehaviour
{
    [Header("Buttons")]
    // 객관식 보기 버튼
    [SerializeField] private List<Button> choiceButtons;
    // O/X 선택 버튼
    [SerializeField] private List<Button> findRightButtons;



    [SerializeField] private Button answerSubmitButton;
    [SerializeField] private Button resetQuizButton;
    [SerializeField] private Button endQuizButton;
    [SerializeField] private Button closeQuizScreenButton;
    [SerializeField] private Button nextQuizButton;
    [SerializeField] private Button stampTourClearButton;


    // 현재 퀴즈 타입
    [SerializeField] private QuizDataWrapper quizDataWrapper; // 현재 가지고있는 퀴즈데이터

    // 퀴즈 인덱스
    private int currentQuizIndex = -1;

    // 현재 퀴즈 생성 타입
    [SerializeField] public QuizGenerationType currentQuizGenerationType;
    // 현재 선택한 정답
    private int userSelectQuizAnswer = -1;

    [Header("BottomButtons")]
    [SerializeField] private GameObject onQuizObject; // 퀴즈중일 때 활성화할 오브젝트 ( 정답선택 버튼)
    [SerializeField] private GameObject endQuizObject; // 퀴즈가 끝났을 때 활성화할 오브젝트 ( 종료, 다음퀴즈 버튼)

    [Header("BodyParts")]
    [SerializeField] private GameObject multipleChoicePart;
    [SerializeField] private GameObject findRightPart;
    [SerializeField] private TextMeshProUGUI quizText_TMP;

    [Header("MultipleChoice")]
    [SerializeField] private List<TextMeshProUGUI> choiceTMP; // 보기 텍스트

    [Header("FindRight")]
    [SerializeField] private GameObject findrightSelectObject;
    [SerializeField] private RectTransform resultHolder;
    [SerializeField] private GameObject correctResultPrefab;
    [SerializeField] private GameObject inCorrectResultPrefab;

    [Header("Answer Spirte")]
    [SerializeField] private Sprite unSelectedSprite; //선택안됨
    [SerializeField] private Sprite SelectedSprite;   // 선택됨
    [SerializeField] private Sprite CorrectSprite;    // 정답
    [SerializeField] private Sprite InCorrectSprite;  // 오답

    private bool bOnQuiz = true;

    private void OnEnable()
    {
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            int index = i;
            choiceButtons[i].onClick.AddListener(() => SelectChoice(index));
        }

        for (int i = 0; i < findRightButtons.Count; i++)
        {
            int index = i;
            findRightButtons[i].onClick.AddListener(() => SelectChoice(index));
        }
        answerSubmitButton.onClick.AddListener(SubmitAnswer);
        resetQuizButton.onClick.AddListener(PopupResetQuiz);
        endQuizButton.onClick.AddListener(QuitQuiz);
        closeQuizScreenButton.onClick.AddListener(QuitQuiz);
        nextQuizButton.onClick.AddListener(() => SetQuiz());
        stampTourClearButton.onClick.AddListener(OnStampTourClear);

        SetQuiz();

    }

    private void OnDisable()
    {
        foreach (Button choiceButton in choiceButtons)
        {
            choiceButton.onClick.RemoveAllListeners();

        }
        foreach (Button findRightButton in findRightButtons)
        {
            findRightButton.onClick.RemoveAllListeners();
        }

        answerSubmitButton.onClick.RemoveAllListeners();
        resetQuizButton.onClick.RemoveAllListeners();
        endQuizButton.onClick.RemoveAllListeners();
        closeQuizScreenButton.onClick.RemoveAllListeners();
        nextQuizButton.onClick.RemoveAllListeners();
        stampTourClearButton.onClick.RemoveAllListeners();
    }

    private void OnStampTourClear()
    {
        csUIManager.Instance.PopupQuizScreen(false);
        csStampTourManager.Instance.SetStampTourClearUI();
    }

    private void SetQuiz()
    {
        if (quizDataWrapper != null)
        {
            // 퀴즈 데이터 있으면 초기화
            quizDataWrapper = new QuizDataWrapper();
        }

        answerSubmitButton.interactable = false;

        userSelectQuizAnswer = -1;

        // 퀴즈데이터 가져오기
        switch (currentQuizGenerationType)
        {
            case QuizGenerationType.ObserveQuiz:
                {
                    quizDataWrapper = GetRandomObserveQuiz();
                    break;
                }
            case QuizGenerationType.StampTourQuiz:
                {
                    QuizData stampTourQuizData = GetRandomStampTourQuiz();
                    quizDataWrapper = new QuizDataWrapper
                    {
                        quizData = stampTourQuizData,
                        IsSolvedQuestion = false,
                        plantScientificName = ""
                    };
                    break;
                }
        }



        // 퀴즈 보기 text 설정
        if (quizDataWrapper.quizData.quizChoices != null)
        {

            for (int i = 0; i < quizDataWrapper.quizData.quizChoices.Count; ++i)
            {

                {
                    choiceTMP[i].text = quizDataWrapper.quizData.quizChoices[i];
                }
            }
        }

        quizText_TMP.text = "Q." + quizDataWrapper.quizData.quizDescription;

        SetBodyPart();

        // 퀴즈 하단 버튼 UI변경
        SetOnQuizUI(true);
    }

    // 하단 버튼 UI 표시 설정
    private void SetOnQuizUI(bool isOnQuiz, bool isCorrect = false)
    {
        onQuizObject.gameObject.SetActive(isOnQuiz);
        endQuizObject.gameObject.SetActive(!isOnQuiz);

        bOnQuiz = isOnQuiz;

        if (!isOnQuiz)
        {
            endQuizButton.gameObject.SetActive(false);
            nextQuizButton.gameObject.SetActive(false);
            stampTourClearButton.gameObject.SetActive(false);

            if (currentQuizGenerationType == QuizGenerationType.ObserveQuiz)
            {
                endQuizButton.gameObject.SetActive(true);
                endQuizButton.GetComponentInChildren<TextMeshProUGUI>().text = "종료";
                nextQuizButton.gameObject.SetActive(true);
                nextQuizButton.GetComponentInChildren<TextMeshProUGUI>().text = "다음퀴즈";
                nextQuizButton.onClick.RemoveAllListeners();
                nextQuizButton.onClick.AddListener(() => SetQuiz());
            }
            else
            {
                if (isCorrect)
                {
                    stampTourClearButton.gameObject.SetActive(true);
                }
                else
                {
                    endQuizButton.gameObject.SetActive(true);
                    endQuizButton.GetComponentInChildren<TextMeshProUGUI>().text = "종료";
                    nextQuizButton.gameObject.SetActive(true);
                    nextQuizButton.GetComponentInChildren<TextMeshProUGUI>().text = "다시시도";
                    nextQuizButton.onClick.RemoveAllListeners();
                    nextQuizButton.onClick.AddListener(() => SetQuiz());
                }
            }
        }
    }

    public void SelectChoice(int index)
    {
        if (!bOnQuiz) return;

        switch (quizDataWrapper.quizData.quizType)
        {
            case QuizType.None:
                {
                    Debug.Log("No QuizType ");
                    break;
                }
            case QuizType.MultipleChoice:
                {
                    for (int i = 0; i < choiceButtons.Count; ++i)
                    {
                        if (i == index)
                        {
                            choiceButtons[i].GetComponent<Image>().sprite = SelectedSprite;
                        }
                        else
                        {
                            choiceButtons[i].GetComponent<Image>().sprite = unSelectedSprite;
                        }
                    }
                    break;
                }
            case QuizType.FindRight:
                {
                    for (int i = 0; i < findRightButtons.Count; ++i)
                    {
                        if (i == index)
                        {
                            findRightButtons[i].GetComponent<Image>().sprite = SelectedSprite;
                        }
                        else
                        {
                            findRightButtons[i].GetComponent<Image>().sprite = unSelectedSprite;

                        }
                    }
                    break;
                }

        }
        userSelectQuizAnswer = index + 1; // index는 0부터 시작이니 정답은 +1

        answerSubmitButton.interactable = true;

    }

    private void SubmitAnswer()
    {
        if (userSelectQuizAnswer == -1)
        {
            Debug.Log("No selectedChoice ");
            return;
        }

        bool isCorrect = quizDataWrapper.quizData.answer == userSelectQuizAnswer;
        SetOnQuizUI(false, isCorrect);

        switch (quizDataWrapper.quizData.quizType)
        {
            case QuizType.None:
                {
                    Debug.Log("No QuizType ");
                    break;
                }
            case QuizType.MultipleChoice:
                {
                    for (int i = 0; i < choiceButtons.Count; ++i)
                    {
                        if (isCorrect)
                        {
                            RewardAndSaveQuizData();

                            choiceButtons[quizDataWrapper.quizData.answer - 1].GetComponent<Image>().sprite = CorrectSprite;
                        }
                        else
                        {
                            choiceButtons[quizDataWrapper.quizData.answer - 1].GetComponent<Image>().sprite = InCorrectSprite;
                        }
                    }
                    break;
                }
            case QuizType.FindRight:
                {
                    if (isCorrect)
                    {
                        RewardAndSaveQuizData();

                        findrightSelectObject.SetActive(false);

                        Instantiate(correctResultPrefab, resultHolder, false);


                    }
                    else
                    {
                        findrightSelectObject.SetActive(false);

                        Instantiate(inCorrectResultPrefab, resultHolder, false);

                    }
                    break;
                }
        }
    }

    private void QuitQuiz()
    {
        csUIManager.Instance.PopupQuizScreen(false);

        csUIManager.Instance.ResetAIChatText();


    }


    private void SetBodyPart()
    {
        switch (quizDataWrapper.quizData.quizType)
        {
            case QuizType.None:
                {
                    Debug.Log("No QuizType ");
                    break;
                }
            case QuizType.MultipleChoice:
                {
                    multipleChoicePart.SetActive(true);
                    findRightPart.SetActive(false);

                    // 선택,정답 이미지 비활성화
                    foreach (Button choiceButton in choiceButtons)
                    {
                        choiceButton.GetComponent<Image>().sprite = unSelectedSprite;
                    }
                    break;
                }
            case QuizType.FindRight:
                {
                    multipleChoicePart.SetActive(false);
                    findRightPart.SetActive(true);
                    findrightSelectObject.SetActive(true);
                    // 버튼 색 회색으로 초기화
                    foreach (Button findRightButton in findRightButtons)
                    {
                        findRightButton.GetComponent<Image>().sprite = unSelectedSprite;
                    }
                    foreach (Transform child in resultHolder)
                    {
                        Destroy(child.gameObject);
                    }

                    break;
                }
        }
    }

    // 퀴즈 데이터 저장 및 포인트 지급
    private void RewardAndSaveQuizData()
    {
        if (quizDataWrapper.IsSolvedQuestion == false && currentQuizGenerationType == QuizGenerationType.ObserveQuiz)
        {
            quizDataWrapper.IsSolvedQuestion = true; // 맞춘 문제로 설정

            csSingleton.Instance.savedQuizList.quizDataWrapperList[currentQuizIndex] = quizDataWrapper; // 변경된 값 저장

            csSaveLodeManager.Instance.SaveQuizData(); // 퀴즈 데이터 저장

            csSingleton.Instance.RewardPoint(10); // 정답 맞추면 포인트 10점 지급

        }
    }

    private void PopupResetQuiz()
    {
        csPopupPanel.Instance.PopupResetQuiz(SetQuiz);
    }

    // 랜덤한 퀴즈를 추출( 안 풀었던 문제 먼저 )
    public QuizDataWrapper GetRandomObserveQuiz()
    {
        QuizDataWrapperList quizDataWrapperList = csSingleton.Instance.savedQuizList;

        var unsolvedList = quizDataWrapperList.quizDataWrapperList
               .Where(q => !q.IsSolvedQuestion)
               .ToList();

        // 타입을 List<QuizDataWrapper> 로 변경
        List<QuizDataWrapper> targetList =
            (unsolvedList.Count > 0) ? unsolvedList : quizDataWrapperList.quizDataWrapperList;

        currentQuizIndex = UnityEngine.Random.Range(0, targetList.Count);
        return targetList[currentQuizIndex];
    }

    public QuizData GetRandomStampTourQuiz()
    {
        int id = csStampTourManager.Instance.currentTourLocationData.locationID;
        List<QuizData> quizList = LoadStampTourQuizByLocation(id);

        int randomIndex = UnityEngine.Random.Range(0, quizList.Count);

        return quizList[randomIndex];
    }

    public List<QuizData> LoadStampTourQuizByLocation(int locationId)
    {
        // CSV 로드
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/StampTourQuiz/{locationId}");

        // CSV 없으면 기본 문제 1개 만들어서 반환
        if (csvFile == null)
        {
            Debug.LogWarning($"⚠ StampTourQuiz 파일 없음 → 기본 문제 생성 | locationId: {locationId}");

            List<QuizData> defaultQuiz = new List<QuizData>();

            defaultQuiz.Add(new QuizData
            {
                quizType = QuizType.MultipleChoice,
                answer = 1,
                quizDescription = "장미의 꽃말 중 하나는 무엇일까요?",
                quizChoices = new List<string> { "질투", "용기", "순수", "감사" }
            });

            return defaultQuiz;
        }

        // CSV 존재하면 리스트로 파싱
        List<QuizData> quizList = new List<QuizData>();
        string[] lines = csvFile.text.Split('\n');

        // 🔥 첫 번째 라인(헤더) 스킵하기 위해 i = 1부터
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');

            if (parts.Length < 4)
                continue;

            QuizType quizType = (QuizType)Enum.Parse(typeof(QuizType), parts[0]);
            int answerIndex = int.Parse(parts[1]);
            string description = parts[2];
            List<string> choices = parts[3].Split('|').ToList();

            quizList.Add(new QuizData
            {
                quizType = quizType,
                answer = answerIndex,
                quizDescription = description,
                quizChoices = choices
            });
        }

        return quizList;
    }

}
using Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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


    // 현재 퀴즈 타입
    [SerializeField] private QuizData quizData; // 현재 가지고있는 퀴즈데이터
    // 현재 선택한 정답
    private int userSelectQuizAnswer =-1;

    [Header("BottomButtons")]
    [SerializeField] private GameObject onQuizObject; // 퀴즈중일 때 활성화할 오브젝트 ( 정답선택 버튼)
    [SerializeField] private GameObject endQuizObject; // 퀴즈가 끝났을 때 활성화할 오브젝트 ( 종료, 다음퀴즈 버튼)

    [Header("BodyParts")]
    [SerializeField] private GameObject multipleChoicePart;
    [SerializeField] private GameObject findRightPart;
    [SerializeField] private TextMeshProUGUI quizText_TMP;

    [Header("MultipleChoice")]
    [SerializeField] private List<TextMeshProUGUI>  choiceTMP; // 보기 텍스트

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
        for(int i = 0; i < choiceButtons.Count; i++)
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
        nextQuizButton.onClick.AddListener(SetQuiz);

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
    }

    private void SetQuiz()
    {


        if (quizData != null)
        {
            // 퀴즈 데이터 있으면 초기화
            quizData = new QuizData();
        }

        answerSubmitButton.interactable = false;

        userSelectQuizAnswer = -1;

        // 퀴즈데이터 가져오기
        quizData = GetRandomQuiz();// 테스트 퀴즈   

        // 퀴즈 보기 text 설정
        if (quizData.quizChoices != null)
        {

            for (int i = 0; i < quizData.quizChoices.Count; ++i)
            {

                {
                    choiceTMP[i].text = quizData.quizChoices[i];
                }
            }
        }

        quizText_TMP.text = "Q." + quizData.quizDescription;

        SetBodyPart();

        // 퀴즈 하단 버튼 UI변경
        SetOnQuizUI(true);
    }

    // 하단 버튼 UI 표시 설정
    private void SetOnQuizUI(bool isOnQuiz)
    {
        onQuizObject.gameObject.SetActive(isOnQuiz); // 퀴즈 중일 때 정답 선택버튼 활성화
        endQuizObject.gameObject.SetActive(!isOnQuiz); // 퀴즈 끝나고 종료, 다음퀴즈 버튼 활성화

        bOnQuiz = isOnQuiz;
    }

    public void SelectChoice(int index)
    {
        if (!bOnQuiz) return;

        switch (quizData.quizType)
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
        userSelectQuizAnswer = index+1; // index는 0부터 시작이니 정답은 +1

        answerSubmitButton.interactable = true;

    }

    private void SubmitAnswer()
    {
        if(userSelectQuizAnswer == -1)
        {
            Debug.Log("No selectedChoice ");
            return;
        }

        SetOnQuizUI(false);

        switch (quizData.quizType)
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
                        // 사용자가 선택한 정답과 퀴즈의 정답이 같으면 정답 스프라이트로 변경
                        if(quizData.answer == userSelectQuizAnswer)
                        {
                            choiceButtons[quizData.answer-1].GetComponent<Image>().sprite = CorrectSprite;
                        }
                        // 다르면 기존 선택 스프라이트는 냅두고 퀴즈의 정답만 오답 스프라이트로변경
                        else
                        {
                            choiceButtons[quizData.answer-1].GetComponent<Image>().sprite = InCorrectSprite;
                        }
                    }
                    break;
                }
            case QuizType.FindRight:
                {
                    if(quizData.answer == userSelectQuizAnswer)
                    {
                        findrightSelectObject.SetActive(false);

                        Instantiate(correctResultPrefab, resultHolder,false);
                    }
                    else
                    {
                        findrightSelectObject.SetActive(false);

                        Instantiate(inCorrectResultPrefab, resultHolder, false);

                    }
                    break;
                }
        }

        // 퀴즈 정답에 대한 설명 텍스트 추가
        //quizText_TMP = 
    }
    
    private void QuitQuiz()
    {
        csUIManager.Instance.PopupQuizScreen(false);

        csUIManager.Instance.ResetAIChatText();

        
    }


    private void SetBodyPart()
    {
        switch(quizData.quizType)
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
                    foreach(Transform child  in resultHolder)
                    {
                        Destroy(child.gameObject);
                    }

                    break;
                }
        }
    }

    private void PopupResetQuiz()
    {
        csPopupPanel.Instance.PopupResetQuiz(SetQuiz);
    }

    public QuizData GetRandomQuiz()
    {
        QuizData testQuiz1 = new QuizData
        {
            quizType = QuizType.MultipleChoice,
            answer = 1,
            quizDescription = "장미의 꽃말 중 하나는 무엇일까요?",
            quizChoices = new List<string>
        {
            "질투",
            "용기",
            "순수",
            "감사"
        }
        };

        QuizData testQuiz2 = new QuizData
        {
            quizType = QuizType.MultipleChoice,
            answer = 2,
            quizDescription = "해바라기가 태양을 따라 고개를 돌리는 현상을 무엇이라고 할까요?",
            quizChoices = new List<string>
        {
            "광합성",
            "주광성",
            "음지성",
            "반사광"
        }
        };

        QuizData testQuiz3 = new QuizData
        {
            quizType = QuizType.MultipleChoice,
            answer = 1,
            quizDescription = "벚꽃이 가장 화려하게 피는 계절은?",
            quizChoices = new List<string>
        {
            "봄",
            "여름",
            "가을",
            "겨울"
        }
        };

        QuizData[] quizPool = { testQuiz1, testQuiz2, testQuiz3 };
        return quizPool[UnityEngine.Random.Range(0, quizPool.Length)];
    }

}

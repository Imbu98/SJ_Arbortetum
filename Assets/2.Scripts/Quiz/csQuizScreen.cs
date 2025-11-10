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

    [Header("MultipleChoice")]
    [SerializeField] private List<TextMeshProUGUI>  choiceTMP; // 보기 텍스트

    [Header("FindRight")]
    [SerializeField] private RectTransform correctResultHolder;
    [SerializeField] private RectTransform IncorrectResultHolder;

    [SerializeField] private GameObject correctResultPrefab;
    [SerializeField] private GameObject inCorrectResultPrefab;

    [SerializeField] private Color unSelectedColor; // O/X 버튼 비선택 색깔
    [SerializeField] private Color selectedColor; // O/X 버튼 선택 색깔
    [SerializeField] private Color inCorrectColor; // O/X 오답 배경 이미지
    [SerializeField] private Color correctColor; // O/X 정답 배경 이미지


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
        resetQuizButton.onClick.AddListener(SetQuiz);
        endQuizButton.onClick.AddListener(QuitQuiz);
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
        nextQuizButton.onClick.RemoveAllListeners();
    }

    public void SelectChoice(int index)
    {
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
                            // 보기를 골랐으면 선택 이미지 활성화
                            choiceButtons[i].transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            // 나머지 선택 이미지는 비활성화
                            choiceButtons[i].transform.GetChild(0).gameObject.SetActive(false);
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
                            findRightButtons[i].GetComponent<Image>().color = selectedColor;
                        }
                        else
                        {
                            findRightButtons[i].GetComponent<Image>().color = unSelectedColor;
                            
                        }
                    }
                    break;
                }

        }
        userSelectQuizAnswer = index+1; // index는 0부터 시작이니 정답은 +1

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
                        if (i+1 == quizData.answer)
                        {
                            // 퀴즈의 정답과 같은 index의 버튼의 정답 이미지 활성화
                            choiceButtons[i].transform.GetChild(1).gameObject.SetActive(true);
                        }
                        else
                        {
                            // 나머지 정답 이미지 비활성화
                            choiceButtons[i].transform.GetChild(1).gameObject.SetActive(false);
                        }
                    }
                    break;
                }
            case QuizType.FindRight:
                {
                    if(quizData.answer == userSelectQuizAnswer)
                    {
                        findRightButtons[userSelectQuizAnswer-1].GetComponent<Image>().color = correctColor;
                        Instantiate(correctResultPrefab, findRightButtons[userSelectQuizAnswer-1].transform.GetChild(0).transform);
                    }
                    else
                    {
                        findRightButtons[userSelectQuizAnswer-1].GetComponent<Image>().color = inCorrectColor;
                        Instantiate(inCorrectResultPrefab, findRightButtons[userSelectQuizAnswer-1].transform.GetChild(0).transform);
                    }
                    break;
                }

        }
    }

       
    
    private void QuitQuiz()
    {
        this.gameObject.SetActive(false);

        csUI_Manager.Instance.ResetAIChatText();
    }

    private void SetQuiz()
    {
        if (quizData!=null)
        {
            // 퀴즈 데이터 있으면 초기화
            quizData = new QuizData();
        }
        userSelectQuizAnswer = -1;

        // 퀴즈데이터 가져오기
        quizData = GetRandomQuiz();// 테스트 퀴즈   

        // 퀴즈 보기 text 설정
        if (quizData.quizChoices!=null)
        {
        
            for (int i = 0; i < quizData.quizChoices.Count; ++i)
            {

                {
                    choiceTMP[i].text = quizData.quizChoices[i];
                }
            }
        }
        csUI_Manager.Instance.SetAIChatText(quizData.quizDescription);

        SetBodyPart();

        // 퀴즈 하단 버튼 UI변경
        SetOnQuizUI(true);
    }

    // 하단 버튼 UI 표시 설정
    private void SetOnQuizUI(bool IsOnQuiz)
    {
        onQuizObject.gameObject.SetActive(IsOnQuiz); // 퀴즈 중일 때 정답 선택버튼 활성화
        endQuizObject.gameObject.SetActive(!IsOnQuiz); // 퀴즈 끝나고 종료, 다음퀴즈 버튼 활성화
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
                        choiceButton.transform.GetChild(0).gameObject.SetActive(false);
                        choiceButton.transform.GetChild(1).gameObject.SetActive(false);
                    }
                    break;
                }
            case QuizType.FindRight:
                {
                    multipleChoicePart.SetActive(false);
                    findRightPart.SetActive(true);

                    // 버튼 색 회색으로 초기화
                    foreach (Button findRightButton in findRightButtons)
                    {
                        findRightButton.transform.GetComponent<Image>().color = Color.gray;
                    }
                    // 정답 결과 삭제
                    foreach(Transform child in correctResultHolder)
                    {
                        Destroy(child.gameObject);
                    }
                    // 정답 결과 삭제
                    foreach (Transform child in IncorrectResultHolder)
                    {
                        Destroy(child.gameObject);
                    }
                    break;
                }
        }
    }


    public QuizData GetRandomQuiz()
    {
        QuizData testQuiz = new QuizData
        {
            quizType = QuizType.MultipleChoice,
            answer = 2,
            quizDescription = "다음 중 대한민국의 수도는 무엇인가?",
            quizChoices = new List<string>
        {
            "부산",
            "서울",
            "인천",
            "대전"
        }
        };

        QuizData oxQuiz = new QuizData
        {
            quizType = QuizType.FindRight,
            answer = 1,
            quizDescription = "지구는 태양을 중심으로 공전한다.",
        };

        QuizData[] quizPool = { testQuiz, oxQuiz };
        return quizPool[UnityEngine.Random.Range(0, quizPool.Length)];
    }

}

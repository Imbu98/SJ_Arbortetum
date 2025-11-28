using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csQnA_Screen : MonoBehaviour
{
    [SerializeField] private List<Button> selectQnA_Buttons;

    [SerializeField] private Button selectQnA_Button;

    private int currentIndex = -1; // 현재 선택된 버튼 인덱스
    [Header("Button Colors")]
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Color selectedTextColor;
    [SerializeField] private Sprite unSelectedSprite;
    [SerializeField] private Color unSelectedTextColor;

    private List<string> koQnA_List = new List<string>();

    private List<string> enQnA_List = new List<string>();
    private void Start()
    {
        SetQnAList();
    }
    private void OnEnable()
    {
        // 미션 생성 버튼 비활성화
        MissonCreateButtonInteractable(false);

        // 버튼 클릭 리스너 등록
        for (int i = 0; i < selectQnA_Buttons.Count; i++)
        {
            int index = i; // 클로저 문제 방지
            selectQnA_Buttons[i].onClick.AddListener(() => OnSelectButtonClicked(index));
            // UI초기화
            selectQnA_Buttons[i].image.sprite = unSelectedSprite;
            selectQnA_Buttons[i].GetComponentInChildren<TextMeshProUGUI>().color = unSelectedTextColor;
        }

        selectQnA_Button.onClick.AddListener(OnClickQnAButton);
    }

    private void OnDisable()
    {
        // 리스너 해제
        foreach (var btn in selectQnA_Buttons)
        {
            btn.onClick.RemoveAllListeners();
        }

        selectQnA_Button.onClick.RemoveAllListeners();

        currentIndex = -1; // 인덱스 초기화
    }

    private void OnSelectButtonClicked(int index)
    {
        // 같은 버튼 다시 누르면 유지
        if (currentIndex == index)
            return;

        currentIndex = index;

        UpDateButtonUI(index);

        MissonCreateButtonInteractable(true);
    }

    private void UpDateButtonUI(int index)
    {
        for (int i = 0; i < selectQnA_Buttons.Count; i++)
        {
            if (i == index)
            {
                selectQnA_Buttons[i].image.sprite = selectedSprite;
                selectQnA_Buttons[i].GetComponentInChildren<TextMeshProUGUI>().color = selectedTextColor;
            }
            else
            {
                selectQnA_Buttons[i].image.sprite = unSelectedSprite;
                selectQnA_Buttons[i].GetComponentInChildren<TextMeshProUGUI>().color = unSelectedTextColor;
            }
        }

    }
    private void MissonCreateButtonInteractable(bool IsInteractable)
    {
        selectQnA_Button.interactable = IsInteractable;
    }

    private void SetQnAList()
    {
        TextAsset QnA_csvFile = Resources.Load<TextAsset>("CSV/QnA");
        if (QnA_csvFile == null)
        {
            Debug.LogError("❌ QnA.csv 파일을 찾을 수 없습니다.");
            return;
        }

        // CSV 내용 읽기
        string[] lines = QnA_csvFile.text.Split('\n');

        // 첫 번째 줄(헤더)은 제외하므로 1부터 시작
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            // 🔥 여기서 " , " 처리 가능한 SplitCsvLine 함수 사용
            string[] columns = SplitCsvLine(line);

            if (columns.Length < 2)
            {
                Debug.LogWarning($"⚠ CSV 형식 오류: {line}");
                continue;
            }

            string korean = columns[0].Trim('"').Trim();
            string english = columns[1].Trim('"').Trim();

            koQnA_List.Add(korean);
            enQnA_List.Add(english);
        }
    }
    public static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool insideQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current.Trim());
        return result.ToArray();
    }

    private void OnClickQnAButton()
    {
        string selectedQnA = string.Empty;
        if (csSingleton.Instance.nLanguage == 0) // 한국어
        {
            selectedQnA = koQnA_List[currentIndex];
        }
        else // 영어
        {
            selectedQnA = enQnA_List[currentIndex];
        }
        csUIManager.Instance.SetAIChatText(selectedQnA);

        csUIManager.Instance.PopupQnA_Screen(false);
    }
}

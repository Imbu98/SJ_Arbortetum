using Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class csSpeechPanel : MonoBehaviour
{
    // SpeechToTextScreen을 여는 버튼
    [SerializeField] private Button startSpeechToTextButton;

    // 서버에서 날라온 ai챗봇의 대화 내용
    [SerializeField] private TextMeshProUGUI aIText_TMP;

    private TouchScreenKeyboard keyboard;
    

    private void Update()
    {
        if (keyboard != null)
        {
            if (keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                // 입력 완료 처리
                OnInputEndEdit(keyboard.text);
                keyboard = null;
                startSpeechToTextButton.gameObject.SetActive(false);
            }
            else if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                // 취소 처리
                keyboard = null;
                startSpeechToTextButton.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        startSpeechToTextButton.onClick.AddListener(OpenSpeechToTextScreen);

        csUI_Manager.Instance.ResetAIChatText();
    }

    private void OnDisable()
    {
        startSpeechToTextButton.onClick.RemoveAllListeners();
    }

    // 대화하기 버튼 클릭 시 키보드와 음성 대화 버튼 등장
    public void OnOpenKeyboardButtonClicked()
    {
#if !UNITY_EDITOR
        // 키보드 열기
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);

        TouchScreenKeyboard.hideInput = false;

        startSpeechToTextButton.gameObject.SetActive(true);
#endif

    }

    // 입력 완료 이벤트
    public void OnInputEndEdit(string text)
    {
        Debug.Log("입력 완료: " + text);

        startSpeechToTextButton.gameObject.SetActive(false);

        SendResultToSerever(text);
    }

    // 키보드 등장시 같이 등장하는 버튼을 누르면 키패드가 닫히고 SpeechToTextScreen열림
    private void OpenSpeechToTextScreen()
    {
        startSpeechToTextButton.gameObject.SetActive(false);

        csUI_Manager.Instance.PopupSpeechToText(true);

        if (keyboard != null)
        {
            keyboard.active = false;
            keyboard = null;
        }
    }

    public void CloseSpeechToTextScreen()
    {
        csUI_Manager.Instance.PopupSpeechToText(false);
    }

    // 서버에 내용 보내기
    async public void SendResultToSerever(string result)
    {
        csUI_Manager.Instance.PopupSpeechToText(false);

        Debug.Log(result);

        ChatMessage userChatMessage = new ChatMessage
        {
            UID = csSingleton.Instance.UID,
            msg = result
        };

        //List<ChatMessage> tempChatList = new List<ChatMessage>(csSingleton.Instance.strSavedChatHistory);
        //tempChatList.Add(userChatMessage);

        string jsonData = JsonConvert.SerializeObject(userChatMessage);

        string aITextResult = "챗봇의 Test용 Text입니다";//await csNetworkManager.Instance.AsyncGetAIChatResult(jsonData);

        //AI 응답이 null/빈 문자열이면 → 저장하지 않고 return
        if (string.IsNullOrEmpty(aITextResult))
        {
            Debug.LogWarning("AI 응답이 null 이므로 User 메시지를 저장하지 않습니다.");
            return;
        }

        csUI_Manager.Instance.SetAIChatText(aITextResult); // 메인화면의 text에 result 내용을 적기

        
        //// 응답이 왔으면 user text 먼저 저장
        //csSingleton.Instance.strSavedChatHistory.Add(userChatMessage);

        //ChatMessage aIChatMessage = new ChatMessage
        //{
        //    role = "AI",
        //    msg = aITextResult
        //};
        //// 이후 챗봇 Text 내역 저장
        //csSingleton.Instance.strSavedChatHistory.Add(aIChatMessage);
        
        //csSaveLodeManager.Instance.SaveChatHistory();
    }
}

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
    private void OnInputEndEdit(string text)
    {
        Debug.Log("입력 완료: " + text);

        startSpeechToTextButton.gameObject.SetActive(false);

        SendResultToSerever(text);
    }

    // 키보드 등장시 같이 등장하는 버튼을 누르면 키패드가 닫히고 SpeechToTextScreen열림
    private void OpenSpeechToTextScreen()
    {
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
    public void SendResultToSerever(string result)
    {
        csUI_Manager.Instance.PopupSpeechToText(false);

        Debug.Log(result);

        // 임시
        aIText_TMP.text = result;
        // 서버에서 챗봇 대화내용 불러오기
        // aIText_TMP.text = await~~

    }
}

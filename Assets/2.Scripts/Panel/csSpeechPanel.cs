using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class csSpeechPanel : MonoBehaviour
{
    public Button startSpeechToTextButton;
    private TouchScreenKeyboard keyboard;

    [SerializeField] private Button speechButton;

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
        speechButton?.onClick.AddListener(OnOpenKeyboardButtonClicked);
        startSpeechToTextButton?.onClick.AddListener(OnStartSpeechToTextButtonClicked);
    }

    private void OnDisable()
    {
        speechButton?.onClick.RemoveListener(OnOpenKeyboardButtonClicked);
        startSpeechToTextButton?.onClick.RemoveListener(OnStartSpeechToTextButtonClicked);
    }

    // 버튼에 연결
    public void OnOpenKeyboardButtonClicked()
    {
        // 키보드 열기
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);

        startSpeechToTextButton.gameObject.SetActive(true);

    }

    // 입력 완료 이벤트
    private void OnInputEndEdit(string text)
    {
        Debug.Log("입력 완료: " + text);

        startSpeechToTextButton.gameObject.SetActive(false);
    }

    private void OnStartSpeechToTextButtonClicked()
    {

    }
}

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
                // �Է� �Ϸ� ó��
                OnInputEndEdit(keyboard.text);
                keyboard = null;
                startSpeechToTextButton.gameObject.SetActive(false);
            }
            else if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                // ��� ó��
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

    // ��ư�� ����
    public void OnOpenKeyboardButtonClicked()
    {
        // Ű���� ����
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);

        startSpeechToTextButton.gameObject.SetActive(true);

    }

    // �Է� �Ϸ� �̺�Ʈ
    private void OnInputEndEdit(string text)
    {
        Debug.Log("�Է� �Ϸ�: " + text);

        startSpeechToTextButton.gameObject.SetActive(false);
    }

    private void OnStartSpeechToTextButtonClicked()
    {

    }
}

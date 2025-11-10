using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csMainScreen : MonoBehaviour
{
    [SerializeField] private GameObject userNickNameInputUI;

    [SerializeField] private TMP_InputField userNickNameInputfield;

    private void Awake()
    {
        // 게임 데이터불러오기(닉네임 등)
        csSaveLodeManager.Instance.Load();

        // 세팅 불러오기
        csSaveLodeManager.Instance.LoadSet();

        // 채팅내역 불러오기
        csSaveLodeManager.Instance.LoadChatHistory();

        // 저장 
        csSaveLodeManager.Instance.LoadSavedPlant();
    }
    private void OnEnable()
    {
        userNickNameInputfield.onValueChanged.AddListener(InputUserNickName);

        if (csSingleton.Instance.strPlayerNickName==string.Empty)
        {
            StartSetUserNickName();
        }
        else
        {

        }
    }

    private void OnDisable()
    {
        userNickNameInputfield.onValueChanged.RemoveAllListeners();
    }

    private void StartSetUserNickName()
    {
        List<string> texts = new List<string>();

        texts.Add(csLocalizationManager.Instance.LocalizationString("Key_Introduce1"));
        texts.Add(csLocalizationManager.Instance.LocalizationString("Key_Introduce2"));

        StartCoroutine(csUI_Manager.Instance.PlayAIChatSequence(texts, () =>
        {
            userNickNameInputUI.gameObject.SetActive(true);
        }));
    }

    private void InputUserNickName(string text)
    {

    }
}

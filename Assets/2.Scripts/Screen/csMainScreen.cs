using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csMainScreen : MonoBehaviour
{
    [SerializeField] private GameObject userNickNameInputUI;

    [SerializeField] private TMP_InputField userNickNameInputfield;

    [SerializeField] private Button confirmUserNickNameButton;

    [SerializeField] private GameObject mainUI;
    [SerializeField] private Animator uiAnimator;



    private void Awake()
    {
      
    }
    private void OnEnable()
    {
        userNickNameInputfield.onValueChanged.AddListener(InputUserNickName);
        confirmUserNickNameButton.onClick.AddListener(SetNickName);

        if (csSingleton.Instance.strPlayerNickName==string.Empty)
        {
            StartSetUserNickName();
        }
        else
        {
            SetMainUI();
        }
    }

    private void OnDisable()
    {
        userNickNameInputfield.onValueChanged.RemoveAllListeners();
        confirmUserNickNameButton.onClick.RemoveAllListeners();
    }

    private void StartSetUserNickName()
    {
        List<string> texts = new List<string>();

        texts.Add(csLocalizationManager.Instance.LocalizationString("Key_Introduce1"));
        texts.Add(csLocalizationManager.Instance.LocalizationString("Key_Introduce2"));

        StartCoroutine(csUIManager.Instance.PlayAIChatSequence(texts, () =>
        {
            userNickNameInputUI.gameObject.SetActive(true);
            uiAnimator.SetBool("IsFinishIntroduce", true);
            confirmUserNickNameButton.interactable = false;
        }));
    }

    private void InputUserNickName(string text)
    {
        bool IsValidNickName = false;

        // 1) ±æÀÌ Á¦ÇÑ (2~16ÀÚ)
        if (text.Length >= 2 || text.Length <= 16)
        {
            IsValidNickName = true;
        }

        // 2) Æ¯¼ö¹®ÀÚ Æ÷ÇÔ ¿©ºÎ °Ë»ç
        //    Çã¿ë ¹®ÀÚ: ÇÑ±Û, ¿µ¾î, ¼ýÀÚ
        //    ±ÝÁö ¹®ÀÚ: Æ¯¼ö¹®ÀÚ ÀüÃ¼
        //    ^ ¡æ ¹®ÀÚ¿­ ½ÃÀÛ, $ ¡æ ¹®ÀÚ¿­ ³¡
        //    [] ¾È¿¡¼­ Çã¿ë ¹®ÀÚ Á¤ÀÇ
        //    {2,16} ¡æ ±æÀÌµµ ´Ù½Ã Ã¼Å© °¡´É
        System.Text.RegularExpressions.Regex regex =
            new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9°¡-ÆR]{2,16}$");

        IsValidNickName =  regex.IsMatch(text);

        if(IsValidNickName)
        {
            confirmUserNickNameButton.interactable = true;
        }
        else
        {
            confirmUserNickNameButton.interactable = false;
        }

    }

    private void SetNickName()
    {
        if (userNickNameInputfield.text.Length <= 0)
        {
            Debug.Log("No Input");
        }
        else
        {
            csSingleton.Instance.strPlayerNickName = userNickNameInputfield.text;

            csSaveLodeManager.Instance.SaveData();

            SetMainUI();
        }
         
    }
    private void SetMainUI()
    {
        userNickNameInputUI.SetActive(false);


        mainUI.SetActive(true);

        uiAnimator.SetBool("IsSettedNickName", true);

        csUIManager.Instance.SetIsInMainScreen(true);
    }
}

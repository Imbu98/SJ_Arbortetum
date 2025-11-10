using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PopupPanel : MonoBehaviour
{
    public static PopupPanel Instance { get { return _Instance; } }
    private static PopupPanel _Instance;

    [Header("Popup Parts")]
    [SerializeField] private PopupPart popupPart;

    [Header("TermsOfUse")]
    [SerializeField] private TextMeshProUGUI TermsOfUseText;
    [SerializeField] private GameObject TermsOfUsePopUp;


    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        
        //contentTypeDropdown.onValueChanged.AddListener(DD_SetContentType);
        //createGameButton.onClick.AddListener(Btn_CreateGame);
        //foreach(var buttons in SetLanguageButtons)
        //{
        //     �� ��ư�鿡 �̺�Ʈ �߰� ( �������� ���� ��� ���� )
        //    buttons.onClick.AddListener();
        //}
    }

    public void CloseAllParts()
    {
        popupPart.gameObject.SetActive(false);
    }

    //public void PopupQuitGame()
    //{
    //    popupPart.gameObject.SetActive(true);
    //    popupPart.InitText(null,null, "PopupPanel", "Popup_QuitTitle");
    //    popupPart.InitButtonA("PopupPanel", "Popup_Yes", GameManager.I.QuitGame);
    //    popupPart.InitButtonB("PopupPanel", "Popup_No", () => popupPart.gameObject.SetActive(false));
    //}
}

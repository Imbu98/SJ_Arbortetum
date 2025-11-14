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

public class csPopupPanel : MonoBehaviour
{
    public static csPopupPanel Instance { get { return _Instance; } }
    private static csPopupPanel _Instance;

    [Header("Popup Parts")]
    [SerializeField] private csPopupPart popupPart;

    [Header("TermsOfUse")]
    public GameObject termsOfUsePopup; // 약관 동의창
    [SerializeField] private TextMeshProUGUI TermsOfUseText;
    [SerializeField] private GameObject TermsOfUseDetailPopup;


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

    public void PopupSetScreenToCamera(UnityAction unityaction)
    {
        popupPart.gameObject.SetActive(true);
        popupPart.InitText("PopupPanel", "Popup_SetToCameraScreen");
        popupPart.InitButtonA("PopupPanel", "Popup_Yes", unityaction+CloseAllParts);
    }


    // 약관 동의 메뉴 팝업 
    public void PopupAgreeTermsOfUse(bool bShow)
    {
        termsOfUsePopup.SetActive(bShow);
    }

    public void OpenTermsOfUsePopUpButton(PolicyType policyType)
    {

        var localizedTermsOfUseString = new LocalizedString { TableReference = "MainPanel" };

        if (policyType == PolicyType.Service)
        {
            localizedTermsOfUseString.TableEntryReference = "Key_ServicePolicy";
        }
        else if (policyType == PolicyType.Privacy)
        {
            localizedTermsOfUseString.TableEntryReference = "Key_PrivacyPolicy";
        }
        else if (policyType == PolicyType.Marketing)
        {
            localizedTermsOfUseString.TableEntryReference = "Key_MarketingPolicy";

        }
        else
        {
            Debug.LogError($"Invalid popUpType: {policyType.ToString()}");
            return;
        }


        var modeHandle = localizedTermsOfUseString.GetLocalizedStringAsync();
        modeHandle.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                TermsOfUseText.text = handle.Result;
                TermsOfUseDetailPopup.SetActive(true);
            }
        };
    }
}

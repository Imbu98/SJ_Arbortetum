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
    public GameObject missionClearScreen; // 미션 성공 시 뜨는 창


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

        csUIManager.Instance.Remove(popupPart);
    }

    public void PopupSetScreenToCamera(UnityAction unityaction)
    {
        OpendAndBindingBackButton();
        popupPart.InitText("PopupPanel", "Popup_SetToCameraScreen");
        popupPart.InitButtonA("PopupPanel", "Popup_Yes", BindingActionAndClosePopup(unityaction));
    }

    public void PopupCancelMission(UnityAction unityaction)
    {
        OpendAndBindingBackButton();
        popupPart.InitText("PopupPanel", "Popup_ResetMission");
        popupPart.InitButtonA("PopupPanel", "Popup_No", CloseAllParts);
        popupPart.InitButtonB("PopupPanel", "Popup_Yes", BindingActionAndClosePopup(unityaction));
    }

    public void PopupResetMission(UnityAction unityaction)
    {
        OpendAndBindingBackButton();
        popupPart.InitText("PopupPanel", "Popup_ResetMission");
        popupPart.InitButtonA("PopupPanel", "Popup_No", CloseAllParts);
        popupPart.InitButtonB("PopupPanel", "Popup_Reset", BindingActionAndClosePopup(unityaction));
    }
    public void PopupForgiveCurrentMission(UnityAction unityaction)
    {
        OpendAndBindingBackButton();
        popupPart.InitText("PopupPanel", "Popup_ResetMission");
        popupPart.InitButtonA("PopupPanel", "Popup_No", CloseAllParts);
        popupPart.InitButtonB("PopupPanel", "Popup_Forgive", BindingActionAndClosePopup(unityaction));
    }

    public void PopupResetQuiz(UnityAction unityaction)
    {
        OpendAndBindingBackButton();
        popupPart.InitText("PopupPanel", "Popup_ResetQuiz");
        popupPart.InitButtonA("PopupPanel", "Popup_No", CloseAllParts);
        popupPart.InitButtonB("PopupPanel", "Popup_Reset", BindingActionAndClosePopup(unityaction));
    }

    public void PopupQuitApplication(UnityAction unityaction)
    {
        OpendAndBindingBackButton();
        popupPart.InitText("PopupPanel", "Popup_QuitApp");
        popupPart.InitButtonA("PopupPanel", "Popup_No", CloseAllParts);
        popupPart.InitButtonB("PopupPanel", "Popup_Quit", BindingActionAndClosePopup(unityaction));
    }

    // 팝업이 호출 될 때, 팝업을 나타내고 뒤로가기에 팝업끄는 액션 바인딩
    private void OpendAndBindingBackButton()
    {
        popupPart.gameObject.SetActive(true);
        csUIManager.Instance.Push(popupPart, CloseAllParts);
    }

    private UnityAction BindingActionAndClosePopup(UnityAction unityaction)
    {
        return () =>
        {
            unityaction?.Invoke();
            CloseAllParts();
        };
    }


    // 약관 동의 메뉴 팝업 
    public void PopupAgreeTermsOfUse(bool bShow)
    {

        termsOfUsePopup.SetActive(bShow);
        
    }

    public void OpenTermsOfUseDetailPopUpButton(PolicyType policyType)
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
                csUIManager.Instance.Push(TermsOfUseDetailPopup, CloseTermsOfUseDetail);
            }
        };
    }

    public void CloseTermsOfUseDetail()
    {
        TermsOfUseDetailPopup.SetActive(false);

        csUIManager.Instance.Remove(TermsOfUseDetailPopup);
    }
}

using Data;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class csSettingScreen : MonoBehaviour
{
    [Header("UserInfo")]
    [SerializeField] private TextMeshProUGUI UID_TMP;
    [SerializeField] private TextMeshProUGUI userNickName_TMP;
    [SerializeField] private Button nickNameChangeButton;
    [SerializeField] private GameObject nickNameChangePopup;
    [SerializeField] private TMP_InputField nickNameChangeInputField;
    [SerializeField] private Button confirmNickNameChangeButton;
    [SerializeField] private Button cancelNickNameChangeButton;

    [Header("Sounds")]
    [SerializeField] private Button bgmMuteToggleButton;
    [SerializeField] private Image bgmMuteIcon;
    [SerializeField] private Button soundEffectMuteToggleButton;
    [SerializeField] private Image soundEffectMuteIcon;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider soundEffectSlider;
    [SerializeField] private TextMeshProUGUI bgmAmount_TMP;
    [SerializeField] private TextMeshProUGUI soundEffectAmount_TMP;

    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    [Header("RecommendQuiz&Mission")]
    [SerializeField] private List<Button> recommendTimeButtonList;
    [SerializeField] private Color SelectedColor;
    [SerializeField] private Color unSelectedColor;

    [Header("TermsOfUse")]
    [SerializeField] private Button serviceTermsUseOfButton;
    [SerializeField] private Button privacyTermsUseOfButton;
    [SerializeField] private Button marketingTermsUseOfButton;

    [Header("BottomButtons")]
    [SerializeField] private Button quitAppButton;
    [SerializeField] private Button signOutButton;

    [SerializeField] private Button closeSettingScreenButton;

    private void OnEnable()
    {
        nickNameChangeButton.onClick.AddListener(OnClickNicknameChangeButton);
        nickNameChangeInputField.onValueChanged.AddListener(InputUserNickName);
        confirmNickNameChangeButton.onClick.AddListener(SetNickName);
        cancelNickNameChangeButton.onClick.AddListener(CloseNickNameChangePopup);


        bgmMuteToggleButton.onClick.AddListener(ToggleBgmMute);
        bgmSlider.onValueChanged.AddListener(BgmSliderValueChanged);

        soundEffectMuteToggleButton.onClick.AddListener(ToogleSoundEffectMute);
        soundEffectSlider.onValueChanged.AddListener(SoundEffectSliderValueChanged);

        for(int i =0;i<recommendTimeButtonList.Count;i++)
        {
            int capturedindex = i; // 클로저 방지
            recommendTimeButtonList[i].onClick.AddListener(()=>SetRecommendTime(capturedindex));
        }

        serviceTermsUseOfButton.onClick.AddListener(() => csPopupPanel.Instance.OpenTermsOfUseDetailPopUpButton(PolicyType.Service));
        privacyTermsUseOfButton.onClick.AddListener(() => csPopupPanel.Instance.OpenTermsOfUseDetailPopUpButton(PolicyType.Privacy));
        marketingTermsUseOfButton.onClick.AddListener(()=> csPopupPanel.Instance.OpenTermsOfUseDetailPopUpButton(PolicyType.Marketing));


        signOutButton.onClick.AddListener(OnClickSignOutButton);
        quitAppButton.onClick.AddListener(OnClickQuitApplicationButton);

        closeSettingScreenButton.onClick.AddListener(OnClickCloseSettingScreenButton);

        InitUI();
    }

    private void OnDisable()
    {
        nickNameChangeButton.onClick.RemoveAllListeners();
        nickNameChangeInputField.onValueChanged.RemoveAllListeners();
        confirmNickNameChangeButton.onClick.RemoveAllListeners();
        cancelNickNameChangeButton.onClick.RemoveAllListeners();

        bgmMuteToggleButton.onClick.RemoveAllListeners();
        bgmSlider.onValueChanged.RemoveAllListeners();

        soundEffectMuteToggleButton.onClick.RemoveAllListeners();
        soundEffectSlider.onValueChanged.RemoveAllListeners();

        for (int i = 0; i < recommendTimeButtonList.Count; i++)
        {
            recommendTimeButtonList[i].onClick.RemoveAllListeners();
        }

        serviceTermsUseOfButton.onClick.RemoveAllListeners();
        privacyTermsUseOfButton.onClick.RemoveAllListeners();
        marketingTermsUseOfButton.onClick.RemoveAllListeners();


        signOutButton.onClick.RemoveAllListeners();
        quitAppButton.onClick.RemoveAllListeners();

        closeSettingScreenButton.onClick.RemoveAllListeners();
    }

    private void InitUI()
    {
        userNickName_TMP.text = csSingleton.Instance.strPlayerNickName;
        UID_TMP.text = csSingleton.Instance.UID;

        bgmMuteIcon.sprite = csSingleton.Instance.bBgmMute ? soundOffSprite : soundOnSprite;
        soundEffectMuteIcon.sprite = csSingleton.Instance.bSoundEffectMute ? soundOffSprite : soundOnSprite;

        bgmSlider.value = csSingleton.Instance.fBgm;
        bgmAmount_TMP.text = (csSingleton.Instance.fBgm * 100f).ToString("F0");

        soundEffectSlider.value = csSingleton.Instance.fSoundEffect;
        soundEffectAmount_TMP.text = (csSingleton.Instance.fSoundEffect * 100f).ToString("F0");

        int timerIndex=-1;

        switch(csSingleton.Instance.fRecommendTimer)
        {
            case 300f:
                {
                    timerIndex = 0; break;
                }
            case 600f:
                {
                    timerIndex = 1; break;
                }
            case 1800f:
                {
                    timerIndex = 2; break;
                }
            case 0f:
                {
                    timerIndex = 3; break;
                }
        }

        for(int i = 0;i<recommendTimeButtonList.Count;i++)
        {
            if(i==timerIndex)
            {
                recommendTimeButtonList[i].GetComponent<Image>().color = SelectedColor;
            }
            else
            {
                recommendTimeButtonList[i].GetComponent<Image>().color = unSelectedColor;
            }
        }
    }

    private void ToggleBgmMute()
    {
        csSingleton.Instance.bBgmMute = !csSingleton.Instance.bBgmMute;
        bgmMuteIcon.sprite = csSingleton.Instance.bBgmMute? soundOffSprite: soundOnSprite;

        csSaveLodeManager.Instance.SaveSet();
    }

    private void ToogleSoundEffectMute()
    {
        csSingleton.Instance.bSoundEffectMute = !csSingleton.Instance.bSoundEffectMute;
        soundEffectMuteIcon.sprite = csSingleton.Instance.bSoundEffectMute ? soundOffSprite : soundOnSprite;

        csSaveLodeManager.Instance.SaveSet();
    }

    private void BgmSliderValueChanged(float value)
    {
        csSingleton.Instance.fBgm = value;
        bgmAmount_TMP.text = (value * 100f).ToString("F0"); // 소수값이니 100을 곱해서 정수값으로 표시

    }

    private void SoundEffectSliderValueChanged(float value)
    {
        csSingleton.Instance.fSoundEffect = value;
        soundEffectAmount_TMP.text = (value * 100f).ToString("F0"); // 소수값이니 100을 곱해서 정수값으로 표시
    }

    private void SetRecommendTime(int index)
    {
        // 버튼 UI 설정
        for(int i=0;i<recommendTimeButtonList.Count;i++)
        {
            if(i==index)
            {
                recommendTimeButtonList[i].GetComponent<Image>().color = SelectedColor;
            }
            else
            {
                recommendTimeButtonList[i].GetComponent<Image>().color = unSelectedColor;
            }
        }

        switch(index)
        {
            case 0:
                {
                    csSingleton.Instance.fRecommendTimer = 300f;
                    break;
                }
            case 1:
                {
                    csSingleton.Instance.fRecommendTimer = 600f;
                    break;
                }

            case 2:
                {
                    csSingleton.Instance.fRecommendTimer = 1800f;
                    break;
                }
            case 3:
                {
                    csSingleton.Instance.fRecommendTimer = 0f;
                    break;
                }
        }
        csSaveLodeManager.Instance.SaveSet();
    }

    private void OnClickSignOutButton()
    {
        this.gameObject.SetActive(false);

        csLoginManager.Instance.SignOut();
    }

    private void OnClickQuitApplicationButton()
    {
        csPopupPanel.Instance.PopupQuitApplication(csUIManager.Instance.QuitApplication);
    }

    private void OnClickCloseSettingScreenButton()
    {
        csSaveLodeManager.Instance.SaveSet();
        csUIManager.Instance.PopupSettingScreen(false);
    }

    private void OnClickNicknameChangeButton()
    {
        nickNameChangePopup.SetActive(true);
    }

    private void InputUserNickName(string text)
    {
        bool IsValidNickName = false;

        // 1) 길이 제한 (2~16자)
        if (text.Length >= 2 || text.Length <= 16)
        {
            IsValidNickName = true;
        }

        // 2) 특수문자 포함 여부 검사
        //    허용 문자: 한글, 영어, 숫자
        //    금지 문자: 특수문자 전체
        //    ^ → 문자열 시작, $ → 문자열 끝
        //    [] 안에서 허용 문자 정의
        //    {2,16} → 길이도 다시 체크 가능
        System.Text.RegularExpressions.Regex regex =
            new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9가-힣]{2,16}$");

        IsValidNickName = regex.IsMatch(text);

        if (IsValidNickName)
        {
            confirmNickNameChangeButton.interactable = true;
        }
        else
        {
            confirmNickNameChangeButton.interactable = false;
        }

    }

    private void SetNickName()
    {
        if (nickNameChangeInputField.text.Length <= 0)
        {
            Debug.Log("No Input");
        }
        else
        {
            csSingleton.Instance.strPlayerNickName = nickNameChangeInputField.text;

            userNickName_TMP.text = nickNameChangeInputField.text;

            csSaveLodeManager.Instance.SaveData();

            CloseNickNameChangePopup();
        }
    }

    public void CloseNickNameChangePopup()
    {
        nickNameChangePopup.SetActive(false);

        nickNameChangeInputField.text = string.Empty;

        csUIManager.Instance.ResetAIChatText();
    }
}

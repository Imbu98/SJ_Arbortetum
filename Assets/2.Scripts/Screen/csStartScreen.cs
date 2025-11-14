using UnityEngine;
using UnityEngine.UI;
using Data;

public class csStartScreen : MonoBehaviour
{
    

    // 구글, 애플 로그인 버튼
    [Header("Login Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button appleLoginButton;

    [Header("Agree to Terms&Conditions")]

    [SerializeField] private Button agreeAllButton;
    [SerializeField] private Button agreeServicePolicyButton;
    [SerializeField] private Button agreePrivacyPolicyButton;
    [SerializeField] private Button agreeMarketingPolicyButton;

    [SerializeField] private Image agreeAll_IconImage;
    [SerializeField] private Image agreeServicePolicyIconImage;
    [SerializeField] private Image agreePrivacyPolicyIconImage;
    [SerializeField] private Image agreeMarketingPolicyIconImage;

    [SerializeField] private Button showServicePolicyButton;
    [SerializeField] private Button showPrivacyPolicyButton;
    [SerializeField] private Button showMarketingPolicyButton;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button closeTermsOfUseButton;

    [SerializeField] private Color agreeColor;
    [SerializeField] private Color disagreeColor;


    private bool isAgreeServicePolicy = false;
    private bool isAgreePrivacyPolicy = false;
    private bool isAgreeMarketingPolicy = false;
    private bool isAgreeAll = false;


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

    private void Start()
    {


        csUI_Manager.Instance.ChangeScreen(csUI_Manager.Instance.startScreen);
    }
    void OnEnable()
    {
        startButton?.onClick.AddListener(OnStartButtonClicked);
        googleLoginButton?.onClick.AddListener(csLoginManager.Instance.GoogleLogin);
        appleLoginButton?.onClick.AddListener(csLoginManager.Instance.AppleLogin);
        closeTermsOfUseButton.onClick.AddListener(CloseTermsOfUse);

        agreeAllButton.onClick.AddListener(AgreeAll);
        agreeServicePolicyButton.onClick.AddListener(ToggleServicePolicy);
        agreePrivacyPolicyButton.onClick.AddListener(TogglePrivacyPolicy);
        agreeMarketingPolicyButton.onClick.AddListener(ToggleMarketingPolicy);

        showServicePolicyButton.onClick.AddListener(() => csPopupPanel.Instance.OpenTermsOfUsePopUpButton(PolicyType.Service));
        showPrivacyPolicyButton.onClick.AddListener(() => csPopupPanel.Instance.OpenTermsOfUsePopUpButton(PolicyType.Privacy));
        showMarketingPolicyButton.onClick.AddListener(() => csPopupPanel.Instance.OpenTermsOfUsePopUpButton(PolicyType.Marketing));

        if (csSingleton.Instance.bAutoLogin)
        {
            // 구글로그인
            if(csSingleton.Instance.nSavedLoginType==1)
            {
                csLoginManager.Instance.GoogleLogin();
            }
            // 애플로그인
            else if(csSingleton.Instance.nSavedLoginType == 2)
            {
                csLoginManager.Instance.AppleLogin();
            }
        }
        else
        {
            SetLoginButton();
        }
            
    }

    // Update is called once per frame
    void OnDisable()
    {
        googleLoginButton?.onClick.RemoveAllListeners();
        appleLoginButton?.onClick.RemoveAllListeners();
        closeTermsOfUseButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();

        agreeAllButton.onClick.RemoveAllListeners();
        agreeServicePolicyButton.onClick.RemoveAllListeners();
        agreePrivacyPolicyButton.onClick.RemoveAllListeners();
        agreeMarketingPolicyButton.onClick.RemoveAllListeners();

        showServicePolicyButton.onClick.RemoveAllListeners();
        showPrivacyPolicyButton.onClick.RemoveAllListeners();
        showMarketingPolicyButton.onClick.RemoveAllListeners();

    }

    private void SetLoginButton()
    {
#if UNITY_ANDROID
        googleLoginButton.gameObject.SetActive(true);
        appleLoginButton.gameObject.SetActive(false);
        confirmButton.onClick.AddListener(() => csLoginManager.Instance.GoogleLoginSuccess());
#elif UNITY_IOS
        googleLoginButton.gameObject.SetActive(false);
        appleLoginButton.gameObject.SetActive(true);
        confirmButton.onClick.AddListener(() => csLoginManager.Instance.AppleLoginSuccess());
#endif
    }


    void OnStartButtonClicked()
    {
        csUI_Manager.Instance.ChangeScreen(csUI_Manager.Instance.mainScreen);
    }


    private void ToggleServicePolicy()
    {
        isAgreeServicePolicy = !isAgreeServicePolicy;

        agreeServicePolicyIconImage.color =isAgreeServicePolicy ? agreeColor : disagreeColor;

        CheckAgreeAll();
    }
    private void TogglePrivacyPolicy()
    {
        isAgreePrivacyPolicy = !isAgreePrivacyPolicy;

        agreePrivacyPolicyIconImage.color = isAgreePrivacyPolicy ? agreeColor : disagreeColor;

        CheckAgreeAll();

    }
    private void ToggleMarketingPolicy()
    {
        isAgreeMarketingPolicy = !isAgreeMarketingPolicy;

        agreeMarketingPolicyIconImage.color= isAgreeMarketingPolicy ? agreeColor : disagreeColor;

        CheckAgreeAll();
    }

    private void AgreeAll()
    {
        if(isAgreeAll==false)
        {
            isAgreeServicePolicy = true;
            agreeServicePolicyIconImage.color = agreeColor;

            isAgreePrivacyPolicy = true;
            agreePrivacyPolicyIconImage.color = agreeColor;

            isAgreeMarketingPolicy = true;
            agreeMarketingPolicyIconImage.color = agreeColor;

            isAgreeAll = true;
            agreeAll_IconImage.color = agreeColor;

            
        }
        else if(isAgreeAll==true)
        {
            isAgreeServicePolicy = false;
            agreeServicePolicyIconImage.color = disagreeColor;

            isAgreePrivacyPolicy = false;
            agreePrivacyPolicyIconImage.color = disagreeColor;

            isAgreeMarketingPolicy = false;
            agreeMarketingPolicyIconImage.color = disagreeColor;

            isAgreeAll = false;
            agreeAll_IconImage.color = disagreeColor;

            
        }

        CheckAgreeAll();
    }

    private void CheckAgreeAll()
    {
        if(isAgreeServicePolicy&&isAgreePrivacyPolicy&&isAgreeMarketingPolicy)
        {
            agreeAll_IconImage.color = agreeColor;
            isAgreeAll = true;

            confirmButton.interactable = true;
        }
        else
        {
            agreeAll_IconImage.color = disagreeColor;
            isAgreeAll = false;

            confirmButton.interactable = false;
        }
    }

    // 약관 동의창 닫기
    private void CloseTermsOfUse()
    {
        isAgreeServicePolicy = false;
        agreeServicePolicyIconImage.color = disagreeColor;

        isAgreePrivacyPolicy = false;
        agreePrivacyPolicyIconImage.color = disagreeColor;

        isAgreeMarketingPolicy = false;
        agreeMarketingPolicyIconImage.color = disagreeColor;

        isAgreeAll = false;

        confirmButton.interactable = false;

        csPopupPanel.Instance.PopupAgreeTermsOfUse(false);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class csStartScreen : MonoBehaviour
{
    [SerializeField] private Button startButton;

    // 구글, 애플 로그인 버튼
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button appleLoginButton;

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

    }

    private void SetLoginButton()
    {
#if UNITY_ANDROID
        googleLoginButton.gameObject.SetActive(true);
        appleLoginButton.gameObject.SetActive(false);
#elif UNITY_IOS
        googleLoginButton.gameObject.SetActive(false);
        appleLoginButton.gameObject.SetActive(true);
#endif
    }
   
   
    void OnStartButtonClicked()
    {
        csUI_Manager.Instance.ChangeScreen(csUI_Manager.Instance.mainScreen);
    }
}

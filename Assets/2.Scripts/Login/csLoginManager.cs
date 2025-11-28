using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class csLoginManager : MonoBehaviour
{
    public static csLoginManager Instance { get { return _Instance; } }
    private static csLoginManager _Instance;

    private IAppleAuthManager appleAuthManager;

    private void Start()
    {
        //애플로그인 초기화
#if UNITY_IOS
InitializeAppleAuth();
#endif
    }
private void Update()
{
#if UNITY_IOS
        if (this.appleAuthManager != null)
        {
            this.appleAuthManager.Update();
        }
#endif
}

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

#if UNITY_ANDROID
    // GPGS 로그인 
    public void GoogleLogin()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
    }
    /// <summary>
    /// GPGS 로그인
    /// </summary>
    /// <param name="status"></param>
    void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            if(csSingleton.Instance.bTermsofUse)
            {
                GoogleLoginSuccess();
            }
            else
            {
                csPopupPanel.Instance.PopupAgreeTermsOfUse(true);
            }
        }
        else
        {
            Debug.Log("GOOGLE LOGIN FAILED");
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
           
        }
    }

    public void GoogleLoginSuccess()
    {
        Debug.Log("GOOGLE LOGIN SUCCESS");
        string name = PlayGamesPlatform.Instance.GetUserDisplayName();
        string id = PlayGamesPlatform.Instance.GetUserId();

        csSingleton.Instance.bAutoLogin = true;
        csSingleton.Instance.UID = id;
        csSingleton.Instance.nSavedLoginType = 1;
        csSingleton.Instance.bTermsofUse = true;

        csSaveLodeManager.Instance.SaveSet();


        csUIManager.Instance.ChangeScreen(csUIManager.Instance.mainScreen.gameObject);
        csPopupPanel.Instance.PopupAgreeTermsOfUse(false);
    }
   
#endif


    /// <summary>
    /// 애플 로그인 초기화
    /// </summary>
    void InitializeAppleAuth()
    {
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            var deserializer = new PayloadDeserializer();
            appleAuthManager = new AppleAuthManager(deserializer);

            appleAuthManager?.Update();

            Debug.LogError("애플 로그인 초기화 완료");
        }
        else
        {
            Debug.LogError("Apple Auth not supported on this platform.");
        }
    }

    public bool IsAppleAuthManagerSetted()
    {
        if(appleAuthManager!=null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AppleLogin()
    {
        Debug.Log("애플 로그인 실행");

#if UNITY_IOS
    StartCoroutine(TryAppleLoginRoutine());
#endif
    }

    private IEnumerator TryAppleLoginRoutine()
    {
        // appleAuthManager가 생길 때까지 대기
        while (appleAuthManager == null)
        {
            yield return null;
        }

        // 초기화 완료—이제 로그인 실행
        ExecAppleLogin();
    }

    private void ExecAppleLogin()
    {
       
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    var appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        // Apple Identity Token 얻기 (JWT)
                        string idToken = Encoding.UTF8.GetString(
                            appleIdCredential.IdentityToken,
                            0,
                            appleIdCredential.IdentityToken.Length);

                        csSingleton.Instance.UID = appleIdCredential.User;

                        if (csSingleton.Instance.bTermsofUse)
                        {
                            AppleLoginSuccess();

                        }
                        else
                        {
                            csPopupPanel.Instance.PopupAgreeTermsOfUse(true);
                        }


                    }
                    else
                    {
                        Debug.LogError("appleIdCredential is null");
                    }
                },
                error =>
                {
                    Debug.LogError("Apple login failed: " + error.LocalizedDescription);
                }
            );
    }


    public void AppleLoginSuccess()
    {

        Debug.Log("Apple login Success");

        csSingleton.Instance.bAutoLogin = true;
        csSingleton.Instance.nSavedLoginType = 2;
        csSingleton.Instance.bTermsofUse = true;

        csSaveLodeManager.Instance.SaveSet();
        csUIManager.Instance.ChangeScreen(csUIManager.Instance.mainScreen.gameObject);
        csPopupPanel.Instance.PopupAgreeTermsOfUse(false);
    }
    public void OnClickedSignoutButton(UnityAction closeSettingScreen)
    {
        csPopupPanel.Instance.PopupQuitSignOut(SignOut+closeSettingScreen);
    }

    // 로그아웃 시 어플에 로컬 정보들만 지우기
    private void SignOut()
    {
        csSingleton.Instance.UID = "";
        csSingleton.Instance.bAutoLogin = false;
        csSingleton.Instance.nSavedLoginType = 0;

        csSaveLodeManager.Instance.SaveSet();

        csUIManager.Instance.ChangeScreen(csUIManager.Instance.startScreen);
    }

    //    PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (bool success) =>
    //    {
    //        if (success)
    //        {
    //            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
    //            string id = PlayGamesPlatform.Instance.GetUserId();

    //            csSingleton.Instance.bAutoLogin = true;
    //            csSingleton.Instance.UID = id;

    //            csUIManager.Instance.ChangeScreen(csUIManager.Instance.mainScreen);

    //        }
    //        else
    //        {
    //            Debug.Log("GPGS Login Failed");
    //        }

    //    });
    //}
}





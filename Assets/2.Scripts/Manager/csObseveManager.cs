using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;


public class csObserveManager : MonoBehaviour
{
    public static csObserveManager Instance { get { return _Instance; } }
    private static csObserveManager _Instance;

    // 관찰하기 화면
    [SerializeField] public GameObject observeScreenObject;
    // 분석 로딩 화면
    [SerializeField] public GameObject observeLoadingObject;
    // 분석 결과 화면
    [SerializeField] public GameObject observeResultObject;

    // 현재 띄워진 관찰하기 화면 저장
    private GameObject currentObserve;

    [HideInInspector] public Texture2D capturedTexture;

    [SerializeField] private csObserveResult _observeResult;
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

    public void SetObserveScreen(GameObject currentObserveScreen)
    {
        if(currentObserve!=null)
        {
            currentObserve.SetActive(false);
        }

        currentObserve = currentObserveScreen;
        currentObserveScreen.SetActive(true);
    }

    public void CloseObserveScreen()
    {
        if (currentObserve)
        {
            currentObserve.SetActive(false);
            currentObserve = null;
        }
    }

    // 카메라 촬영 화면으로
    public void SetCameraScreen()
    {
        if(currentObserve)
        {
            currentObserve.SetActive(false);
        }
        observeScreenObject.SetActive(true);
        currentObserve = observeScreenObject;
    }

    async public void AnalyzeTexture()
    {
        SetObserveScreen(observeLoadingObject);


        // AI 분석 완료 후 데이터를 받아서 observeResult Init 해주기
        //await csNetworkManager.Instance.

        SetObserveScreen(observeResultObject);
        //_observeResult.Init(plantData)
    }
}

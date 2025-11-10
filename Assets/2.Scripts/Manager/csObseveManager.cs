using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading.Tasks;
using TMPro;


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

    public RawImage targetDisplay;

    [SerializeField] private TextMeshProUGUI loadingPercentage;
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


        loadingPercentage.text = "0%";
        // 초기화

        // ✅ 진행률 콜백에서 TMP 텍스트 업데이트
        var analyzedPlantData = await csNetworkManager.Instance.AsyncGetPlantImageAsync(
            capturedTexture,
            (pct) =>
            {
                loadingPercentage.text = $"{pct * 100f:F0}%";
            });

        if (analyzedPlantData != null)
        {
            // 정보가 있으면 해당정보로 서버에서 식물에 대한 정보 가져오기
            //await csNetworkManager.Instance.AsyncGetPlantInfo()

            SetObserveScreen(observeResultObject);
            _observeResult.Init(analyzedPlantData);
        }
        else if(analyzedPlantData==null/* || analyzedPlantData.score<21414*/)
        {
            // 다시 찍기
        }
    }
}

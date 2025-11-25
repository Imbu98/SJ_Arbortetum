using Data;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.AddressableAssets.GUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class csObserveResult : MonoBehaviour
{
    [Header("ScrollView")]
    [SerializeField] private csCustomScrollRect scrollView;

    [Header("ActiveImageIcon")]
    [SerializeField] private Image showActiveImagePrefab; // 몇번째 이미지가 활성화 되었는지 알려주는 이미지 프리펩
    [SerializeField] private RectTransform showActiveImageHolder;
    private List<Image> showActiveImageIconList=new List<Image>(); // 생성된 이미지 아이콘 저장
    [SerializeField] private Sprite currentPageSprite;
    [SerializeField] private Sprite defaultPageSprite;

    [Header("Image")]
    [SerializeField] private RawImage imagePrefab; // 이미지 생성 프리팹
    private List<Texture2D> ImageList = new List<Texture2D>();
    [SerializeField] private Sprite defaultSprite; // 이미지 불러오기에 실패했을 때 쓸 기본 이미지


    [Header("Button")]
    [SerializeField] private Button resultButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI plantNameTMP;
    [SerializeField] private TextMeshProUGUI plantScientificName_TMP;
    [SerializeField] private TextMeshProUGUI plantDescriptionTMP;

    private int currentPageCount=0; // 몇번째 페이지의 이미지인지
    
    private int imageCount=0; // 이미지 개수 
    
    private bool isSnapping = false; // 스냅중인지

    private GetPlantResponse currentPlantData;

    private void OnEnable()
    {
        //Init();

        scrollView.onEndDragEvent.AddListener(OnScrollEndDrag);
    }

    public void OnDisable()
    {
        scrollView.onEndDragEvent.RemoveAllListeners();
    }

    public async Task Init(GetPlantResponse plantData)
    {
        currentPlantData = plantData; 

        Clear();

        await SetUI();

        SetContentImage();
    }

    // UI초기화
    public void Clear()
    {
        scrollView.horizontalNormalizedPosition = 0f;

        currentPageCount = 0;

        ImageList.Clear();
        
        if(showActiveImageIconList.Count > 0 )
        {
            showActiveImageIconList.Clear();
        }
        
    }

    // 이미지 슬라이더를 위한 크기 설정
    private void SetContentImage()
    {
        if (scrollView == null) return;

        RectTransform viewport = scrollView.viewport;
        RectTransform content = scrollView.content;

        if (viewport == null || content == null) return;

       // 이미지 리스트 삭제
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 활성 이미지 알려주는 이미지 리스트 삭제
        foreach (Transform child in showActiveImageHolder)
        {
            Destroy(child.gameObject);
        }

        // 이미지 리스트를 content에 자식으로 생성
        for (int i = 0; i < ImageList.Count; i++)
        {
            if (imagePrefab)
            {
                RawImage newImage = Instantiate(imagePrefab, content);
                imagePrefab.GetComponent<csObservePlantImagePrefab>()?.Init(
                    currentPlantData.plantimages[i].license,
                    currentPlantData.plantimages[i].reference
                );
                if(ImageList[i] != null)
                {
                    newImage.texture = ImageList[i];
                }
                else
                {
                    newImage.texture = defaultSprite.texture;
                }
                
            }
            if (showActiveImagePrefab && showActiveImageHolder)
            {
                Image showActiveImage = Instantiate(showActiveImagePrefab, showActiveImageHolder);
                showActiveImageIconList.Add(showActiveImage);
            }
        }

        imageCount = ImageList.Count;

        if (imageCount == 0) return;

        Vector2 viewportSize = viewport.rect.size;

        foreach (RectTransform child in scrollView.content)
        {
            // 뷰포트 크기에 맞게 설정
            child.sizeDelta = new Vector2(viewportSize.x, viewportSize.y);
        }

        SetActiveImageUI();
    }

    private void OnScrollEndDrag(PointerEventData eventData)
    {
        if (isSnapping || imageCount <= 1) return;

        float pos = scrollView.horizontalNormalizedPosition;
        float pageInterval = 1f / (imageCount - 1);
        int nearestPage = Mathf.RoundToInt(pos / pageInterval);
        nearestPage = Mathf.Clamp(nearestPage, 0, imageCount - 1);

        StopAllCoroutines();
        StartCoroutine(SmoothSnapTo(nearestPage));
    }

    private IEnumerator SmoothSnapTo(int targetPage)
    {
        isSnapping = true;

        float targetPos = (float)targetPage / (imageCount - 1);
        float duration = 0.25f;
        float time = 0f;
        float startPos = scrollView.horizontalNormalizedPosition;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / duration);
            scrollView.horizontalNormalizedPosition = Mathf.Lerp(startPos, targetPos, t);
            yield return null;
        }

        scrollView.horizontalNormalizedPosition = targetPos;
        currentPageCount = targetPage;
        isSnapping = false;

        SetActiveImageUI();
    }
    // 활성 이미지 UI 나타내기
    private void SetActiveImageUI()
    {
        if(showActiveImageIconList.Count > 0)
        {
            for(int i = 0;i<showActiveImageIconList.Count ;++i)
            {
                if(i == currentPageCount)
                {
                    showActiveImageIconList[i].sprite = currentPageSprite;
                }
                else
                {
                    showActiveImageIconList[i].sprite = defaultPageSprite;
                }
            }
            
        }
    }

    // 식물 정보 UI 세팅
    async private Task SetUI()
    {
        if (currentPlantData.commonNames.Count > 0)
        {
            plantNameTMP.text = currentPlantData.commonNames[0];
        }

        plantScientificName_TMP.text = currentPlantData.plantScientificName;

        string descriptionText = FormatPlantDescription("느티나무 Zelkova serrata (Thunb.) Makino ● 세계 분포 : 대만, 러시아, 일본, 중국, 한국 ● 국내 분포 : 전국의 산지 ● 형태 ○ 수형 : 낙엽성 교목이며 높이 30m까지 자란다. ○ 잎 : 잎은 어긋나며 길이 3-12cm의 장타원형-타원형 또는 장타원상 난형이다. 잎끝은 길게 뾰족하고 밑부분은 쐐기형이며 가장자리에는 뾰족한 톱니가 있다. 양면(특히 맥 위)에 뻣뻣한 털이 있다. 잎자루는 길이 2-10mm이며 털이 있다. ○ 꽃 : 꽃은 4-5월에 잎과 동시에 핀다. 수꽃은 새가지의 아랫부분에 모여 달리며 지름 3mm정도이고 수술은 4-6개이다. 암꽃은 새가지의 윗부분에 모여 달리며 지름 1.5mm정도 이고 암술대는 2개로 깊게 갈라진다. ○ 열매 : 열매는 견과이고 지름 3-4mm의 일그러진 편구형이며 9-10월에 익는다. ● 참고 예로부터 느티나무를 괴목(槐木)으로 불렀으며 마을 정자목으로 가장 많이 이용한 나무이다. 국명 은 누튀나무에서 변한 것으로서, ‘누틔’는 가을철 누른색으로 단풍 드는 특성에서 유래된 것으로 추정한다."); // FormatPlantDescription(currentPlantData.description);

        plantDescriptionTMP.text = descriptionText;

        bool isMissionInProgress = csMissionManager.Instance.IsMissonOnProgress;
        // 미션중일 때 미션의 요구식물과 같은지 확인
        bool isSamePlant = isMissionInProgress &&
                           csMissionManager.Instance.GetCurrentMissionStep().plantName == currentPlantData.plantScientificName;

        resultButton.onClick.RemoveAllListeners();

        resultButton.onClick.AddListener(() =>
        {
            SaveCurrentPlantName();
        });
        // 버튼 텍스트변경과 함수 바인딩
        if (isSamePlant)
        {
            // 현재 미션과 같은 식물일 때 → 미션 클리어
            resultButton.onClick.AddListener(csMissionManager.Instance.ClearCurrentMissionStep);
            resultButton.GetComponentInChildren<TextMeshProUGUI>().text = "단서획득"; // 추후 로컬라제이션
        }
        else
        {
            // 미션이 없거나, 다른 식물일 때 → 사진 촬영 화면으로
            resultButton.onClick.AddListener(csObserveManager.Instance.SetCameraScreen);
            resultButton.GetComponentInChildren<TextMeshProUGUI>().text = "확인"; // 추후 로컬라제이션
        }



        for (int i = 0; i < currentPlantData.plantimages.Count;++i)
        {
            string url = currentPlantData.plantimages[i].url;

            Texture2D plantTexture = await csNetworkManager.Instance.DownloadImage(url);
            if (plantTexture != null)
            {
                ImageList.Add(plantTexture);
            }

        }
    }
     
    private void SaveCurrentPlantName()
    {
        string plantName = currentPlantData.plantScientificName;

        if (!csSingleton.Instance.savedPlant.Contains(plantName))
        {
            csSingleton.Instance.savedPlant.Add(plantName);
            csSaveLodeManager.Instance.SaveSavedPlant();
            Debug.Log("식물 저장됨: " + plantName);
        }
        else
        {
            Debug.Log("이미 저장된 식물: " + plantName);
        }
    }

        public string FormatPlantDescription(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string output = input;

            // ●, ○ 앞에 줄바꿈 적용
            output = output.Replace("●", "\n\n●");
            output = output.Replace("○", "\n○");

            // 중복 줄바꿈 정리 (2번 이상 → 2번으로 통일)
            while (output.Contains("\n\n\n"))
            {
                output = output.Replace("\n\n\n", "\n\n");
            }

            // 양쪽 공백 제거
            return output.Trim();
        }

}

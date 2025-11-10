using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class csObserveResult : MonoBehaviour
{
    [Header("ScrollView")]
    [SerializeField] private csCustomScrollRect scrollView;

    [Header("ActiveImageIcon")]
    [SerializeField] private Image showActiveImagePrefab; // 몇번째 이미지가 활성화 되었는지 알려주는 이미지 프리펩
    [SerializeField] private RectTransform showActiveImageHolder;
    private List<Image> showActiveImageIconList=new List<Image>(); // 생성된 이미지 아이콘 저장

    [Header("Image")]
    [SerializeField] private Image imagePrefab; // 이미지 생성 프리팹
    [SerializeField] List<Sprite> testImageList; // 에디터용 테스트 이미지 리스트( 나중에 서버에서 가져온 텍스쳐로 변경 예정)

    [Header("Button")]
    [SerializeField] private Button resultButton;

    private int currentPageCount=0; // 몇번째 페이지의 이미지인지
    
    private int imageCount=0; // 이미지 개수 
    
    private bool isSnapping = false; // 스냅중인지

    private PlantData currentPlantData;

    private void OnEnable()
    {
        //Init();

        scrollView.onEndDragEvent.AddListener(OnScrollEndDrag);
    }

    public void OnDisable()
    {
        scrollView.onEndDragEvent.RemoveAllListeners();
    }

    public void Init(PlantData plantData)
    {
        currentPlantData = plantData; 

        Clear();

        SetContentImageSize();

        SetUI();
    }

    // UI초기화
    public void Clear()
    {
        scrollView.horizontalNormalizedPosition = 0f;

        currentPageCount = 0;
        //testImageList.Clear();
        if(showActiveImageIconList.Count > 0 )
        {
            showActiveImageIconList.Clear();
        }
        
    }

    // 이미지 슬라이더를 위한 크기 설정
    private void SetContentImageSize()
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
        for (int i = 0; i < testImageList.Count; i++)
        {
            if (imagePrefab)
            {
                Image newImage = Instantiate(imagePrefab, content);
                newImage.sprite = testImageList[i];
            }
            if (showActiveImagePrefab && showActiveImageHolder)
            {
                Image showActiveImage = Instantiate(showActiveImagePrefab, showActiveImageHolder);
                showActiveImageIconList.Add(showActiveImage);
            }
        }

        imageCount = testImageList.Count;

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
                    showActiveImageIconList[i].color = Color.yellow;
                }
                else
                {
                    showActiveImageIconList[i].color = Color.white;
                }
            }
            
        }
    }

    // 식물 정보 UI 세팅
    private void SetUI()
    {
        resultButton.onClick.RemoveAllListeners();

        bool isMissionInProgress = csMissionManager.Instance.IsMissonOnProgress;
        bool isSamePlant = isMissionInProgress &&
                           csMissionManager.Instance.GetCurrentMissionStep().plantName == currentPlantData.name;

        resultButton.onClick.AddListener(() =>
        {
            SaveCurrentPlantName();
        });
        // 버튼 텍스트변경과 함수 바인딩
        if (isSamePlant)
        {
            // 현재 미션과 같은 식물일 때 → 미션 클리어
            resultButton.onClick.AddListener(csMissionManager.Instance.ClearCurrentMissionStep);
            resultButton.GetComponentInChildren<TextMeshProUGUI>().text = "확인"; // 추후 로컬라제이션
        }
        else
        {
            // 미션이 없거나, 다른 식물일 때 → 사진 촬영 화면으로
            resultButton.onClick.AddListener(csObserveManager.Instance.SetCameraScreen);
            resultButton.GetComponentInChildren<TextMeshProUGUI>().text = "단서획득"; // 추후 로컬라제이션
        }
    }
    private void SaveCurrentPlantName()
    {
        string plantName = currentPlantData.name;

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



}

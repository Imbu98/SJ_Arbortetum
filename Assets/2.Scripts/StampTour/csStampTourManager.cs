using Data;
using UnityEngine;

public class csStampTourManager : MonoBehaviour
{
    public static csStampTourManager Instance { get { return _Instance; } }
    private static csStampTourManager _Instance;

    public csStampTourUIManager _stampTourUIManager;

    [HideInInspector]
    [SerializeField] private int _currentStampTourIndex;

    public int currentStampTourIndex
    {
        get { return _currentStampTourIndex; }
        set
        {
            _currentStampTourIndex = value;

            // 싱글톤에 즉시 반영
            csSingleton.Instance.stampTourProgressData.stampTourIndex = value;
        }
    }

    [HideInInspector]
    public int currentTourLocationIndex;

    [HideInInspector]
    public LocationData currentTourLocationData;


    // 만들어진 미션 목록 정보 저장
    private StampTourProgressData _currentStampTourProgressData;

    public StampTourProgressData currentStampTourProgressData
    {
        get { return _currentStampTourProgressData; }
        set
        {
            _currentStampTourProgressData = value;

            if (csSingleton.Instance.stampTourProgressData != null)
                csSingleton.Instance.stampTourProgressData = value;
        }
    }

    void Awake()
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

    private void Start()
    {
        currentStampTourProgressData = csSingleton.Instance.stampTourProgressData;

        // 스탬프투어가 하나도 없으면
        if (currentStampTourProgressData.stampTourInfoList.Count == 0)
        {
            CreateNewStampTour();
        }
    }

    private void CreateNewStampTour()
    {
        _currentStampTourIndex = -1;
        // stampTourBodyList 개수만큼 StampTourInfo 생성
        int bodyCount = _stampTourUIManager. stampTourBodyList.Count;

        for (int i = 0; i < bodyCount; i++)
        {
            StampTourInfo tourInfo = new StampTourInfo();

            tourInfo.IsCleared = false;

            // 해당 Body에 연결된 CourseList 개수만큼 StampTourCourseInfo 생성
            csStampTourBody body = _stampTourUIManager.stampTourBodyList[i];
            int courseCount = body.CourseList.Count;

            for (int c = 0; c < courseCount; c++)
            {
                tourInfo.stampTourCourseList.Add(new StampTourCourseInfo
                {
                    IsCleared = false
                });
            }

            // 리스트에 추가
            currentStampTourProgressData.stampTourInfoList.Add(tourInfo);
        }
        csSaveLodeManager.Instance.SaveStampTour();
    }

    public void SetStampTourClearUI()
    {
        currentStampTourProgressData.stampTourInfoList[currentStampTourIndex].stampTourCourseList[currentTourLocationIndex].IsCleared = true;

        bool allCoursesCleared = false;

        foreach (var course in currentStampTourProgressData.stampTourInfoList[currentStampTourIndex].stampTourCourseList)
        {
            if (!course.IsCleared)
            {
                allCoursesCleared = false;
                break;
            }
            allCoursesCleared = true;
        }

        if(allCoursesCleared)
        {
            currentStampTourProgressData.stampTourInfoList[currentStampTourIndex].IsCleared = true;
        }

        _stampTourUIManager.SetMissionBody();

        csSaveLodeManager.Instance.SaveStampTour();
    }

}

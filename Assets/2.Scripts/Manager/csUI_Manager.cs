using UnityEngine;

public class csUI_Manager : MonoBehaviour
{
    public static csUI_Manager Instance { get { return _Instance; } }
    private static csUI_Manager _Instance;

    public GameObject startScreen;
    public GameObject mainScreen;
    public GameObject mapScreen;
    public GameObject missionPopup;


    private GameObject currentScreen;
    private GameObject currentPanel;

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
    void Start()
    {
        
    }

    // 화면 전환 메서드
    public void ChangeScreen(GameObject newScreen)
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }
        newScreen.SetActive(true);
        currentScreen = newScreen;
    }

    // 메인 화면 내의 패널 전환 메서드
    public void ChangePanel(GameObject newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        newPanel.SetActive(true);
        currentPanel = newPanel;
    }

    // 지도 나타내기
    public void PopupMap(bool bShow)
    {
        mapScreen.SetActive(bShow);
    }

    // 미션창 나타내기

    public void PopupMission(bool bShow)
    {
        missionPopup.SetActive(bShow);
    }

}

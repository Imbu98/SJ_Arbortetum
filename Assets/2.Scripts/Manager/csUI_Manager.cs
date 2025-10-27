using UnityEngine;

public class csUI_Manager : MonoBehaviour
{
    public static csUI_Manager Instance { get { return _Instance; } }
    private static csUI_Manager _Instance;

    public GameObject startScreen;
    public GameObject mainScreen;
    public GameObject mapScreen;


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

    // ȭ�� ��ȯ �޼���
    public void ChangeScreen(GameObject newScreen)
    {
        if (currentScreen != null)
        {
            currentScreen.SetActive(false);
        }
        newScreen.SetActive(true);
        currentScreen = newScreen;
    }

    // ���� ȭ�� ���� �г� ��ȯ �޼���
    public void ChangePanel(GameObject newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        newPanel.SetActive(true);
        currentPanel = newPanel;
    }

    public void PopupMap(bool bShow)
    {
        mapScreen.SetActive(bShow);
    }

}

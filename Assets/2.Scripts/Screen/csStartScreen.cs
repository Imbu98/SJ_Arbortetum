using UnityEngine;
using UnityEngine.UI;

public class csStartScreen : MonoBehaviour
{
    [SerializeField] private Button startButton;

    private void Start()
    {
        csUI_Manager.Instance.ChangeScreen(csUI_Manager.Instance.startScreen);
    }
    void OnEnable()
    {
        startButton?.onClick.AddListener(OnStartButtonClicked);
    }

    // Update is called once per frame
    void OnDisable()
    {
        startButton?.onClick.RemoveListener(OnStartButtonClicked);
    }

    void OnStartButtonClicked()
    {
        csUI_Manager.Instance.ChangeScreen(csUI_Manager.Instance.mainScreen);
    }
}

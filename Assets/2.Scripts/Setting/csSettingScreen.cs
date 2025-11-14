using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csSettingScreen : MonoBehaviour
{
    

    [SerializeField] TextMeshProUGUI UID_TMP;
    [SerializeField] TextMeshProUGUI userNickName_TMP;

    [Header("Buttons")]
    [SerializeField] Button closeAppButton;
    [SerializeField] Button signOutButton;

    private void OnEnable()
    {
        signOutButton.onClick.AddListener(OnClickSignOutButton);

        InitUI();
    }

    private void OnDisable()
    {
        signOutButton.onClick.RemoveAllListeners();
    }

    private void InitUI()
    {
        userNickName_TMP.text = csSingleton.Instance.strPlayerNickName;
        UID_TMP.text = csSingleton.Instance.UID;
    }

    private void OnClickSignOutButton()
    {
        this.gameObject.SetActive(false);
        csLoginManager.Instance.SignOut();
    }


}

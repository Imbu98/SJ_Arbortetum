using UnityEngine;
using UnityEngine.UI;

public class csMissonCreating : MonoBehaviour
{
    [SerializeField] private Button cancelCreateMissonButton;
    private void OnEnable()
    {
        cancelCreateMissonButton.onClick.AddListener(OnClickCancelCreateMission);
    }
    private void OnDisable()
    {
        cancelCreateMissonButton.onClick.RemoveAllListeners();
    }

    private void OnClickCancelCreateMission()
    {
        csMissionManager.Instance.PopupCancleCreateMisson();
    }
}

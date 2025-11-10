using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PopupPart : MonoBehaviour
{
    [Header("Info Texts")]
    [SerializeField] private LocalizeTextSetter infoText;

    [Space(10)]
    [Header("Buttons")]
    [SerializeField] private LocalizeTextSetter buttonAText;
    [SerializeField] private Button buttonA;

    [SerializeField] private LocalizeTextSetter buttonBText;
    [SerializeField] private Button buttonB;

   

    private void OnEnable()
    {
        buttonA.onClick.RemoveAllListeners();
        buttonB.onClick.RemoveAllListeners();
        buttonA.gameObject.SetActive(false);
        buttonB.gameObject.SetActive(false);
    }

    public void InitText( string titleKey, string infoTable, string infoKey)
    {
        infoText.GetComponent<TMP_Text>().text = string.Empty;
        infoText.Init(infoTable, infoKey);
        buttonA.gameObject.SetActive(false);
        buttonB.gameObject.SetActive(false);
    }

    public void InitButtonA(string table, string key, UnityAction action)
    {
        buttonA.gameObject.SetActive(true);
        buttonAText.Init(table, key);
        buttonA.onClick.RemoveAllListeners();
        buttonA.onClick.AddListener(action);
    }

    public void InitButtonB(string table, string key, UnityAction action)
    {
        buttonB.gameObject.SetActive(true);
        buttonBText.Init(table, key);
        buttonB.onClick.RemoveAllListeners();
        buttonB.onClick.AddListener(action);
    }

    public void SetText(string info, string buttonA, string buttonB)
    {
        if (info != null)
            infoText.GetComponent<TMP_Text>().text = info;
        if (buttonA != null)
            buttonAText.GetComponent<TMP_Text>().text = buttonA;
        if (buttonB != null)
            buttonBText.GetComponent<TMP_Text>().text = buttonB;
    }
}

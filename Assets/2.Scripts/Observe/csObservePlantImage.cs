using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csObservePlantImagePrefab : MonoBehaviour
{
    public TextMeshProUGUI LicenseTextTMP;
    public TextMeshProUGUI ReferenceTextTMP;

    public void Init(string licenseText, string referenceText)
    {
        LicenseTextTMP.text = licenseText;
        ReferenceTextTMP.text = referenceText;
    }
}

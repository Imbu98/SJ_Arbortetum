using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class csObservePlantImagePrefab : MonoBehaviour
{
    public TextMeshProUGUI LicenseTextTMP;
    public TextMeshProUGUI ReferenceTextTMP;

    public void Init(string licenseText=null, string referenceText=null)
    {
        LicenseTextTMP.text = licenseText!=null?licenseText:string.Empty;
        ReferenceTextTMP.text = referenceText != null ? licenseText : string.Empty;
    }
}

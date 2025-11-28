using UnityEngine;

public class csStampTourSelectButtonController : MonoBehaviour
{
    [SerializeField] private GameObject clearObject;

    public void SetClearObjectActive(bool isActive)
    {
        if (clearObject != null)
        {
            clearObject.SetActive(isActive);
        }
    }
}

using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class csLoginManager : MonoBehaviour
{
    public static csLoginManager Instance { get { return _Instance; } }
    private static csLoginManager _Instance;

    private void Awake()
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


}

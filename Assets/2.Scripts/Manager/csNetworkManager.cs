using UnityEngine;
using Data;

public class csNetworkManager : MonoBehaviour
{
    public static csNetworkManager Instance { get { return _Instance; } }
    private static csNetworkManager _Instance;
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

    public MissionDto GetMissionDatas()
    {
        return null;
    }
}

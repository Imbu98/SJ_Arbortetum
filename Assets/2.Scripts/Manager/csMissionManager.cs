using UnityEngine;

public class csMissionManager : MonoBehaviour
{
    public static csMissionManager Instance { get { return _Instance; } }
    private static csMissionManager _Instance;

    // 몇 번째 미션인지 확인하는 인덱스
    [HideInInspector]public int currentMissionIndex = 0;
    // 미션 중인지 확인하는 인덱스
    [HideInInspector]public bool IsMissionPlaying = false;


    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class csMissionManager : MonoBehaviour
{
    public static csMissionManager Instance { get { return _Instance; } }
    private static csMissionManager _Instance;

    public csMissonUIManager _missonUIManager;


    // 어떤 스타일의 미션을 생성해야 하는지에 대한 변수
    [HideInInspector] private int missionStyle = -1;
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

    // 서버로부터 미션 받아오는 함수
    public void CreateMisson()
    {
        // 미션 생성중 창으로 변경
        _missonUIManager.ChangeMissonPanel(1);
    }
}

using UnityEngine;

public class csCreatedMissonPanel : MonoBehaviour
{
    // 켜질때마다 만들어진 미션 목록 초기화
    private void OnEnable()
    {
        csMissionManager.Instance.SetCreatedMissonUI(true);
    }
}

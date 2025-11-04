using System.Collections.Generic;
using UnityEngine;

public class csMissonUIManager : MonoBehaviour
{
    // 미션창 UI 전환용 패널 리스트
    [SerializeField] private List<GameObject> missonPanels;

    private GameObject currentMissonPanel;

    public void ChangeMissonPanel(int panelindex)
    {
        if(currentMissonPanel != null)
        {
            currentMissonPanel.SetActive(false);
        }

        missonPanels[panelindex].SetActive(true);
    }
}

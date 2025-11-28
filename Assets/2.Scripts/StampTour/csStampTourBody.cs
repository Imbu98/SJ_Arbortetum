using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csStampTourBody : MonoBehaviour
{
    [SerializeField] public List<csStampTourCourseButtonController> CourseList;

    private void OnEnable()
    {
        int currentStampTourIndex = csStampTourManager.Instance.currentStampTourIndex;

        for (int i=0; i<CourseList.Count; i++)
        {
            bool isCleared = csStampTourManager.Instance.currentStampTourProgressData.stampTourInfoList[currentStampTourIndex].stampTourCourseList[i].IsCleared;
            CourseList[i].SetCourseButtonUI(isCleared, i);
        }
    }
}

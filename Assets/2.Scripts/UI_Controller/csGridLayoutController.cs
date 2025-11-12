using UnityEngine;
using UnityEngine.UI;

// 이 컴포넌트는 GridLayoutGroup이 반드시 필요합니다.
[RequireComponent(typeof(GridLayoutGroup))]
public class csGridLayoutController : MonoBehaviour
{
    [Header("그리드 설정")]
    [Tooltip("한 줄에 표시할 열의 개수")]
    public int columnCount = 2;
    // 셀의 고정 높이 (필요에 따라 조정 가능)

    [Header("계산 기준 Transform")]
    [Tooltip("셀 크기 계산의 기준이 될 부모 RectTransform (일반적으로 Content)")]
    [SerializeField] private RectTransform contentTransform;

    private GridLayoutGroup grid;


    void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();

        if (contentTransform == null)
        {
            contentTransform = transform.parent.GetComponent<RectTransform>();
            Debug.LogWarning("contentTransform이 지정되지 않아 부모 RectTransform을 기준으로 설정합니다.", this.gameObject);
        }

        UpdateCellSize();
    }

    // RectTransform의 크기가 변경될 때마다 호출되는 유니티 이벤트 함수
    // 에디터에서나 실행 중에 창 크기가 바뀔 때 자동으로 셀 크기를 다시 계산
    private void OnRectTransformDimensionsChange()
    {
        if (grid != null && contentTransform != null)
        {
            UpdateCellSize();
        }
    }

    /// <summary>
    /// contentTransform의 너비에 맞춰 셀 크기를 다시 계산하고 적용합니다.
    /// </summary>
    public void UpdateCellSize()
    {
        // 컴포넌트가 준비되지 않았다면 함수를 종료하여 에러를 방지합니다.
        if (grid == null || contentTransform == null)
        {
            return;
        }

        // 1. 기준 너비 가져오기
        float contentWidth = contentTransform.rect.width;

        // 2. 여백 및 간격 계산
        float horizontalPadding = grid.padding.left + grid.padding.right;
        float totalSpacing = grid.spacing.x * (columnCount - 1);

        // 3. 최종 셀 너비 계산
        float cellWidth = (contentWidth - horizontalPadding - totalSpacing) / columnCount;


        // 4. 계산된 셀 크기 적용
        grid.cellSize = new Vector2(cellWidth,grid.cellSize.y);
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csUILineMeshRenderer : MaskableGraphic
{
    [Header("Line Settings")]
    public List<Vector2> points = new List<Vector2>();
    public float thickness = 10f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Count < 2)
            return;

        float half = thickness / 2f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 p0 = points[i];
            Vector2 p1 = points[i + 1];

            Vector2 dir = (p1 - p0).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x);

            // Quad 4점 생성
            Vector2 v0 = p0 + normal * half;
            Vector2 v1 = p0 - normal * half;
            Vector2 v2 = p1 - normal * half;
            Vector2 v3 = p1 + normal * half;

            int index = vh.currentVertCount;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = v0;
            vh.AddVert(vertex);

            vertex.position = v1;
            vh.AddVert(vertex);

            vertex.position = v2;
            vh.AddVert(vertex);

            vertex.position = v3;
            vh.AddVert(vertex);

            // Quad 2개의 삼각형 구성
            vh.AddTriangle(index + 0, index + 1, index + 2);
            vh.AddTriangle(index + 2, index + 3, index + 0);
        }
    }

    // 외부에서 좌표 세팅 후 즉시 선 갱신
    public void SetPoints(List<Vector2> positions)
    {
        points = positions;
        SetVerticesDirty(); // 다시 그려라
    }
}

using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : MaskableGraphic
{
    [Header("Line Settings")]
    [Tooltip("Points to draw the line through.")]
    public Vector2[] points;

    [Tooltip("Color of the rendered line.")]
    public Color color = Color.white;

    [Space]
    [Header("Appearance Settings")]

    [Tooltip("Thickness of the line (in pixels).")]
    [Range(0.1f, 100f)]
    public float thickness = 5f;

    [Tooltip("Spacing between circular segments (in pixels).")]
    [Range(0.1f, 100f)]
    public float spacing = 5f;

    [Tooltip("Number of segments used to draw each circle.")]
    [Range(3, 60)]
    public int segments = 12;


    private const int MaxVertices = 60000; // Limit to prevent overflow

    protected override void OnPopulateMesh(VertexHelper vh)
    {

        vh.Clear();

        if (points == null || points.Length < 2)
            return;

        float radius = Mathf.Max(thickness / 2f, 0.1f); // prevent 0 or negative radius

        int totalVerticesEstimate = 0;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 start = points[i];
            Vector2 end = points[i + 1];
            Vector2 dir = (end - start).normalized;
            float distance = Vector2.Distance(start, end);

            for (float d = 0; d < distance; d += Mathf.Max(spacing, 0.1f))
            {
                if (totalVerticesEstimate + segments + 2 >= MaxVertices)
                    return;

                Vector2 pos = start + dir * d;
                DrawCircle(pos, radius, vh, color);
                totalVerticesEstimate += segments + 2;
            }
        }
    }



    private void DrawCircle(Vector2 center, float radius, VertexHelper vh, Color color)
    {
        int startIndex = vh.currentVertCount;

        float angleStep = 360f / segments;
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        // Center vertex
        vertex.position = center;
        vh.AddVert(vertex);

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + center;
            vertex.position = pos;
            vh.AddVert(vertex);

            if (i > 0)
            {
                vh.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
            }
        }
    }

    public void ClearLine()
    {
        points = null;
        SetVerticesDirty(); // Triggers the mesh to be redrawn (in this case, cleared)
    }

}
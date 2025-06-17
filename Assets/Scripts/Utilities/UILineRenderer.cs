using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : MaskableGraphic
{
    public Vector2[] points;
    public Color color;
    public bool isCenter = true;
    public float thickness = 5f;
    public float spacing = 5f;
    public int segments = 12;



    protected override void OnPopulateMesh(VertexHelper vh)
    {

        vh.Clear();

        if (points == null || points.Length < 2)
            return;

        float radius = thickness / 2f;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 start = points[i];
            Vector2 end = points[i + 1];
            Vector2 dir = (end - start).normalized;
            float distance = Vector2.Distance(start, end);

            // Draw circles along the line
            for (float d = 0; d < distance; d += spacing)
            {
                Vector2 pos = start + dir * d;
                DrawCircle(pos, radius, vh, color);
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


    /// <summary>
    /// Creates a rect from two points that acts as a line segment
    /// </summary>
    /// <param name="point1">The starting point of the segment</param>
    /// <param name="point2">The endint point of the segment</param>
    /// <param name="vh">The vertex helper that the segment is added to</param>
    private void CreateLineSegment(Vector3 point1, Vector3 point2, VertexHelper vh)
    {
        Vector3 offset = !isCenter ? (rectTransform.sizeDelta / 2) : Vector2.zero;

        // Create vertex template
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = this.color;

        // Create the start of the segment
        Quaternion point1Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point1, point2) + 90);
        vertex.position = point1Rotation * new Vector3(-thickness / 2, 0);
        vertex.position += point1 - offset;
        vh.AddVert(vertex);
        vertex.position = point1Rotation * new Vector3(thickness / 2, 0);
        vertex.position += point1 - offset;
        vh.AddVert(vertex);

        // Create the end of the segment
        Quaternion point2Rotation = Quaternion.Euler(0, 0, RotatePointTowards(point2, point1) - 90);
        vertex.position = point2Rotation * new Vector3(-thickness / 2, 0);
        vertex.position += point2 - offset;
        vh.AddVert(vertex);
        vertex.position = point2Rotation * new Vector3(thickness / 2, 0);
        vertex.position += point2 - offset;
        vh.AddVert(vertex);

        // Also add the end point
        vertex.position = point2 - offset;
        vh.AddVert(vertex);
    }

    /// <summary>
    /// Gets the angle that a vertex needs to rotate to face target vertex
    /// </summary>
    /// <param name="vertex">The vertex being rotated</param>
    /// <param name="target">The vertex to rotate towards</param>
    /// <returns>The angle required to rotate vertex towards target</returns>
    private float RotatePointTowards(Vector2 vertex, Vector2 target)
    {
        return (float)(Mathf.Atan2(target.y - vertex.y, target.x - vertex.x) * (180 / Mathf.PI));
    }
}
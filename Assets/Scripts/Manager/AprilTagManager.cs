using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AprilTagManager : MonoBehaviour
{

    [Header("References")]
    public UILineRenderer lineRenderer;

    private Texture2D currentFrame;

    public void SendCapturedFrame()
    {
        // Dispose of old frame
        if (currentFrame != null)
        {
            Destroy(currentFrame);
            currentFrame = null;
        }

        currentFrame = WebcamManager.Instance.CaptureFrame();
        if (currentFrame != null)
        {
            WebcamManager.Instance.capturedImage.texture = currentFrame;
            StartCoroutine(SendToServer(currentFrame));
        }
    }

    private IEnumerator SendToServer(Texture2D frame)
    {
        byte[] imageBytes = frame.EncodeToPNG();

        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "frame.png", "image/png");

        using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:5000/detect", form))
        {
            request.SetRequestHeader("Accept", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log("Server Response: " + responseText);

            try
            {
                DetectionWrapper tagResponse = JsonUtility.FromJson<DetectionWrapper>(responseText);

                if (tagResponse.detections == null || tagResponse.detections.Count == 0)
                {
                    Debug.LogWarning("No tags detected in the response.");
                    yield break;
                }

                DisplayDetections(tagResponse);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse server response: {ex.Message}");
            }
        }
    }

    private void DisplayDetections(DetectionWrapper tagResponse)
    {
        foreach (var detection in tagResponse.detections)
        {
            DrawTagOutline(detection);
        }
    }

    private void DrawTagOutline(AprilTagDeserializer detection)
    {
        int cornerCount = detection.corners.Count;
        if (cornerCount == 0) return;

        Vector2[] cornerPoints = new Vector2[cornerCount + 1];
        RectTransform imageRect = WebcamManager.Instance.capturedImage.rectTransform;
        Texture2D frame = currentFrame;

        for (int i = 0; i < cornerCount; i++)
        {
            cornerPoints[i] = ConvertToUILocalPosition(detection.corners[i], frame, imageRect);
        }

        cornerPoints[cornerCount] = cornerPoints[0]; // Close the loop

        lineRenderer.points = cornerPoints;
        lineRenderer.color = Color.green;
        lineRenderer.SetAllDirty();
    }

    private Vector2 ConvertToUILocalPosition(Vector2 pixelCoord, Texture2D frame, RectTransform imageRect)
    {
        // Convert to normalized (0–1)
        float normX = pixelCoord.x/frame.width ;
        float normY = pixelCoord.y/ frame.height ;

        // Flip Y (image origin = top-left or bottom-left?)
        normY = 1f - normY;

        // Map to rect
        float localX = normX * imageRect.rect.width;
        float localY = normY * imageRect.rect.height;

        // Adjust for pivot being in center
        return new Vector2(
            localX,
            localY
        );
    }

    // JSON Wrapper Classes
    [Serializable]
    public class DetectionWrapper
    {
        public List<AprilTagDeserializer> detections;
    }

    [Serializable]
    public class AprilTagDeserializer
    {
        public Vector2 center;
        public List<Vector2> corners;
        public int id;
    }
}

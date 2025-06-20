using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.UI.GridLayoutGroup;

public class AprilTagDetector : MonoBehaviour
{

    // === Inspector References ===
    [Header("References")]
    public UILineRenderer lineRenderer;
    public AprilTagDisplayManager tagDisplayManager;

    // === Runtime Data ===
    private Texture2D currentFrame;

    public void SendCapturedFrame()
    {
        // Dispose previous frame and clear overlays.
        if (currentFrame != null)
        {
            Destroy(currentFrame);
            currentFrame = null;

            var keys = new List<int>(tagDisplayManager.activeImageOverlays.Keys);
            foreach (int key in keys)
            {
                Destroy(tagDisplayManager.activeImageOverlays[key]);
            }
            tagDisplayManager.activeImageOverlays.Clear();
            lineRenderer.ClearLine();
        }

        // Capture new frame and update the UI image.
        currentFrame = WebcamManager.Instance.CaptureFrame();
        if (currentFrame != null)
        {
            WebcamManager.Instance.capturedImage.texture = currentFrame;
            StartCoroutine(SendToServer(currentFrame));
        }
    }

    // === Send Captured Frame to Python Server for April Tag detection ===
    private IEnumerator SendToServer(Texture2D frame)
    {
        byte[] imageBytes = frame.EncodeToPNG();

        // Build form data and request.
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

    // === Handle Detection Results ===
    private void DisplayDetections(DetectionWrapper tagResponse)
    {
        foreach (var detection in tagResponse.detections)
        {
            // Parse center and corner position of the tag.
            int cornerCount = detection.corners.Count;
            if (cornerCount == 0) return;

            Vector2[] cornerPoints = new Vector2[cornerCount + 1];
            RectTransform imageRect = WebcamManager.Instance.capturedImage.rectTransform;


            // Convert corners from pixel to UI space.
            for (int i = 0; i < cornerCount; i++)
            {
                cornerPoints[i] = ConvertToUILocalPosition(detection.corners[i], currentFrame, imageRect);
            }

            // Close the loop for the line renderer.
            cornerPoints[cornerCount] = cornerPoints[0];

            // Calculate tag size & position in uI space.
            float width = Mathf.Abs(cornerPoints[2].x - cornerPoints[0].x) - 50;
            float height = Mathf.Abs(cornerPoints[2].y - cornerPoints[0].y) - 50;
            Vector2 tagSize = new Vector2(width, height);
            Vector2 tagCenterPos = ConvertToUILocalPosition(detection.center, currentFrame, imageRect);


            // Display corresponding overlay image and draw border.
            tagDisplayManager.DisplayImageAtTag(detection.id,tagCenterPos, new Vector2(width, height));
            DrawTagOutline(cornerPoints);
        }
    }

    // === Draw Line Border Around Detected Tag ===
    private void DrawTagOutline(Vector2[] cornerPoints)
    {
        lineRenderer.points = cornerPoints;
        lineRenderer.color = Color.green;
        lineRenderer.SetAllDirty();
    }


    // === Convert Pixel to UI Local Position ===
    private Vector2 ConvertToUILocalPosition(Vector2 pixelCoord, Texture2D frame, RectTransform imageRect)
    {
        // Normalize coordinates to 0–1 range.
        float normX = pixelCoord.x / frame.width;
        float normY = pixelCoord.y / frame.height;

         // Flip Y (image origin = top-left or bottom-left?)
        normY = 1f - normY;

        // Convert to local position within the rect.
        float localX = normX * imageRect.rect.width;
        float localY = normY * imageRect.rect.height;

        // Adjust for pivot at center of rect.
        return new Vector2(
            localX - imageRect.rect.width / 2f,
            localY - imageRect.rect.height / 2f
        );
    }


    // === JSON Wrapper Classes ===
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

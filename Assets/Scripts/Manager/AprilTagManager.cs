using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AprilTagManager : MonoBehaviour
{
    public void SendCapturedFrame()
    {
        Texture2D frame = WebcamManager.Instance.CaptureFrame();
        if (frame != null)
        {
            WebcamManager.Instance.capturedImage.texture = frame;
            StartCoroutine(SendToServer(frame));
        }
    }

    private IEnumerator SendToServer(Texture2D frame)
    {
        byte[] imageBytes = frame.EncodeToPNG();

        // Create form and attach image
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, "frame.png", "image/png");

        using (UnityWebRequest request = UnityWebRequest.Post("http://localhost:5000/detect", form))
        {
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("Server Response: " + responseText);

                // Parse the tag ID
                try
                {
                    var tagResponse = JsonUtility.FromJson<TagResponse>(responseText);
                    Debug.Log("Detected AprilTag ID: " + tagResponse.id);
                }
                catch
                {
                    Debug.LogWarning("Failed to parse server response.");
                }
            }
        }
    }

    // Helper class for JSON parsing
    [System.Serializable]
    private class TagResponse
    {
        public int id;
    }
}

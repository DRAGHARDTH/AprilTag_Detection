using UnityEngine;
using UnityEngine.UI;

public class WebcamManager : MonoBehaviour
{
    // Singleton set up.
    public static WebcamManager Instance { get; private set; }



    [Header("Dependencies")]
    private WebCamTexture webcamTexture;
    public RawImage webcamOutput;
    public RawImage capturedImage;

    public bool IsWebcamAvailable => webcamTexture != null && webcamTexture.isPlaying;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Enforce singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private void Start()
    {
        InitializeWebcam();
    }

    private void InitializeWebcam()
    {
        // Early checkouts.
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No webcam detected!");
            return;
        }

        // Use the first available webcam
        WebCamDevice device = WebCamTexture.devices[0];
        webcamTexture = new WebCamTexture(device.name);

        webcamTexture.Play();

        if (webcamOutput != null)
        {
            webcamOutput.texture = webcamTexture;
        }

        Debug.Log("Webcam started: " + device.name);
    }

    public Texture2D CaptureFrame()
    {
        if (!IsWebcamAvailable)
        {
            Debug.LogWarning("Webcam not ready.");
            return null;
        }

        Texture2D frame = new Texture2D(webcamTexture.width, webcamTexture.height);
        frame.SetPixels(webcamTexture.GetPixels());
        frame.Apply();

        return frame;
    }

    public void StopWebcam()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    private void OnDestroy()
    {
        StopWebcam();
    }
}

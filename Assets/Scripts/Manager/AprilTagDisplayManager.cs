using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AprilTagDisplayManager : MonoBehaviour
{
    // === Inspector References ===
    [Header("Image Display Settings")]
    [Tooltip("Parent UI container where tag images will be instantiated.")]
    [SerializeField] private RectTransform imageDisplayParent;

    [Tooltip("Prefab containing a RawImage to display the tag-associated sprite.")]
    [SerializeField] private GameObject imagePrefab;

    [Header("Mapping Configuration")]
    [Tooltip("JSON file inside StreamingAssets that maps tag IDs to image file names.")]
    [SerializeField] private string jsonFileName = "aprilTag_image_map.json";

    // === Runtime Data ===
    private Dictionary<int, string> idToImagePath;                  // Maps tag ID to image file name.
    public Dictionary<int, GameObject> activeImageOverlays = new(); // Track spawned overlays by ID.

    private void Start()
    {
        LoadIdToImageMap();
    }

    // === Load and Deserialize the JSON Mapping ===
    private void LoadIdToImageMap()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        // Ensure file exists before reading.
        if (!File.Exists(filePath))
        {
            Debug.LogError("ID-to-image map not found at: " + filePath);
            return;
        }

        // Read and parse JSON content.
        string jsonContent = File.ReadAllText(filePath);
        IdImageMapWrapper wrapper = JsonUtility.FromJson<IdImageMapWrapper>(jsonContent);
        idToImagePath = wrapper.ToDictionary();
    }

    // === Display Mapped Image at AprilTag Position ===
    public void DisplayImageAtTag(int id, Vector2 centerUIPosition, Vector2 size)
    {
        // Attempt to get the mapped image name.
        string imageName = idToImagePath.ContainsKey(id) ? idToImagePath[id] : null;
        Sprite sprite = null;
        string errorMsg = default;

        if (!string.IsNullOrEmpty(imageName))
        {
            // Build Resources path without extension and attempt to load sprite.
            string resourcePath = Path.Combine("TagImages", Path.GetFileNameWithoutExtension(imageName));
            sprite = Resources.Load<Sprite>(resourcePath);

            if (sprite == null)
            {
                errorMsg = $"Image mapping exists but failed to load sprite for Tag ID {id} at: {resourcePath}";
            }
        }
        else
        {
            errorMsg = $"No image mapped for Tag ID: {id}";
        }

        // Instantiate the image object at the desired position and scale.
        GameObject imgObj = Instantiate(imagePrefab, imageDisplayParent);
        RectTransform rect = imgObj.GetComponent<RectTransform>();
        TextMeshProUGUI tagID = imgObj.GetComponentInChildren<TextMeshProUGUI>();
        rect.anchoredPosition = centerUIPosition;
        rect.sizeDelta = size; // Set size based on tag area

        // Assign sprite texture to RawImage component.
        RawImage rawImage = imgObj.GetComponent<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = sprite != null ? sprite.texture : null;
        }
        else
        {
            Debug.LogWarning("RawImage component not found on prefab.");
        }

        // Display Tag id.
        if (tagID != null)
        {
            tagID.text = sprite != null ? 
                $"Detected Tag ID: {id}"
                : errorMsg;
        }

        // Cache overlay by tag ID.
        activeImageOverlays[id] = imgObj;

        // Log warning if sprite wasn't found.
        if (!string.IsNullOrEmpty(errorMsg))
        {
            Debug.LogWarning(errorMsg);
        }
    }

    // === JSON Wrapper Classes ===
    [Serializable]
    private class IdImageMapWrapper
    {
        public List<IdImagePair> entries;

        // Convert list to dictionary.
        public Dictionary<int, string> ToDictionary()
        {
            Dictionary<int, string> dict = new();
            foreach (var pair in entries)
            {
                dict[pair.id] = pair.image;
            }
            return dict;
        }
    }

    [Serializable]
    private class IdImagePair
    {
        public int id;
        public string image;
    }
}

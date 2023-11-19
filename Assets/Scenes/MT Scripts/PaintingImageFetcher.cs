using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PaintingImageFetcher : MonoBehaviour
{
    public GameObject displayImage; // Assign this in the Unity Editor
    public Text titleText; // Assign the text UI object in the Unity Editor

    private HttpClient httpClient = new HttpClient();
    private List<(string url, string title)> imageInfo = new List<(string url, string title)>();
    private int currentImageIndex = 0;

    void Start()
    {
        if (displayImage == null || titleText == null)
        {
            Debug.LogError("Display Image or Title Text component not assigned.");
            return;
        }

        StartCoroutine(Slideshow());
    }

    public void StartFetchingImageUrl(string paintingTitle)
    {
        FetchPaintingImageUrl(paintingTitle);
    }

    private async void FetchPaintingImageUrl(string paintingTitle)
    {
        string apiUrl = ConstructQuery(paintingTitle);
        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            string imageUrl = ExtractImageUrl(jsonResponse);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                imageInfo.Add((imageUrl, paintingTitle));
                Debug.Log("Image URL added: " + imageUrl);
            }
            else
            {
                Debug.LogError("No image found for the given title.");
            }
        }
        else
        {
            Debug.LogError("API request failed: " + response.ReasonPhrase);
        }
    }

    private IEnumerator Slideshow()
    {
        while (true)
        {
            if (imageInfo.Count > 0 && currentImageIndex < imageInfo.Count)
            {
                var (url, title) = imageInfo[currentImageIndex];
                StartCoroutine(DownloadImage(url));
                titleText.text = title;
                currentImageIndex = (currentImageIndex + 1) % imageInfo.Count;
            }
            yield return new WaitForSeconds(3);
        }
    }

    private IEnumerator DownloadImage(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Texture downloadedTexture = DownloadHandlerTexture.GetContent(webRequest);

                float aspectRatio = (float)downloadedTexture.width / downloadedTexture.height;

                // Get the current scale of the quad
                Vector3 currentScale = displayImage.transform.localScale;

                // Set the new scale based on the aspect ratio
                displayImage.transform.localScale = new Vector3(currentScale.y * aspectRatio, currentScale.y, currentScale.z);

                displayImage.GetComponent<Renderer>().material.mainTexture = downloadedTexture;
            }
        }
    }

    private string ExtractImageUrl(string json)
    {
        string imageUrlMarker = "\"source\":\"";
        int sourceIndex = json.IndexOf(imageUrlMarker);

        if (sourceIndex != -1)
        {
            int startIndex = sourceIndex + imageUrlMarker.Length;
            int endIndex = json.IndexOf("\"", startIndex);

            if (endIndex != -1)
            {
                return json.Substring(startIndex, endIndex - startIndex);
            }
        }

        return null;
    }

    private string ConstructQuery(string title)
    {
        string baseUrl = "https://en.wikipedia.org/w/api.php";
        string query = "?action=query&format=json&prop=pageimages&titles=" + Uri.EscapeDataString(title) + "&piprop=thumbnail&pithumbsize=500";
        return baseUrl + query;
    }
}



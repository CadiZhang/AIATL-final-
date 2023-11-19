using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using OpenAI;

public class ChatGPTStream : MonoBehaviour
{
    [SerializeField] private InputField inputField;
    [SerializeField] private Button button;
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;
    [SerializeField] private Text text; // Your debug canvas

    private ElevenLabs elevenlabsAPI;


    private RectTransform currentStreamingResponseUI = null;
    private float height;
    private OpenAIApi openai = new OpenAIApi("");
    private CancellationTokenSource token = new CancellationTokenSource();

    private List<ChatMessage> messages = new List<ChatMessage>();

    private int contentChunkCounter = 0;  // Counter for content chunks
    private int messageCounter = 0;
    private string concatenatedResponseText = "";

    private void Start()
    {
        button.onClick.AddListener(SendMessageToAPI);
        elevenlabsAPI = FindObjectOfType<ElevenLabs>();

    }

    private void AppendMessage(ChatMessage message)
    {
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
        item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
        item.anchoredPosition = new Vector2(0, -height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        height += item.sizeDelta.y;
        scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        scroll.verticalNormalizedPosition = 0;
    }



    public void SendMessageToAPI()
    {
        concatenatedResponseText = "";
        button.enabled = false;
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = inputField.text
        };
        messages.Add(newMessage);
        AppendMessage(newMessage);

        openai.CreateChatCompletionAsync(new CreateChatCompletionRequest()
        {
            Model = "gpt-4",
            Messages = messages,
            Stream = true
        }, HandleResponse, HandleCompletion, token);

        inputField.text = "";
        button.enabled = true;
        messageCounter++;
    }

    private void HandleResponse(List<CreateChatCompletionResponse> responses)
    {
        if (responses == null || responses.Count == 0)
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return;
        }

        String contentChunk = responses.Select(r => r.Choices[0].Delta.Content).Aggregate((a, b) => a + b).Trim();
        //concatenatedResponseText += contentChunk;
        // Debug canvas update
        text.text = contentChunk;

        if (currentStreamingResponseUI == null)
        {
            var newMessage = new ChatMessage()
            {
                Role = "system",
                Content = ""
            };
            messages.Add(newMessage);
            var item = Instantiate(received, scroll.content);
            currentStreamingResponseUI = item;
            item.anchoredPosition = new Vector2(0, -height);
        }

        var currentTextComponent = currentStreamingResponseUI.GetChild(0).GetChild(0).GetComponent<Text>();
        currentTextComponent.text = contentChunk;

        concatenatedResponseText = currentTextComponent.text;

        Debug.Log("Content Chunk: " + contentChunk);

    }


    private void HandleCompletion()
    {
        Debug.Log("Streamed message completed.");
        // Call Elevenlabs API to read the complete text aloud
        Debug.Log(concatenatedResponseText);
        if (!string.IsNullOrEmpty(concatenatedResponseText))
        {
            elevenlabsAPI.GetAudio(concatenatedResponseText);
            concatenatedResponseText = ""; // Reset the text for next use
        }

        if (currentStreamingResponseUI != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(currentStreamingResponseUI);
            height += currentStreamingResponseUI.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
            currentStreamingResponseUI = null;
        }
    }




    private void OnDestroy()
    {
        token.Cancel();
    }
}
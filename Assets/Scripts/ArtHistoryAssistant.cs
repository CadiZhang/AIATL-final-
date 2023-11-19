using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Add this line
using OpenAI;

public class ArtHistoryAssistant : MonoBehaviour
{
    [SerializeField] private Whisper1 whisperScript;
    [SerializeField] private PaintingImageFetcher imageFetcherScript;

    private List<ChatMessage> messages = new List<ChatMessage>();

    private OpenAIApi openai = new OpenAIApi("");

    private async void Start()
    {
        // Initialize or start the Whisper script to record and transcribe audio
        // For example, you might have a method in Whisper1 like:
        // whisperScript.StartRecording();
        // Make sure to call EndRecording() at the right time to get the transcription
    }

    public async void ProcessTranscribedText(string transcribedText)
    {
        string instruction = "You're an art history teacher. Respond with a list of the 10 most important titles of artworks from the specified period in history in the format: 'Title 1, Title 2, Title 3, Title 4, Title 5'. For example, the output format should be like: 'The Last Supper, Mona Lisa, The Creation of Adam, The Birth of Venus, The School of Athens";
        var prompt = $"{instruction}\nQuestion: {transcribedText}";

        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = prompt
        };

        messages.Add(newMessage);

        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4",
            Messages = messages,
        });

        string response = completionResponse.Choices[0].Message.Content.Trim(); // Replace 'Message.Content' with the correct property path
        List<string> titles = ParseTitles(response);
        Debug.Log("RESPONSE : " + response);

        foreach (var title in titles)
        {
            imageFetcherScript.StartFetchingImageUrl(title);
            Debug.Log("TITLE : " + title);
        }
    }

    private List<string> ParseTitles(string response)
    {
        // Assuming the response is in the format 'Title 1, Title 2, ...'
        return response.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}

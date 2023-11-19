using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq; // Add this line
using OpenAI;

public class ArtworkLecture : MonoBehaviour
{
    [SerializeField] private Whisper1 whisperScript;
    private ElevenLabs elevenlabsAPI;

    private List<ChatMessage> messages = new List<ChatMessage>();

    private OpenAIApi openai = new OpenAIApi("");

    private async void Start()
    {
        // Initialize or start the Whisper script to record and transcribe audio
        // For example, you might have a method in Whisper1 like:
        // whisperScript.StartRecording();
        // Make sure to call EndRecording() at the right time to get the transcription
        elevenlabsAPI = FindObjectOfType<ElevenLabs>();
    }

    public async void ProcessTranscribedText(string transcribedText)
    {
        string instruction = "You're an art history teacher. Give me a 20 second summary of the significance about whatever artwork asked about:";
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

        if (!string.IsNullOrEmpty(response))
        {
            Debug.Log(response);
            elevenlabsAPI.GetAudio(response);
            response = ""; // Reset the text for next use
        }
    }

}

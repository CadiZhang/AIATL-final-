using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace OpenAI
{
    public class Whisper1 : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private Text message;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private InputField inputFieldMessage;
        [SerializeField] private Text audioSizeText;
        [SerializeField] private InputActionReference xButtonAction;
        [SerializeField] private InputActionReference yButtonAction;
        [SerializeField] private InputActionReference aButtonAction;
        [SerializeField] private InputActionReference bButtonAction;
        [SerializeField] private ArtHistoryAssistant artHistoryAssistant;
        [SerializeField] private ArtLecture artLecture;
        [SerializeField] private ArtworkLecture artworkLecture;


        private readonly string fileName = "output.wav";

        private AudioClip clip;
        private bool isRecording;
        private OpenAIApi openai = new OpenAIApi("");

        private void Start()
        {

            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(ChangeMicrophone);

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);

            xButtonAction.action.started += _ => StartRecording();
            yButtonAction.action.started += _ => EndRecording();
            aButtonAction.action.started += _ => StartRecording1();
            bButtonAction.action.started += _ => EndRecording1();
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }

        public void StartRecording()
        {
            Debug.Log("Start Recording called.");
            if (isRecording) return; // Prevent restarting the recording if it's already in progress.

            isRecording = true;
            message.text = "Listening...";
            var index = PlayerPrefs.GetInt("user-mic-device-index");

#if !UNITY_WEBGL
            clip = Microphone.Start(dropdown.options[index].text, false, 20, 44100);            // Use int.MaxValue for "unlimited" time, but keep in mind, it's still limited.
            Debug.Log("Is Recording? " + Microphone.IsRecording(dropdown.options[index].text));

#endif
        }

        public async void EndRecording()
        {
            Debug.Log("End Recording called.");
            if (!isRecording) return;
            message.text = "Transcripting...";
            if (Microphone.IsRecording(dropdown.options[PlayerPrefs.GetInt("user-mic-device-index")].text))
            {
                Microphone.End(dropdown.options[PlayerPrefs.GetInt("user-mic-device-index")].text);
            }

            if (clip != null)
            {
                Debug.Log("Clip is not null.");
                clip = SaveWav1.TrimSilence(clip, 0.01f);
                byte[] data = SaveWav1.Save(fileName, clip);

                Debug.Log($"Data array is {(data != null ? "not null" : "null")}");
                if (data != null)
                {
                    Debug.Log($"Data Length: {data.Length}");
                }

                var req = new CreateAudioTranscriptionsRequest
                {
                    FileData = new FileData() { Data = data, Name = "audio.wav" },
                    Model = "whisper-1",
                    Language = "en"
                };

                var res = await openai.CreateAudioTranscription(req);
                Debug.Log($"API Response: {res}");

                progressBar.fillAmount = 0;
                message.text = res.Text;
                //inputFieldMessage.text = res.Text;
                isRecording = false;

                // Use the declared reference to ArtHistoryAssistant
                if (artHistoryAssistant != null)
                {
                    artHistoryAssistant.ProcessTranscribedText(res.Text);
                }
                else
                {
                    Debug.LogError("ArtHistoryAssistant reference not set.");
                }
                if (artLecture != null)
                {
                    artLecture.ProcessTranscribedText(res.Text);
                }
                else
                {
                    Debug.LogError("ArtLecture reference not set.");
                }
            }
            else
            {
                message.text = "Null clip";
            }
        }



        private void Update()
        {
            if (isRecording)
            {
                // We're keeping this here in case you'd like to add a visual progress indicator in the future.
                // time += Time.deltaTime;
                // progressBar.fillAmount = time / duration;
            }
        }


        public void StartRecording1()
        {
            Debug.Log("Start Recording called.");
            if (isRecording) return; // Prevent restarting the recording if it's already in progress.

            isRecording = true;
            message.text = "Listening...";
            var index = PlayerPrefs.GetInt("user-mic-device-index");

#if !UNITY_WEBGL
            clip = Microphone.Start(dropdown.options[index].text, false, 20, 44100);            // Use int.MaxValue for "unlimited" time, but keep in mind, it's still limited.
            Debug.Log("Is Recording? " + Microphone.IsRecording(dropdown.options[index].text));

#endif
        }

        public async void EndRecording1()
        {
            Debug.Log("End Recording called.");
            if (!isRecording) return;
            message.text = "Transcripting...";
            if (Microphone.IsRecording(dropdown.options[PlayerPrefs.GetInt("user-mic-device-index")].text))
            {
                Microphone.End(dropdown.options[PlayerPrefs.GetInt("user-mic-device-index")].text);
            }

            if (clip != null)
            {
                Debug.Log("Clip is not null.");
                clip = SaveWav1.TrimSilence(clip, 0.01f);
                byte[] data = SaveWav1.Save(fileName, clip);

                Debug.Log($"Data array is {(data != null ? "not null" : "null")}");
                if (data != null)
                {
                    Debug.Log($"Data Length: {data.Length}");
                }

                var req = new CreateAudioTranscriptionsRequest
                {
                    FileData = new FileData() { Data = data, Name = "audio.wav" },
                    Model = "whisper-1",
                    Language = "en"
                };

                var res = await openai.CreateAudioTranscription(req);
                Debug.Log($"API Response: {res}");

                progressBar.fillAmount = 0;
                message.text = res.Text;
                //inputFieldMessage.text = res.Text;
                isRecording = false;


                if (artLecture != null)
                {
                    artLecture.ProcessTranscribedText(res.Text);
                }
                else
                {
                    Debug.LogError("ArtLecture reference not set.");
                }
            }
            else
            {
                message.text = "Null clip";
            }
        }
    }
}

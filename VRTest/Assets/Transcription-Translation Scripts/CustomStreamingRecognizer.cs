using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Google.Cloud.Speech.V1;
using Grpc.Core;
using Google.Apis.Auth.OAuth2;
//using Google.Apis.Auth;
using Grpc.Auth;
using TMPro;
using UnityEngine.UI;

namespace GoogleCloudStreamingSpeechToText
{
    [Serializable]
    public class TranscriptionEvent : UnityEvent<string> { }
    [RequireComponent(typeof(AudioSource))]
    public class CustomStreamingRecognizer : MonoBehaviour
    {
        public string microphoneName
        {
            get => _microphoneName;
            set
            {
                if (_microphoneName == value)
                {
                    return;
                }

                _microphoneName = value;
                if (Application.isPlaying && IsListening())
                {
                    Restart();
                }
            }
        }

        public string lang = "en_US";
        public bool dormant = true;
        public bool startOnAwake = true;
        public bool returnInterimResults = true;
        public bool enableDebugLogging = false;
        public UnityEvent onStartListening;
        public UnityEvent onStopListening;
        public TranscriptionEvent onFinalResult = new TranscriptionEvent();
        public TranscriptionEvent onInterimResult = new TranscriptionEvent();

        private bool _initialized = false;
        private bool _listening = false;
        private bool _restart = false;
        private bool _newStreamOnRestart = false;
        private bool _newStream = false;
        [SerializeField] private string _microphoneName;
        private AudioSource _audioSource;
        private CancellationTokenSource _cancellationTokenSource;
        private byte[] _buffer;
        private SpeechClient.StreamingRecognizeStream _streamingCall;
        private List<ByteString> _audioInput = new List<ByteString>();
        private List<ByteString> _lastAudioInput = new List<ByteString>();
        private int _resultEndTime = 0;
        private int _isFinalEndTime = 0;
        private int _finalRequestEndTime = 0;
        private double _bridgingOffset = 0;

        private const string CredentialFileName = "gcp_credentials.json";
        private const double NormalizedFloatTo16BitConversionFactor = 0x7FFF + 0.4999999999999999;
        private const float MicInitializationTimeout = 1;
        private const int StreamingLimit = 290000; // almost 5 minutes

        public TextMeshPro debugStr;

        public Dropdown sourceDrop;


        public string languageCode(string lang)
        {
            if(lang == "English (United States)")
            {
                return "en";
            }

            if(lang == "Spanish (Mexico)")
            {
                return "es";
            }

            if(lang == "French (France)")
            {
                return "fr";
            }

            return "en_US";
        }

        public void changeLang(int i)
        {
            ChangeLanguage(languageCode(sourceDrop.options[sourceDrop.value].text));
        }


        public void Log(String str)
        {
            Debug.Log(str);
            debugStr.text = str;
        }

        public void StartListening()
        {
            if (!_initialized)
            {
                return;
            }

            StartCoroutine(nameof(RequestMicrophoneAuthorizationAndStartListening));
        }

        public void makeDormant()
        {
            dormant = true;
        }

        public void makeNotDormant()
        {
            dormant = false;
        }

        public async void StopListening()
        {
            if (!_initialized || _cancellationTokenSource == null)
            {
                return;
            }

            try
            {
                Task whenCanceled = Task.Delay(Timeout.InfiniteTimeSpan, _cancellationTokenSource.Token);

                _cancellationTokenSource.Cancel();

                try
                {
                    await whenCanceled;
                }
                catch (TaskCanceledException)
                {
                    if (enableDebugLogging)
                    {
                        Log("Stopped.");
                    }

                }
            }
            catch (ObjectDisposedException) { }
        }

        public bool IsListening()
        {
            return _listening;
        }


        public void ChangeLanguage(string language)
        {
            lang = language;
            Restart();
        }

        private void Restart()
        {
            if (!_initialized)
            {
                return;
            }

            _restart = true;
            StopListening();
        }

        private void Awake()
        {
            //string credentialsPath = /*Application.persistentDataPath + "/temp.json";*/ Path.Combine(Application.streamingAssetsPath, CredentialFileName);
            /*if (!File.Exists(credentialsPath))
			{
				Log("Could not find StreamingAssets/gcp_credentials.json. Please create a Google service account key for a Google Cloud Platform project with the Speech-to-Text API enabled, then download that key as a JSON file and save it as StreamingAssets/gcp_credentials.json in this project. For more info on creating a service account key, see Google's documentation: https://cloud.google.com/speech-to-text/docs/quickstart-client-libraries#before-you-begin");
				return;
			}*/

            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();
            audioConfiguration.dspBufferSize = 1024;
            AudioSettings.Reset(audioConfiguration);

            _buffer = new byte[audioConfiguration.dspBufferSize * 2];

            _audioSource = gameObject.GetComponent<AudioSource>();
            AudioMixer audioMixer = (AudioMixer)Resources.Load("MicrophoneMixer");
            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups("MuteMicrophone");
            if (audioMixerGroups.Length > 0)
            {
                _audioSource.outputAudioMixerGroup = audioMixerGroups[0];
            }

            string[] microphoneDevices = Microphone.devices;
            if (string.IsNullOrEmpty(_microphoneName) || Array.IndexOf(microphoneDevices, _microphoneName) == -1)
            {
                _microphoneName = microphoneDevices[0];
            }

            _initialized = true;

            if (startOnAwake)
            {
                StartListening();
            }
        }

        private void OnDestroy()
        {
            if (!_initialized)
            {
                return;
            }

            Microphone.End(Microphone.devices[0]);
            _audioSource.Stop();
            _cancellationTokenSource?.Dispose();
        }

        private async void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_listening || dormant)
            {
                //print("listening: " + _listening.ToString());
                return;
            }

            if (_newStream && _lastAudioInput.Count != 0)
            {
                // Approximate math to calculate time of chunks
                double chunkTime = StreamingLimit / (double)_lastAudioInput.Count;
                if (!Mathf.Approximately((float)chunkTime, 0))
                {
                    if (_bridgingOffset < 0)
                    {
                        _bridgingOffset = 0;
                    }
                    if (_bridgingOffset > _finalRequestEndTime)
                    {
                        _bridgingOffset = _finalRequestEndTime;
                    }
                    int chunksFromMS = (int)Math.Floor(
                        (_finalRequestEndTime - _bridgingOffset) / chunkTime
                    );
                    _bridgingOffset = (int)Math.Floor(
                        (_lastAudioInput.Count - chunksFromMS) * chunkTime
                    );

                    for (int i = chunksFromMS; i < _lastAudioInput.Count; i++)
                    {
                        await _streamingCall.WriteAsync(new StreamingRecognizeRequest()
                        {
                            AudioContent = _lastAudioInput[i]
                        });
                    }
                }
            }
            _newStream = false;

            // convert 1st channel of audio from floating point to 16 bit packed into a byte array
            // reference: https://github.com/naudio/NAudio/blob/ec5266ca90e33809b2c0ceccd5fdbbf54e819568/Docs/RawSourceWaveStream.md#playing-from-a-byte-array
            for (int i = 0; i < data.Length / channels; i++)
            {
                short sample = (short)(data[i * channels] * NormalizedFloatTo16BitConversionFactor);
                byte[] bytes = BitConverter.GetBytes(sample);
                _buffer[i * 2] = bytes[0];
                _buffer[i * 2 + 1] = bytes[1];
            }

            ByteString chunk = ByteString.CopyFrom(_buffer, 0, _buffer.Length);

            _audioInput.Add(chunk);

            await _streamingCall.WriteAsync(new StreamingRecognizeRequest() { AudioContent = chunk });
        }

        private IEnumerator RequestMicrophoneAuthorizationAndStartListening()
        {
            while (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }

            InitializeMicrophoneAndBeginStream();
        }

        private void InitializeMicrophoneAndBeginStream()
        {
            if (enableDebugLogging)
            {
                Log("Starting...");
            }

            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();
            _audioSource.clip = Microphone.Start(Microphone.devices[0], true, 5, 44100); // 44.1 khZ standard for Quest, devices[0] for default Android mic.

            // wait for microphone to initialize
            float timerStartTime = Time.realtimeSinceStartup;
            bool timedOut = false;
            while (!(Microphone.GetPosition(Microphone.devices[0]) > 0) && !timedOut)
            {
                timedOut = Time.realtimeSinceStartup - timerStartTime >= MicInitializationTimeout;
            }

            if (timedOut)
            {
                Log("Unable to initialize microphone.");
                return;
            }

            _audioSource.loop = true;
            _audioSource.Play();

            StreamingMicRecognizeAsync();
        }

        private async Task HandleTranscriptionResponses()
        {
            while (await _streamingCall.ResponseStream.MoveNext(default))
            {
                if (_streamingCall.ResponseStream.Current.Results.Count <= 0)
                {
                    continue;
                }

                StreamingRecognitionResult result = _streamingCall.ResponseStream.Current.Results[0];
                if (result.Alternatives.Count <= 0)
                {
                    continue;
                }

                _resultEndTime = (int)((result.ResultEndTime.Seconds * 1000) + (result.ResultEndTime.Nanos / 1000000));

                string transcript = result.Alternatives[0].Transcript.Trim();

                if (result.IsFinal)
                {
                    if (enableDebugLogging)
                    {
                        Log("Final: " + transcript);
                    }

                    _isFinalEndTime = _resultEndTime;

                    onFinalResult.Invoke(transcript);
                }
                else
                {
                    if (returnInterimResults)
                    {
                        if (enableDebugLogging)
                        {
                            Log("Interim: " + transcript);
                        }

                        onInterimResult.Invoke(transcript);
                    }
                }
            }
        }

        private async void RestartAfterStreamingLimit()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }
            try
            {
                await Task.Delay(StreamingLimit, _cancellationTokenSource.Token);

                _newStreamOnRestart = true;

                if (enableDebugLogging)
                {
                    Log("Streaming limit reached, restarting...");
                }

                Restart();
            }
            catch (TaskCanceledException) { }
        }

        private async void StreamingMicRecognizeAsync()
        {
            Log("Creating Speech Client");
            SpeechClient speech;

            try
            {
                string path = Application.persistentDataPath + "/temp2.json";
                string jsonText = "{\"type\": \"service_account\",\"project_id\": \"oculusspeech2tex-1581101253115\",\"private_key_id\": \"70b5e6163e9bdaa3054c479e56d657531c1b33a9\",\"private_key\": \"-----BEGIN PRIVATE KEY-----\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQDDvKqWHoz2/IS2\n8xTZ4plhlmp5zgMrcEZdJuS+Txh5jDfQoVPAExOU66/wq9etZXniqi6e7e4j8cMC\nA81nFgV9wKcuiRa3G6GLbufNg7fsJH4St2/4a4TbpabAIzHnPp9COAg06Gn/MIyV\nIVlaApmKtDrw9+WRwbnOvQ/Ua++EllVKIYXvWzqLT2RVmcz+asdj+YaaGZVJSjrW\niWYLOE+iZRPS8mHqJGYZEFnHeXSn2eeo+Pp0kL+PDs8kwkGeZzj0E7z+MG1HkIxA\nlYqpYnZaiGfpi3Xck+7EtlGX2DaNKU6OZEKujmpm938XZlP9TF8hWKODWboi6gPM\n6Ob9uOyvAgMBAAECggEARbDY89YwzoeeIR1vYRyDC/HoOzIUgtTY2LXOX8v9pNk9\nzjSCgzLHmBBKdmBYzE4DFacOLlv8nCOqEP+VlIyMq/d6DuGUiuF1pRV5xvyM8bEU\niklxvntZtHLIakSdsf3j6knU8jGF++wnQbm7/IZMcMw26DD7NehJ5cf+6Nz9TLEx\nUPRacSIKzRBSehCNJ+G3KIxcAh0oSVUzjAJzCyH9LmTyr829jbJc13nIfJPjfNnc\n/TpT0bDrMo7OYG/hTVm530BXfKG/PrPhuWR0A8JkrtJjQhkgprVqyp9tze+rotml\nv5AViAO9eoSdT/avh6ofpW9kAq8Wk7CFyfbqU0fAwQKBgQD9VWhbcSbSEeK/KaAa\nFbRSuZKC4M8JUJX6zFRHfWTlehSXNYKaHo2xFfPL0jGzwKRvf9/OE1fGR84FJkb2\n5k+RbS+vz5AhhoNobHyI4Dm8iQwmH+jGdu7V2x6IUghvS8ucFXxwRzMEVTrnZLeP\nWEh2yxTizZth88X81D6JRo6N8QKBgQDFzBFm/kxTa2m1wYAag/dksFUEDDhUj96n\no0bLEYMD0JDltcQRTtN7LJjyCF2sUDAHioHkYxn7R+OtixWoh9SFC4HftQfdVfua\nemVZulf2bm0o6/iJcFR8rvBcwN6SdOTrqZNEN42K0dcDS9yF86j27vMC7kbPseeE\nKxpbJWcEnwKBgFC5LVat+JPJtTn6tpR5RAt0LHZd0jsCnbfb8iMRltv0iakhGOup\nPrcl6piQ8tt3UC5jTdppmXeKG+DSMJyuCL6rTx2zgxbbuPXGU+x9nogwF1nyowbS\nkQo+Fz2S/jMQsOGcixlv5pbD3dAr7L3VgMmxrocSwyd0OSt2KjMt83DRAoGAG1T3\nkqPRKDXPha4XZZKWvPMgzE5j+ktnGQMW5TimQgCwBQ346etajcq717YQO59ZA7HA\nAS8wh+iHLZPqij86vqe4apE5ZPxcqBwBpUR6ozLfIqh4z4UWJ6SE0kxUdShx+Z6V\nGvgSNvyIGYyCDIB++KBMNDt4zXpn43uRSMcCVUECgYB1R8B6sH1ohrMykwG7CVOf\ndp7fC8AVfY39C8XQhciA2QacVaTxMsWNq3lkywwvjGC3UEOvDAJ2LlBQRV8nb4US\nKlA6P5QCabSfjL/wem7UYiy9knaDjmm+ZyZcAoFST1udK7+a7RBIXzl15VMca0uV\nA6QwgOQLGCN33GQ4AeY5IA==\n-----END PRIVATE KEY-----\n\",\"client_email\": \"starting-account-p2cxzxy8g8iv@oculusspeech2tex-1581101253115.iam.gserviceaccount.com\",\"client_id\": \"110235965105049024840\",\"auth_uri\": \"https://accounts.google.com/o/oauth2/auth\",\"token_uri\": \"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\": \"https://www.googleapis.com/oauth2/v1/certs\",\"client_x509_cert_url\": \"https://www.googleapis.com/robot/v1/metadata/x509/starting-account-p2cxzxy8g8iv%40oculusspeech2tex-1581101253115.iam.gserviceaccount.com\"}";
                //File.WriteAllText(path, jsonText);
                Log("Deserializing JSON text to create GoogleCredential.");

                Google.Apis.Auth.OAuth2.GoogleCredential googleCredential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(jsonText);




                //using (Stream m = new FileStream(Application.persistentDataPath + "/temp.json", FileMode.Open))
                //googleCredential = GoogleCredential.FromStream(m);
                var channel = new Grpc.Core.Channel(SpeechClient.DefaultEndpoint.Host,
                    googleCredential.ToChannelCredentials());

                Log("Creating channel");

                speech = SpeechClient.Create(channel);

                Log("Client Created");

                /*speech = new SpeechClientBuilder
				{
					ChannelCredentials = googleCredential
				};*/

            }
            catch (Exception e)
            {
                Log("Exceptio: " + e.Message + " -- " + Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"));
                return;
            }


            _streamingCall = speech.StreamingRecognize();
            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();

            Log("Writing initial request");

            // Write the initial request with the config.
            await _streamingCall.WriteAsync(new StreamingRecognizeRequest()
            {
                StreamingConfig = new StreamingRecognitionConfig()
                {
                    Config = new RecognitionConfig()
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 44100,//audioConfiguration.sampleRate,
                        LanguageCode = lang,
                        MaxAlternatives = 1
                    },
                    InterimResults = returnInterimResults,
                }
            });

            Log("Request written.");


            _cancellationTokenSource = new CancellationTokenSource();

            Task handleTranscriptionResponses = HandleTranscriptionResponses();

            _listening = true;

            if (!_restart)
            {
                onStartListening.Invoke();
            }

            Log("Ready!");

            if (enableDebugLogging)
            {
                Log("Ready to transcribe!");
            }

            RestartAfterStreamingLimit();

            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, _cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Stop recording and shut down.
                if (enableDebugLogging)
                {
                    Log("Stopping...");
                }

                _listening = false;

                Microphone.End(Microphone.devices[0]);
                _audioSource.Stop();

                await _streamingCall.WriteCompleteAsync();
                try
                {
                    await handleTranscriptionResponses;
                }
                catch (RpcException) { }

                if (!_restart)
                {
                    onStopListening.Invoke();
                }

                if (_restart)
                {
                    _restart = false;
                    if (_newStreamOnRestart)
                    {
                        _newStreamOnRestart = false;

                        _newStream = true;

                        if (_resultEndTime > 0)
                        {
                            _finalRequestEndTime = _isFinalEndTime;
                        }
                        _resultEndTime = 0;

                        _lastAudioInput = null;
                        _lastAudioInput = _audioInput;
                        _audioInput = new List<ByteString>();
                    }
                    StartListening();
                }
            }
        }
    }
}

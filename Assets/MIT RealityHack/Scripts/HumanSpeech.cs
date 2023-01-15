//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OpenAI_Unity;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
#if PLATFORM_ANDROID
#endif
#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif
using CandyCoded.env;

    public class HumanSpeech : MonoBehaviour
    {
        // Speech recognition key, required
        [Tooltip("Connection string to Cognitive Services Speech.")]
        private string speechKey = string.Empty;
        [Tooltip("Region for your Cognitive Services Speech instance (must match the key).")]
        public string speechRegion = "eastus";
        private bool micPermissionGranted = false;
        public EmotionManager emotionManager;
        public float speakRange = 10;
        private Rigidbody rb;
        public EyeInteractable eyeInteractable;
        //public Text outputText;
        //public Button recoButton;

        SpeechRecognizer recognizer;
        SpeechConfig config;
        AudioConfig audioInput;
        PushAudioInputStream pushStream;
        
        UnityEvent m_MyEvent = new UnityEvent();
        UnityEvent newEvent = new UnityEvent();

        private object threadLocker = new object();
        private bool recognitionStarted = false;
        private string message;
        int lastSample = 0;
        AudioSource audioSource;

        private bool initiated = false;
        public bool ready = false;

#if PLATFORM_ANDROID || PLATFORM_IOS
        // Required to manifest microphone permission, cf.
        // https://docs.unity3d.com/Manual/android-manifest.html
        private Microphone mic;
#endif

        private byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
        {
            MemoryStream dataStream = new MemoryStream();
            int x = sizeof(Int16);
            Int16 maxValue = Int16.MaxValue;
            int i = 0;
            while (i < data.Length)
            {
                dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
                ++i;
            }
            byte[] bytes = dataStream.ToArray();
            dataStream.Dispose();
            return bytes;
        }

        private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.Result.Text;
                //Debug.Log("RecognizingHandler: " + message);
            }
        }

        public void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.Result.Text;
                Debug.Log("RecognizedHandler: " + message);

            }
            ready = true;

        }

        private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.ErrorDetails.ToString();
                Debug.Log("CanceledHandler: " + message);
            }
        }

        public async void Transpose()
        {
            if (recognitionStarted)
            {
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);

                if (Microphone.IsRecording(Microphone.devices[0]))
                {
                    Debug.Log("Microphone.End: " + Microphone.devices[0]);
                    Microphone.End(null);
                    lastSample = 0;
                }

                lock (threadLocker)
                {
                    recognitionStarted = false;
                    Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
                }
            }
            else
            {
                if (!Microphone.IsRecording(Microphone.devices[0]))
                {
                    Debug.Log("Microphone.Start: " + Microphone.devices[0]);
                    audioSource.clip = Microphone.Start(Microphone.devices[0], true, 200, 16000);
                    Debug.Log("audioSource.clip channels: " + audioSource.clip.channels);
                    Debug.Log("audioSource.clip frequency: " + audioSource.clip.frequency);
                }

                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
                lock (threadLocker)
                {
                    recognitionStarted = true;
                    Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
                }
            }
        }

        void Start()
        {

            // Set env
            if (env.TryParseEnvironmentVariable("COGNITIVE_ALT_SPEECH_KEY", out string mySpeechKey))
            {
                speechKey = mySpeechKey;
            }


            
                rb = GetComponent<Rigidbody>();
                // Continue with normal initialization, Text and Button objects are present.
#if PLATFORM_ANDROID
                // Request to use the microphone, cf.
                // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
                //message = "Waiting for mic permission";
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    Permission.RequestUserPermission(Permission.Microphone);
                }
#elif PLATFORM_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
#else
            micPermissionGranted = true;
            message = "Click button to recognize speech";
#endif
                config = SpeechConfig.FromSubscription(speechKey, speechRegion);
                
                pushStream = AudioInputStream.CreatePushStream();
                audioInput = AudioConfig.FromStreamInput(pushStream);
                recognizer = new SpeechRecognizer(config, audioInput);
                recognizer.Recognizing += RecognizingHandler;
                recognizer.Recognized += RecognizedHandler;
                recognizer.Canceled += CanceledHandler;

                
                m_MyEvent.AddListener(Transpose);
                newEvent.AddListener(Prompt);
                
                foreach (var device in Microphone.devices)
                {
                    Debug.Log("DeviceName: " + device);                
                }
                audioSource = GameObject.Find("MyAudioSource").GetComponent<AudioSource>();
            
        }
        
        
        public void Prompt()
        {
            if (!string.IsNullOrEmpty(message) && eyeInteractable.isHovered)
            {
                Collider[] surroundingColliders = Physics.OverlapSphere(this.transform.position, this.speakRange);
                foreach (Collider c in surroundingColliders)
                {
                    var ai = c.GetComponent<OAICharacter>();
                    if (ai)
                    {
                        ai.AddToStory(message);
                        //emotionManager.Submit(message);
                    }
                }
            }
        }
        

        void Disable()
        {
            recognizer.Recognizing -= RecognizingHandler;
            recognizer.Recognized -= RecognizedHandler;
            recognizer.Canceled -= CanceledHandler;
            pushStream.Close();
            recognizer.Dispose();
        }

        void FixedUpdate()
        {
            if (initiated == false)
            {
                m_MyEvent.Invoke();
                initiated = true;
                m_MyEvent.RemoveListener(Transpose);
            }
            if (ready)
            {
                Prompt();
                ready = false;
            }
            
#if PLATFORM_ANDROID
            if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                micPermissionGranted = true;
                //message = "Click button to recognize speech";
            }
#elif PLATFORM_IOS
        if (!micPermissionGranted && Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            micPermissionGranted = true;
            message = "Click button to recognize speech";
        }
#endif

            if (Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == true)
            {
                //GameObject.Find("MyButton").GetComponentInChildren<Text>().text = "Stop";
                int pos = Microphone.GetPosition(Microphone.devices[0]);
                int diff = pos - lastSample;

                if (diff > 0)
                {
                    float[] samples = new float[diff * audioSource.clip.channels];
                    audioSource.clip.GetData(samples, lastSample);
                    byte[] ba = ConvertAudioClipDataToInt16ByteArray(samples);
                    if (ba.Length != 0)
                    {
                        //Debug.Log("pushStream.Write pos:" + Microphone.GetPosition(Microphone.devices[0]).ToString() + " length: " + ba.Length.ToString());
                        pushStream.Write(ba);
                    }
                }
                lastSample = pos;
            }
        }
    }

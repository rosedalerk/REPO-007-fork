//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
// <code>
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using CandyCoded.env;

public class AISpeech : MonoBehaviour
{
    public AudioSource audioSource;

    public String voice = "Jenny";
    // Replace with your own subscription key and service region (e.g., "westus").
    // private string SubscriptionKey; //set with env later on
    private string SubscriptionKey;
    private const string Region = "eastus";
    private const int SampleRate = 24000;
    
    
    private object threadLocker = new object();
    //private bool waitingForSpeak;
    private bool audioSourceNeedStop;
    private string message;

    private SpeechConfig speechConfig;
    private SpeechSynthesizer synthesizer;
    
    private static SpeechConfig SpeechConfig;

    public void Speak(string response)
    {
        StartCoroutine(SpeakAsync(response));
    }

    private System.Collections.IEnumerator SpeakAsync(string response)
    {
        var startTime = DateTime.Now;

        // Starts speech synthesis, and returns once the synthesis is started.
        using (var result = synthesizer.StartSpeakingTextAsync(response).Result)
        {
            // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktop).
            // Use the Unity API to play audio here as a short term solution.
            // Native playback support will be added in the future release.
            var audioDataStream = AudioDataStream.FromResult(result);
            var isFirstAudioChunk = true;
            var audioClip = AudioClip.Create(
                "Speech",
                SampleRate * 600, // Can speak 10mins audio as maximum
                1,
                SampleRate,
                true,
                (float[] audioChunk) =>
                {
                    var chunkSize = audioChunk.Length;
                    var audioChunkBytes = new byte[chunkSize * 2];
                    var readBytes = audioDataStream.ReadData(audioChunkBytes);
                    if (isFirstAudioChunk && readBytes > 0)
                    {
                        var endTime = DateTime.Now;
                        var latency = endTime.Subtract(startTime).TotalMilliseconds;
                        //newMessage = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                        isFirstAudioChunk = false;
                    }

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        if (i < readBytes / 2)
                        {
                            audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                        }
                        else
                        {
                            audioChunk[i] = 0.0f;
                        }
                    }

                    if (readBytes == 0)
                    {
                        Thread.Sleep(100); // Leave some time for the audioSource to finish playback
                        audioSourceNeedStop = true;
                    }
                });
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        yield return null;
    }

    
    void Start()
    {
        // Set env
        if (env.TryParseEnvironmentVariable("COGNITIVE_API_KEY", out string apiKey))
        {
            // Debug.Log($"Debug Mode is: {(isDebug ? "ON" : "OFF")}");
            SubscriptionKey = apiKey;
        }
        SpeechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);


        // Creates an instance of a speech config with specified subscription key and service region.
        SpeechConfig.SpeechSynthesisVoiceName = "en-US-" + voice + "Neural";
        //SpeechConfig.Style = "friendly";
        
        //speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);
        
        // The default format is RIFF, which has a riff header.
        // We are playing the audio in memory as audio clip, which doesn't require riff header.
        // So we need to set the format to raw (24KHz for better quality).
        SpeechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

        // Creates a speech synthesizer.
        // Make sure to dispose the synthesizer after use!
        synthesizer = new SpeechSynthesizer(SpeechConfig, null);

        synthesizer.SynthesisCanceled += (s, e) =>
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
            message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
        };
        
    }

    void Update()
    {
        lock (threadLocker)
        {
            if (audioSourceNeedStop)
            {
                audioSource.Stop();
                audioSourceNeedStop = false;
            }
        }
    }

    void OnDestroy()
    {
        if (synthesizer != null)
        {
            synthesizer.Dispose();
        }
    }
}
// </code>

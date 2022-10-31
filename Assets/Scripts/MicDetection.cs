using System;
using System.Collections;
using Microsoft.MixedReality.Toolkit.Audio;
using Photon.Voice.Unity.UtilityScripts;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class MicDetection : MonoBehaviour
{
    private Slider slider;
    private int sampleWindow = 64;
    private AudioClip micClip;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        PrintVar.print(1, $"Microphone: {Microphone.devices[0]}");
        MicrophoneToAudioClip();
    }

    // Update is called once per frame
    private void Update()
    {
        if (micClip == null)
        {
            PrintVar.print(0, $"micClip is null");
            return;
        }
        PrintVar.print(2, $"{Microphone.IsRecording(Microphone.devices[0])}");
        var val = GetLoudnessFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), micClip);
        PrintVar.print(0, $"Mic Level: {val}");
        slider.value = val;
    }
    
    private void MicrophoneToAudioClip()
        => micClip = Microphone.Start(Microphone.devices[0], true, 20, 44100);

    private float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        int startPos = clipPosition - sampleWindow;
        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPos);
        
        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++)
        {
            totalLoudness += Mathf.Abs(waveData[i]);
        }
        
        return totalLoudness / sampleWindow;
    }
}

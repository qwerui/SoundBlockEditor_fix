using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    AudioSource source;
    Text musicInfo;
    double startTime;
    string musicPath;

    public float bpm;

    private void Start() {
        source = GetComponent<AudioSource>();
        musicInfo = GameObject.Find("MusicInfo").GetComponent<Text>();
    }

    //중간에 음악을 시작하기위한 위치 기록
    public void SetStartTime()
    {
        startTime = AudioSettings.dspTime;
    }

    public void SetMusic(AudioClip clip, string clipName, string path)
    {
        source.clip = clip;
        source.clip.name = clipName;
        musicPath = path;
        musicInfo.text = clipName;
    }

    public void PlayMusic(float offset)
    {
        source.time = 0;
        source.PlayDelayed(offset);
    }

    public void PauseMusic()
    {
        source.Stop();
    }

    public bool GetIsPlaying()
    {
        return source.isPlaying;
    }

    public void ResumeMusic(float time)
    {
        if(time<0)
        {
            source.time = 0;
            source.PlayDelayed(-time);
        }
        else
        {
            source.time = time;
            source.Play();
        }
    }

    public AudioClip GetClip()
    {
        return source.clip;
    }
    
    public string GetSourcePath()
    {
        return musicPath;
    }
}

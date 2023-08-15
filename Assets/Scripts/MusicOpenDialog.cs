using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Windows.Forms;
using Ookii.Dialogs;
using UnityEngine.Networking;

public class MusicOpenDialog : MonoBehaviour
{
    VistaOpenFileDialog openDialog;

    public MusicManager music;
    
    void Start()
    {
        //Window Form 설정 mp3, wav 파일 읽기
        openDialog = new VistaOpenFileDialog();
        openDialog.Filter = "mp3 files (*.mp3) |*.mp3| wav files (*.wav) |*.wav| All files(*.*) |*.*";
        openDialog.FilterIndex = 3;
        openDialog.Title = "Load music";
    }
    public void OpenMusic()
    {
        if(openDialog.ShowDialog() == DialogResult.OK)
        {
            if(openDialog.OpenFile() != null)
            {
                StartCoroutine(FileOpen(openDialog.FileName));
            }
        }
        
    }

    public IEnumerator FileOpen(string filePath)
    {
        AudioType audioType = AudioType.UNKNOWN;
        string musicName = Path.GetFileName(filePath);
        if (openDialog.FileName.Contains(".wav"))
        {
            audioType = AudioType.WAV;
        }
        else if (openDialog.FileName.Contains(".mp3"))
        {
            audioType = AudioType.MPEG;
        }
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, audioType))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                //오류
                Debug.Log(request.error);
                yield break;
            }
            DownloadHandlerAudioClip dlHandler = (DownloadHandlerAudioClip)request.downloadHandler;
            if (dlHandler.isDone)
            {
                AudioClip clip = dlHandler.audioClip;
                if (clip != null)
                {
                    music.SetMusic(DownloadHandlerAudioClip.GetContent(request), musicName.Remove(musicName.LastIndexOf('.')), filePath);
                }
            }
        }
    }
    public void LoadSheetMusic(string path)
    {
        StartCoroutine(FileOpen(path));
    }
}

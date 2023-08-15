using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Windows.Forms;
using Ookii.Dialogs;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class SheetManager : MonoBehaviour
{
    VistaOpenFileDialog openDialog;

    string SavePath;
    string MusicPath = "/Music/";
    string SheetPath = "/Sheet/";

    MusicManager music;
    StreamWriter sw;
    InputField title;
    InputField difficulty;
    InputField artist;
    InputField offset;
    InputField bpmText;
    TMP_Dropdown key;

    BPMChanger changer;
    Bar bar;
    MusicOpenDialog musicDialog;
    EditDirector edit;

    int totalNote;
    
    // Start is called before the first frame update
    void Start()
    {
        SavePath = UnityEngine.Application.persistentDataPath.Replace("SoundBlockEditor","SoundBlock");
        openDialog = new VistaOpenFileDialog();
        openDialog.Filter = "sheet files (*.dat) |*.dat|  All files(*.*) |*.*";
        openDialog.FilterIndex = 2;
        openDialog.Title = "Load sheet";
        music = GameObject.FindObjectOfType<MusicManager>();
        title = GameObject.Find("SaveTitle").GetComponent<InputField>();
        difficulty = GameObject.Find("Difficulty").GetComponent<InputField>();
        artist = GameObject.Find("Artist").GetComponent<InputField>();
        changer = GameObject.FindObjectOfType<BPMChanger>();
        bar = GameObject.FindObjectOfType<Bar>();
        offset = GameObject.Find("Offset").GetComponent<InputField>();
        musicDialog = GameObject.FindObjectOfType<MusicOpenDialog>();
        bpmText = GameObject.Find("BPM").GetComponent<InputField>();
        key = GameObject.Find("Key").GetComponent<TMP_Dropdown>();
        edit = GameObject.FindObjectOfType<EditDirector>();
        totalNote = 0;
    }
    public void SaveSheet()
    {
        string sheetTitle;
        if(title.text.Length <= 0)
        {
            sheetTitle = music.GetClip().name;
        }
        else
        {
            sheetTitle = title.text;
        }
        string fullSheetPath = SavePath+SheetPath+sheetTitle;
        string fullMusicPath = SavePath+MusicPath+music.GetClip().name;
        string extension = null;
        if (music.GetSourcePath().Contains(".wav"))
        {
            extension = ".wav";
        }
        else if (music.GetSourcePath().Contains(".mp3"))
        {
            extension = ".mp3";
        }
        try
        {
            sw = new StreamWriter(fullSheetPath+".dat");
            sw.WriteLine("Title: "+sheetTitle);
            sw.WriteLine("Artist: "+artist.text);
            sw.WriteLine("Offset: "+StaticVar.Instance.offset);
            sw.WriteLine("Difficulty: "+difficulty.text);
            sw.WriteLine("Songpath: "+music.GetClip().name+extension);
            sw.WriteLine("BaseBPM: "+StaticVar.Instance.bpm);
            sw.WriteLine("Key: "+StaticVar.Instance.key);
            sw.WriteLine($"Total_Bar: {StaticVar.Instance.barCount}");
            sw.WriteLine("BPM_List");
            BPMParse(sw);
            sw.WriteLine("Notes");
            NoteParse(sw);
            sw.WriteLine($"Total_Note: {totalNote}");
            sw.WriteLine("Score: 0");
            sw.WriteLine("Combo: 0");
            sw.Flush();
            sw.Close();
        }
        catch
        {
            Debug.Log("File save error");
        }
        try
        {
            
            if(extension==null)
            {
                throw new System.Exception("Extension Error!!");
            }
            File.Copy(music.GetSourcePath(),fullMusicPath+extension,true);
            
        }
        catch
        {
            Debug.Log("Music copy error");
        }
    }
    public void LoadSheet()
    {
        edit.ResetSheet();
        StartCoroutine(OpenSheet());
    }
    IEnumerator OpenSheet()
    {
        if(openDialog.ShowDialog() == DialogResult.OK)
        {
            if(openDialog.OpenFile() != null)
            {
                using(UnityWebRequest request = UnityWebRequest.Get($"file://{openDialog.FileName}"))
                {
                    yield return request.SendWebRequest();
                    if(request.result == UnityWebRequest.Result.ConnectionError ||request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.Log(request.error);
                        yield break;
                    }
                    DownloadHandler dlHandler = request.downloadHandler;
                    if(dlHandler.isDone)
                    {
                        Extract(dlHandler.text);
                    }
                }
            }
        }
    }
    void NoteParse(StreamWriter writer)
    {
        int tempoSize = StaticVar.Instance.tempoSize;
        List<BPMInfo> bpmList = changer.BPMInfoList;
        totalNote = 0;

        foreach(Note note in GameObject.FindObjectsOfType<Note>(false))
        {
            float currBpm=StaticVar.Instance.bpm;
            float noteBar = note.rect.anchoredPosition.y/tempoSize/180/(240/StaticVar.Instance.bpm);
            float time = 0;
            int count = 0;
            int beforeBar = 0;

            //변속이 존재할 경우 변속에 맞도록 시간을 계산
            //(변속 시작 마디 - 마지막 변속 마디) * 60 * 4 / 현재 BPM
            while(count < bpmList.Count)
            {
                if(noteBar < bpmList[count].bar)
                {
                    break;
                }
                time+=(bpmList[count].bar-beforeBar)*240/currBpm;
                currBpm = bpmList[count].bpm;
                beforeBar = bpmList[count].bar;
                count++;
            }
            
            time+=(noteBar-beforeBar)*240/currBpm;
            //노트의 라인, Line Enum과 같은 이름을 가져야한다
            string noteline = note.transform.parent.name;

            if(note.noteType==0) //단노트
            {    
                writer.WriteLine($"{noteline},{note.noteType},{note.rect.anchoredPosition.y*2/tempoSize},{time}");
            }
            else //롱노트
            {
                currBpm = StaticVar.Instance.bpm;
                RectTransform tail = note.transform.GetChild(0).GetComponent<RectTransform>();
                noteBar = (note.rect.anchoredPosition.y+tail.anchoredPosition.y)/tempoSize/180/(240/StaticVar.Instance.bpm);
                float tailTime = 0;
                beforeBar = 0;
                count = 0;
                while(count < bpmList.Count)
                {
                    if(noteBar < bpmList[count].bar)
                    {
                        break;
                    }
                    tailTime+=(bpmList[count].bar-beforeBar)*240/currBpm;
                    currBpm = bpmList[count].bpm;
                    beforeBar = bpmList[count].bar;
                    count++;
                }
                tailTime+=(noteBar-beforeBar)*240/currBpm;
                writer.WriteLine($"{noteline},{note.noteType},{note.rect.anchoredPosition.y*2/tempoSize},{time},{tail.anchoredPosition.y*2/tempoSize},{tailTime}");
            }
            totalNote++;
        }
    }
    void BPMParse(StreamWriter writer)
    {
        int beforeBar = 0;
        float currBpm = StaticVar.Instance.bpm;
        float time = 0;
        foreach(BPMInfo info in changer.BPMInfoList)
        {
            time += (info.bar-beforeBar)*240/currBpm;
            writer.WriteLine(info.bpm+","+time+","+info.bar);
            beforeBar=info.bar;
            currBpm = info.bpm;
        }
    }
    void Extract(string sheet)
    {
        StringReader reader = new StringReader(sheet);
        title.text = reader.ReadLine().Replace("Title: ","");
        artist.text = reader.ReadLine().Replace("Artist: ","");
        offset.text = reader.ReadLine().Replace("Offset: ","");
        StaticVar.Instance.offset = float.Parse(offset.text);
        difficulty.text = reader.ReadLine().Replace("Difficulty: ","");
        musicDialog.LoadSheetMusic(SavePath+MusicPath+reader.ReadLine().Replace("Songpath: ",""));
        bpmText.text = reader.ReadLine().Replace("BaseBPM: ","");
        StaticVar.Instance.bpm = float.Parse(bpmText.text);
        switch(EnumUtil.StringToEnum<Key>(reader.ReadLine().Replace("Key: ","")))
        {
            case Key.KEY4:
                key.value = 0;
                break;
            case Key.KEY5:
                key.value = 1;
                break;
            case Key.KEY6:
                key.value = 2;
                break;
            case Key.KEY8:
                key.value = 3;
                break;
        }
        edit.barCount.text = reader.ReadLine().Replace("total_Bar: ","");
        edit.CreateSheet();
        reader.ReadLine();
        while(true)
        {
            string bpm = reader.ReadLine();
            if(bpm.Equals("Notes"))
            {
                break;
            }
            string[] bpmAndTime = bpm.Split(',');
            changer.LoadBPMList(new BPMInfo(float.Parse(bpmAndTime[0]),int.Parse(bpmAndTime[2])));
        }
        while(true)
        {
            string note = reader.ReadLine();
            if(note.Contains("Total_Note: "))
            {
                break;
            }
            string[] noteInfo = note.Split(',');
            bar.LoadNote(noteInfo);
        }
    }
}
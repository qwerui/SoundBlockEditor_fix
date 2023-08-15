using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EditDirector : MonoBehaviour
{
    public RectTransform bar;
    public RectTransform content;
    public InputField barCount;
    BPMChanger changer;
    MusicManager music;
    Bar barScript;
    
    Scrollbar scroll;


    Coroutine sheetScroll;

    float wheeltick;
    int tempoSize;

    int totalBar;
    float offset = 0;

    private void Start() {
        scroll = GameObject.Find("MusicScrollBar").GetComponent<Scrollbar>();
        changer = GameObject.FindObjectOfType<BPMChanger>();
        music = GameObject.FindObjectOfType<MusicManager>();
        barScript = bar.GetComponent<Bar>();
        totalBar = 0;
        tempoSize = 2;
    }

    public void CreateSheet()
    {
        if(StaticVar.Instance.bpm <= 0)
            return;
        totalBar = int.Parse(barCount.text);
        bar.sizeDelta = new Vector2(800, totalBar * 180 * tempoSize * (240/StaticVar.Instance.bpm));
        barScript.SetBeatBar(tempoSize, totalBar);
        StaticVar.Instance.barCount = totalBar;
        if(totalBar > 2)
            wheeltick = 120/(bar.sizeDelta.y-720);
        else
            wheeltick =0.25f;
    }
    
    public void ChangeSize(float value)
    {
        bar.sizeDelta = new Vector2(800, bar.sizeDelta.y/tempoSize*value);
        content.anchoredPosition = (Vector2.down * bar.sizeDelta.y + Vector2.up * 720) * scroll.value;
        barScript.SetBarSize(value, StaticVar.Instance.tempoSize);
        if(totalBar > 2)
            wheeltick = 120/(bar.sizeDelta.y-720);
        else
            wheeltick =0.25f;
        tempoSize = (int)value;
        StaticVar.Instance.tempoSize = tempoSize;
    }

    public void ResetSheet()
    {
        sheetScroll = null;
        bar.sizeDelta = new Vector2(800, 180*tempoSize);
        barScript.DeleteAll();
        StaticVar.Instance.barCount = 0;
    }

    public void Scroll(float value)
    {
        content.anchoredPosition = value * (Vector2.down * bar.sizeDelta.y+Vector2.up*720);
    }

    private void Update() {
        if(StaticVar.Instance.mode != EditMode.Play)
        {
            scroll.value += Input.mouseScrollDelta.y * wheeltick;
            if(scroll.value<0)
                scroll.value=0;
            if(scroll.value>1)
                scroll.value=1;
        }
        
    }

    public void MusicStart()
    {
        if(StaticVar.Instance.bpm <= 0)
            return;
        
        if(sheetScroll != null)
        {
            StopCoroutine(sheetScroll);
        }
        if(music.GetIsPlaying())
        {
            music.PauseMusic();
        }
        music.PlayMusic(offset);
        StaticVar.Instance.mode = EditMode.Play;
        content.anchoredPosition = Vector2.zero;
        scroll.gameObject.SetActive(false);
        sheetScroll = StartCoroutine(ScrollSheet());
    }

    IEnumerator ScrollSheet()
    {
        int endPoint = -(int)bar.sizeDelta.y + 720;
        float currBpm = StaticVar.Instance.bpm;
        float baseBpm = StaticVar.Instance.bpm;
        music.SetStartTime();
        Queue<BPMInfo> BPMList = new Queue<BPMInfo>(changer.BPMInfoList);
        while(content.anchoredPosition.y > endPoint)
        {
            if(BPMList.Count>0) //변속
            {
                if(content.anchoredPosition.y <= BPMList.Peek().bar*180*tempoSize*-1*240/baseBpm)
                {
                    currBpm = BPMList.Dequeue().bpm;
                }
            }
            content.anchoredPosition+=Vector2.down*180*tempoSize*Time.deltaTime * currBpm / baseBpm;
            yield return null;
        }
    }
    public void PauseMusic()
    {
        if(sheetScroll==null)
            return;
        scroll.gameObject.SetActive(true);
        music.PauseMusic();
        StaticVar.Instance.mode = EditMode.Idle;
        scroll.value = content.anchoredPosition.y / -bar.sizeDelta.y;
        StopCoroutine(sheetScroll);
    }
    public void ResumeMusic()
    {
        if(StaticVar.Instance.bpm <= 0)
            return;
        if(sheetScroll != null)
        {
            StopCoroutine(sheetScroll);
        }
        if(music.GetIsPlaying())
        {
            music.PauseMusic();
        }
        music.ResumeMusic(GetCurrentMusicTime());
        StaticVar.Instance.mode = EditMode.Play;
        scroll.gameObject.SetActive(false);
        sheetScroll = StartCoroutine(ScrollSheet());
    }
    public void SetBPM(string _bpm)
    {
        if(_bpm.Length==0)
            return;
        StaticVar.Instance.bpm = float.Parse(_bpm);
    }
    public void SetKey(int index)
    {
        StaticVar.Instance.key = (Key)index;
        barScript.ShowLine();
    }
    public void ChangeAddMode()
    {
        StaticVar.Instance.mode = EditMode.AddMode;
    }
    public void ChangeFixMode()
    {
        StaticVar.Instance.mode = EditMode.FixMode;
    }
    public void ChangeOffset(string value)
    {
        if(value.Length <= 0)
            return;
        offset = float.Parse(value);
        StaticVar.Instance.offset = offset;
    }
    float GetCurrentMusicTime()
    {
        if(changer.BPMInfoList.Count <= 0)
        {
            //스크롤의 위치를 기반으로 음악 위치 계산 (위치/스크롤 크기 / 180 - 오프셋)
            return (-content.anchoredPosition.y / tempoSize / 180) - offset;
        }
        else //변속이 존재할 경우 변속을 고려한 위치 계산
        {
            float currBpm = StaticVar.Instance.bpm;
            int currBar = (int)Mathf.Floor(-content.anchoredPosition.y / tempoSize / 180 / (240/StaticVar.Instance.bpm));
            float currBarY = -content.anchoredPosition.y/tempoSize/180/240*StaticVar.Instance.bpm - currBar;
            float time = 0;
            int beforeBar = 0;

            foreach(BPMInfo bpmInfo in changer.BPMInfoList)
            {
                if(currBar < bpmInfo.bar)
                {
                    time += (currBar-beforeBar)*240/currBpm;
                    beforeBar = currBar;
                    break;
                }
                time += ((bpmInfo.bar-beforeBar) * 240 / currBpm);
                currBpm = bpmInfo.bpm;
                beforeBar = bpmInfo.bar;
            }
            time += ((currBarY+currBar-beforeBar) * 240 / currBpm);
            return time-offset;
        }
    }
}

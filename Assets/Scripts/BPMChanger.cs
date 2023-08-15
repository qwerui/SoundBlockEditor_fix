using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BPMChanger : MonoBehaviour
{
    public Transform BPM_List;
    public InputField inputBPM;
    public InputField inputBar;
    public GameObject BPMChangeObj;
    public List<BPMInfo> BPMInfoList;

    private void Start() {
        BPMInfoList = new List<BPMInfo>();
    }
    
    public void ChangeBPM()
    {
        if(inputBar.text == null)
            return;
        if(inputBPM.text == null)
            return;

        BPMInfo tempInfo = new BPMInfo(float.Parse(inputBPM.text), int.Parse(inputBar.text));
        BPMInfoList.Add(tempInfo);
        Instantiate(BPMChangeObj, BPM_List).GetComponent<BPMChangeUI>().SetInfo(tempInfo, this);
    }
    public void RemoveFromList(BPMInfo info)
    {
        BPMInfoList.Remove(info);
    }
    public void LoadBPMList(BPMInfo info)
    {
        BPMInfoList.Add(info);
        Instantiate(BPMChangeObj, BPM_List).GetComponent<BPMChangeUI>().SetInfo(info, this);
    }
}
public struct BPMInfo
{
    public float bpm;
    public int bar;
    public BPMInfo(float _bpm, int _bar)
    {
        bpm = _bpm;
        bar = _bar;
    }
}

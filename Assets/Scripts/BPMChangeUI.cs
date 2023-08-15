using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BPMChangeUI : MonoBehaviour
{
    BPMInfo info;
    BPMChanger changer;
    public Text bpmText;
    public Text timeText;

    public void SetInfo(BPMInfo _info, BPMChanger _changer)
    {
       info = _info;
       changer = _changer;
       bpmText.text = info.bpm.ToString();
       timeText.text = $"{info.bar}";
    }
    public void DeleteInfo()
    {
        changer.RemoveFromList(info);
        Destroy(this.gameObject);
    }
}

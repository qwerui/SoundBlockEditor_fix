using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticVar : MonoBehaviour
{
    private static StaticVar instance;
    public static StaticVar Instance
    {
        get
        {
            return instance;
        }
    }
    private void Awake() {
        instance = this;
        key = Key.KEY4;
        tempoSize = 2;
        mode = EditMode.Idle;
        offset = 0;
    }
    public float bpm;
    public Key key;
    public int tempoSize;
    public int barCount;
    public EditMode mode;
    public float offset;
}
public enum EditMode
{
    AddMode,
    FixMode,
    Play,
    Idle
}

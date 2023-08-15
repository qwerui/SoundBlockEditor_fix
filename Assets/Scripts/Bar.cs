using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Bar : MonoBehaviour
{
    public RectTransform rect;
    public GameObject gridLine;
    public GameObject beatLine;
    public Transform beatPosition;
    public Transform gridPosition;
    public GameObject note;

    private void Start() {
        ShowLine();
    }

    public void SetBeatBar(int tempoSize, int barCount)
    {
        foreach(Transform beat in beatPosition)
        {
            Destroy(beat.gameObject);
        }
        for(int i=0;i<barCount;i++)
        {
            GameObject beat = Instantiate(beatLine, beatPosition);
            beat.GetComponent<RectTransform>().anchoredPosition = Vector2.up * 180 * i * tempoSize * (240/StaticVar.Instance.bpm);
            beat.transform.GetChild(0).GetComponent<Text>().text = i.ToString();
        }
    }
    public void SetBarSize(float afterSize, float beforeSize)
    {
        foreach(RectTransform beatRect in beatPosition)
        {
            beatRect.anchoredPosition = beatRect.anchoredPosition * afterSize / beforeSize;
        }
        foreach(RectTransform gridRect in gridPosition)
        {
            gridRect.anchoredPosition = gridRect.anchoredPosition * afterSize / beforeSize;
        }
        foreach(Note noteObj in GetComponentsInChildren<Note>())
        {
            noteObj.SetPosition(afterSize, beforeSize);
        }
    }

    public void DrawGrid(float cut)
    {
        DeleteGrid();
        float oneBlock = 180 * StaticVar.Instance.tempoSize * (240/StaticVar.Instance.bpm) / (float)cut;
        for(int i=0;i<cut*StaticVar.Instance.barCount;i++)
        {
            RectTransform gridPos = Instantiate(gridLine, gridPosition).GetComponent<RectTransform>();
            gridPos.anchoredPosition = Vector2.up * oneBlock * (float)i;
        }
    }
    public void DeleteGrid()
    {
        foreach(Transform t in gridPosition)
        {
            Destroy(t.gameObject);
        }
    }
    public void ClickEvent(BaseEventData baseEvent)
    {
        PointerEventData pointEvent = baseEvent as PointerEventData;
        GameObject tempobj = pointEvent.pointerPressRaycast.gameObject;
        if(StaticVar.Instance.mode == EditMode.AddMode)
        {
            if(pointEvent.button == PointerEventData.InputButton.Left) //좌클릭 시 노트 추가
            {
                if(tempobj.CompareTag("Line"))
                {
                    Vector2 mousePos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,Input.mousePosition,Camera.main,out mousePos);
                    Transform lineTransform = pointEvent.pointerPressRaycast.gameObject.transform;

                    if(gridPosition.childCount>0) //그리드 스냅
                    {
                        float oneGrid = rect.sizeDelta.y / (float)gridPosition.childCount;
                        mousePos = Vector2.up * Mathf.Floor(mousePos.y / oneGrid) * oneGrid;

                        //해당 위치에 노트가 이미 존재할 경우 추가하지 않음
                        foreach(RectTransform notePos in lineTransform)
                        {
                            if(notePos.anchoredPosition.y == mousePos.y)
                            {
                                return;
                            }
                        }
                    }
                    Instantiate(note, lineTransform).GetComponent<RectTransform>().anchoredPosition = Vector2.up * mousePos;
                }
            }
            else //우클릭 시 삭제
            {
                if(tempobj.CompareTag("Note"))
                {
                    Destroy(tempobj);
                }
                else if(tempobj.CompareTag("Notetail"))
                {
                    Destroy(tempobj.transform.parent.gameObject);
                }
            }
        }
        else if(StaticVar.Instance.mode == EditMode.FixMode) //롱노트->단노트 변경
        {
            if(pointEvent.button == PointerEventData.InputButton.Right)
            {
                if(tempobj.CompareTag("Notetail")||tempobj.CompareTag("Note"))
                {
                    tempobj.GetComponentInParent<Note>().DeleteLongNote();
                }
            }
        }
    }
    public void ShortToLongNote(BaseEventData baseEvent)
    {
        if(StaticVar.Instance.mode == EditMode.FixMode)
        {
            PointerEventData pointEvent = baseEvent as PointerEventData;
            GameObject tempObj = pointEvent.pointerPressRaycast.gameObject;
            Vector2 mousePos;
            if(tempObj.CompareTag("Notetail"))
            {
                //연장한 부분 클릭시 기준이 되는 단노트로 변경
                tempObj = tempObj.transform.parent.gameObject;
            }
            if(tempObj.CompareTag("Note"))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(tempObj.GetComponent<RectTransform>(),Input.mousePosition,Camera.main,out mousePos);
                if(pointEvent.dragging)
                {
                    if(gridPosition.childCount>0) //그리드 생성시
                    {
                        float oneGrid = rect.sizeDelta.y / (float)gridPosition.childCount;
                        mousePos = Vector2.up * Mathf.Floor(mousePos.y / oneGrid) * oneGrid;    
                    }
                        tempObj.GetComponent<Note>().CreateLongNote(mousePos.y);
                }
            }
        }
    }
    public void ShowLine()
    {
        for(int i=0;i<8;i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        switch(StaticVar.Instance.key)
        {
            case Key.KEY4:
            for(int i=1;i<=4;i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            break;
            case Key.KEY5:
            for(int i=1;i<=5;i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            break;
            case Key.KEY6:
            for(int i=1;i<=6;i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            break;
            case Key.KEY8:
            for(int i=0;i<=7;i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            break;
        }
    }
    public void DeleteAll()
    {
        foreach(Transform beat in beatPosition)
        {
            Destroy(beat.gameObject);
        }
        foreach(Transform grid in gridPosition)
        {
            Destroy(grid.gameObject);
        }
        foreach(Note note in GetComponentsInChildren<Note>())
        {
            Destroy(note.gameObject);
        }
    }
    public void LoadNote(string[] noteInfo)
    {
        Transform line = transform.GetChild((int)EnumUtil.StringToEnum<Line>(noteInfo[0]));
        if(noteInfo[1].Equals("0"))//Short Note
        {
            Instantiate(note, line).GetComponent<RectTransform>().anchoredPosition = Vector2.up * float.Parse(noteInfo[2]) * StaticVar.Instance.tempoSize / 2;
        }
        else //Long Note
        {
            GameObject tempLongNote = Instantiate(note, line);
            tempLongNote.GetComponent<RectTransform>().anchoredPosition = Vector2.up * float.Parse(noteInfo[2]) * StaticVar.Instance.tempoSize;
            tempLongNote.GetComponent<Note>().CreateLongNote(float.Parse(noteInfo[4]));
        }
    }
}

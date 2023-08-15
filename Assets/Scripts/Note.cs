using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    public RectTransform rect;
    public int noteType = 0; //0 = short, 1 = long
    public GameObject tailPrefab;
    public Image img;
    RectTransform tail;

    private void Start() 
    {
        RectTransform lineRect = transform.parent.GetComponent<RectTransform>();
    }

    public void SetPosition(float value, float beforeValue)
    {
        rect.anchoredPosition = rect.anchoredPosition / beforeValue * value;
        if(noteType==1)
        {
            tail.anchoredPosition = tail.anchoredPosition / beforeValue * value;
            tail.sizeDelta = new Vector2(100, tail.sizeDelta.y /beforeValue * value);
        }
    }
    public void DeleteLongNote()
    {
        noteType = 0;
        tail = null;
        if(transform.childCount!=0)
        {
            Destroy(transform.GetChild(0).gameObject);
            img.color = new Color(1,1,0,1);
        }
    }
    public void CreateLongNote(float y)
    {
        if(noteType==0)
        {
            tail = Instantiate(tailPrefab, transform).GetComponent<RectTransform>();
            img.color = new Color(1,0.5f,0,1);
        }
        noteType = 1;
        if(y>=0)
        {
            tail.anchoredPosition = new Vector2(0, y);
            tail.sizeDelta = new Vector2(0, y);
        }
    }
}

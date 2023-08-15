using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public Bar bar;
    public void SetGrid(int index)
    {
        switch(index)
        {
            case 0:
                DrawGridAll(4);
                break;
            case 1:
                DrawGridAll(8);
                break;
            case 2:
                DrawGridAll(16);
                break;
            case 3:
                DrawGridAll(32);
                break;
            case 4:
                DrawGridAll(3);
                break;
            case 5:
                DrawGridAll(6);
                break;
            case 6:
                DrawGridAll(12);
                break;
            case 7:
                DrawGridAll(24);
                break;
            case 8:
                DeleteGridAll();
                break;
        }
    }
    void DrawGridAll(float cut)
    {
        bar.DrawGrid(cut);
    }
    void DeleteGridAll()
    {
        bar.DeleteGrid();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumUtil
{
    public static T StringToEnum<T>(string input)
    {
        return (T)System.Enum.Parse(typeof(T), input);
    }
}

public enum Key
{
    KEY4 = 0,
    KEY5 = 1,
    KEY6 = 2,
    KEY8 = 3
}

public enum Line
{
    LTrig = 0,
    Line1 = 1,
    Line2 = 2,
    Line3 = 3,
    Line4 = 4, 
    Line5 = 5,
    Line6 = 6,
    RTrig = 7
}
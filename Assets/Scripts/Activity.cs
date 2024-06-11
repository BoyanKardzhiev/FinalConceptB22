using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class Activity : ScriptableObject
{
    public Sprite image;
    public string text;
    public string description;
    public float startHour;
    public float activityDuration;
}

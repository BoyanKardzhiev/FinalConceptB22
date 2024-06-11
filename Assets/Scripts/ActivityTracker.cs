using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActivityTracker : MonoBehaviour
{
    public List<Activity> activities1 = new List<Activity>();
    public List<Activity> activities2 = new List<Activity>();
    public List<Activity> activities3 = new List<Activity>();
    public List<Activity> activities4 = new List<Activity>();
    public List<Activity> activities5 = new List<Activity>();
    public List<Activity> activities6 = new List<Activity>();
    public List<Activity> activities7 = new List<Activity>();
    public List<Activity> activities8 = new List<Activity>();
    public List<Activity> currentActivities = new List<Activity>();
    public Activity currentActivity { get; private set; }

    [SerializeField] Image activityPicture;
    [SerializeField] TextMeshProUGUI activityType;
    [SerializeField] Clock clockScript;
    [SerializeField] GameObject finalConcept, StandbyClock;

    float currentHours, currentTimer;

    void Start()
    {
        currentActivities = activities1;
    }
    // Update is called once per frame
    void Update()
    {
        if(currentTimer > 0)
        {
            finalConcept.SetActive(true);
            StandbyClock.SetActive(false);
            currentTimer -= Time.deltaTime * clockScript.timeMultiplier;
        }
        else
        {
            currentTimer = 0;
            finalConcept.SetActive(false);
            StandbyClock.SetActive(true);
        }
        currentHours = clockScript.hours;
        for(int i=0; i<currentActivities.Count; i++)
        {
            if (currentActivities[i]!=null && i==currentHours)
            {
                activityPicture.sprite = currentActivities[i].image;
                activityType.text = currentActivities[i].text;
                currentActivity = currentActivities[i];
                currentTimer = currentActivities[i].activityDuration*60-((currentHours-currentActivities[i].startHour)*60 + clockScript.minutes)*60; 
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentActivities = activities1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentActivities = activities2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentActivities = activities3;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentActivities = activities4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentActivities = activities5;
        if (Input.GetKeyDown(KeyCode.Alpha6)) currentActivities = activities6;
        if (Input.GetKeyDown(KeyCode.Alpha7)) currentActivities = activities7;
        if (Input.GetKeyDown(KeyCode.Alpha8)) currentActivities = activities8;
    }
}

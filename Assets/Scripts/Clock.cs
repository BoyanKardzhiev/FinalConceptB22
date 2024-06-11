using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Windows.Speech;
using UnityEngine.Networking;
using Amazon.Polly;
using Amazon.Runtime;
using Amazon;
using Amazon.Polly.Model;
using System.IO;
using System.Threading.Tasks;

public class Clock : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;



    public const int hoursDay = 24, minutesHour = 60, secondsMinute = 60;
    float interval = 0.00416667f, intervalColorR, intervalColorG, intervalColorB, intervalHours = 0.00833333f;
    int intervalMinSec = 6;
    public float totalTime, changeTime;
    public float hours { get; private set; }
    float secondsPast;
    public int minutes, currentMinSegment;
    public float timeMultiplier;

    int clockMode;
    bool standByMode;
    [SerializeField] GameObject HourHand, MinuteHand, SecondHand;

    [SerializeField] TextMeshProUGUI clockDisplay;
    [SerializeField] RectTransform Center;
    [SerializeField] GameObject Face, Background, Halfground, Activity, FinalConcept, StandByMode;
    [SerializeField] ActivityTracker activityTrackerScript;
    Image activityImage;
    TextMeshProUGUI activityText,activityClock;


    private Dictionary<string, Action> speechActions = new Dictionary<string, Action>();
    private KeywordRecognizer keywordRecognizer;

    public float darkR = 0.062745f, darkG = 0.10196f, darkB = 0.21569f;
    public float lightR = 1f, lightG = 0.84706f, lightB = 0.55686f;
    public Color Light = new Color(1f, 0.84706f, 0.55686f, 1);
    public Color Dark = new Color(0.062745f, 0.10196f, 0.21569f, 1);

    public AudioLoudnessDetection detector;
    public float loudnessSensibility = 100f;
    public float threshold = 1f;

    // Start is called before the first frame update
    void Start()
    {
        clockMode = 1;
        standByMode = true;

        /*totalTime = 0;
        changeTime = 0;
        hours = 0;
        minutes = 0;*/

        hours = System.DateTime.Now.Hour;
        minutes = System.DateTime.Now.Minute;
        secondsPast = minutes*60 + System.DateTime.Now.Second;
        totalTime = hours * 60 * 60 + secondsPast;
        currentMinSegment = 5 * Mathf.FloorToInt(minutes / 5);

        Debug.Log(hours);
        Debug.Log(minutes);
        Debug.Log(secondsPast);
        Debug.Log(totalTime);

        intervalColorR = (Light.r - Dark.r)/7200;
        intervalColorG = (Light.g - Dark.g)/7200;
        intervalColorB = (Light.b - Dark.b)/7200;

        Face.SetActive(false);

        speechActions.Add("hello", TurnOn);
        speechActions.Add("what is the time", TellTime);
        speechActions.Add("what is the activity", TellActivity);
        speechActions.Add("get away", TurnOff);

        keywordRecognizer = new KeywordRecognizer(speechActions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;


        activityImage = Activity.transform.GetChild(0).GetComponent<Image>();
        activityClock = Activity.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        activityText = Activity.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        FinalConcept.SetActive(false);
        StandByMode.SetActive(true);
    }

    private void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Keyword: " + args.text);
        speechActions[args.text].Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateStandByClock();
        UpdateClock();
        MoveSunMoon();

        currentMinSegment = 5 * Mathf.FloorToInt(minutes / 5);
        totalTime += Time.deltaTime * timeMultiplier;
        secondsPast += Time.deltaTime * timeMultiplier;
        minutes = Mathf.FloorToInt(secondsPast / secondsMinute);
        if (minutes == minutesHour)
        {
            hours++;
            if (hours == hoursDay)
            {
                hours = 0;
            }
            minutes = 0;
            secondsPast = 0;
        }

        if (Input.GetKeyDown(KeyCode.C)) ChangeClockMode();
        //Debug.Log(clockMode);

        if (Input.GetKeyDown(KeyCode.S))
        {
            if(!standByMode)
            {
                ChangeToStandByMode();
            }
            else
            {
                standByMode = false;
                StandByMode.SetActive(false);
                FinalConcept.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.N)) BackToNormalTime();

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            timeMultiplier = Mathf.Clamp(timeMultiplier - 10, 1, 3000);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            timeMultiplier = Mathf.Clamp(timeMultiplier + 10, 1, 3000);
        }
        
        if (hours >= 6 && hours < 8) ChangeToDay();
        else if (hours >= 18 && hours < 20) ChangeToNight();
        else if (hours >=8 && hours <=17)
        {
            changeTime = 0;
            Background.GetComponent<Image>().color = Light;
            Halfground.GetComponent<Image>().color = Light;

            activityImage.color = Color.black;
            activityClock.color = Color.black;
            activityText.color = Color.black;

        }
        else
        {
            changeTime = 0;
            Background.GetComponent<Image>().color = Dark;
            Halfground.GetComponent<Image>().color = Dark;

            activityImage.color = Color.white;
            activityClock.color = Color.white;
            activityText.color = Color.white;
        }

        //Audio Detection Part
        float loudness = detector.GetLoudnessFromMicrophone()*loudnessSensibility;

        //if(loudness > threshold)
        //{
        //    keywordRecognizer.Start();
        //}
        //else keywordRecognizer.Stop();
        //Debug.Log(loudness);
        //Debug.Log(System.DateTime.Now.Second);
    }

    private void UpdateClock()
    {
        switch(clockMode) 
        {
            case 1:
                clockDisplay.text = Mathf.FloorToInt(hours).ToString("00") + ":" + Mathf.FloorToInt(minutes).ToString("00");
                break;
            case 2:
                clockDisplay.text = Mathf.FloorToInt(hours).ToString("00") + ":" + Mathf.FloorToInt(minutes).ToString("00") + "AM";
                if (hours > 12) clockDisplay.text = (Mathf.FloorToInt(hours)-12).ToString("00") + ":" + Mathf.FloorToInt(minutes).ToString("00") + "PM";
                break;
            case 3:
                float clockHours;
                if (hours>12)
                {
                    Debug.Log("I am here");
                    clockHours = hours - 12;
                    if (minutes >= 5 && minutes <= 34)
                    {
                        clockDisplay.text = "It's " + Service.ConvertToWords(currentMinSegment) + " past " + Mathf.FloorToInt(clockHours).ToString("0") + ".";
                    }
                    else if (minutes >34 && minutes<=59)
                    {
                        clockDisplay.text = "It's " + Service.ConvertToWords(60- currentMinSegment) + " to " + Mathf.FloorToInt(clockHours+1).ToString("0") + ".";
                    }
                    else clockDisplay.text = "It's " + Mathf.FloorToInt(clockHours).ToString("0") + " o'clock.";
                }
                else
                {
                    clockHours = hours - 12;
                    if (minutes >= 5 && minutes <= 34)
                    {
                        clockDisplay.text = "It's " + Service.ConvertToWords(currentMinSegment) + " past " + Mathf.FloorToInt(hours).ToString("0") + ".";
                    }
                    else if (minutes > 34 && minutes <= 59)
                    {
                        if(hours+1 > 12)
                        {
                            clockDisplay.text = "It's " + Service.ConvertToWords(60 - currentMinSegment) + " to " + Mathf.FloorToInt(clockHours + 1).ToString("0") + ".";
                        }
                        else clockDisplay.text = "It's " + Service.ConvertToWords(60 - currentMinSegment) + " to " + Mathf.FloorToInt(hours+1).ToString("0") + ".";
                    }
                    else clockDisplay.text = "It's " + Mathf.FloorToInt(hours).ToString("0") + " o'clock.";
                }
                break;
            case 4:
                clockDisplay.text = null;
                break;

        }
    }

    private void UpdateStandByClock()
    {
        HourHand.transform.rotation = Quaternion.Euler(0, 0, -intervalHours * totalTime);
        MinuteHand.transform.rotation = Quaternion.Euler(0, 0, -intervalMinSec * minutes);
        SecondHand.transform.rotation = Quaternion.Euler(0, 0, -intervalMinSec * Mathf.FloorToInt(secondsPast%60));
    }

    private void ChangeClockMode()
    {
        clockMode++;
        if (clockMode > 4) clockMode = 1;
    }

    private void ChangeToStandByMode()
    {
        FinalConcept.SetActive(false);
        StandByMode.SetActive(true);
        standByMode = true;
    }

    private void MoveSunMoon()
    {
        Center.transform.rotation = Quaternion.Euler(0, 0, -interval * totalTime);
    }



    private void ChangeToDay()
    {
        changeTime += Time.deltaTime * timeMultiplier;
        Background.GetComponent<Image>().color = new Color(intervalColorR*changeTime+Dark.r, intervalColorG * changeTime + Dark.g, intervalColorB * changeTime + Dark.b);
        Halfground.GetComponent<Image>().color = new Color(intervalColorR * changeTime + Dark.r, intervalColorG * changeTime + Dark.g, intervalColorB * changeTime + Dark.b);

        if(changeTime > 3600)
        {
            activityImage.color = Color.black;
            activityClock.color = Color.black;
            activityText.color = Color.black;
        }

    }
    private void ChangeToNight()
    {
        changeTime += Time.deltaTime * timeMultiplier;
        Background.GetComponent<Image>().color = new Color(Light.r - intervalColorR * changeTime, Light.g - intervalColorG * changeTime, Light.b - intervalColorB * changeTime);
        Halfground.GetComponent<Image>().color = new Color(Light.r - intervalColorR * changeTime, Light.g - intervalColorG * changeTime, Light.b - intervalColorB * changeTime);

        if (changeTime > 3600)
        {
            activityImage.color = Color.white;
            activityClock.color = Color.white;
            activityText.color = Color.white;
        }
    }


    private async void TurnOn()
    {
        Face.SetActive(true);

        var credentials = new BasicAWSCredentials("AKIAZQ3DO3Y7PW4BSP6G", "nvqs / sS1l4n4jvwfS3ACbTjZkS + coggxIanITvx1");
        var client = new AmazonPollyClient(credentials, RegionEndpoint.EUCentral2);

        var request = new SynthesizeSpeechRequest()
        {
            Text = "Hello! How are you?",
            Engine = Engine.Neural,
            VoiceId = VoiceId.Amy,
            OutputFormat = OutputFormat.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(request);

        WriteIntoFile(response.AudioStream);

        using (var www = UnityWebRequestMultimedia.GetAudioClip($"{Application.persistentDataPath}/audio.mp3", AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();

            var clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    private void TurnOff()
    {
        Face.SetActive(false);
    }

    private void TellTime()
    {
        Debug.Log("It's" + Mathf.FloorToInt(hours).ToString("00") + ":" + Mathf.FloorToInt(minutes).ToString("00"));
    }
    private void TellActivity()
    {
        Debug.Log(activityTrackerScript.currentActivity.description);
    }

    private void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/audio.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }

    private void BackToNormalTime()
    {
        timeMultiplier = 1;

        hours = System.DateTime.Now.Hour;
        minutes = System.DateTime.Now.Minute;
        secondsPast = minutes * 60 + System.DateTime.Now.Second;
        totalTime = hours * 60 * 60 + secondsPast;
    }

}

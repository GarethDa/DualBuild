using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class MetricsPlugin : MonoBehaviour
{
    [DllImport("MetricsPlugin")]
    private static extern void SaveToFile(string displayText, string displayData);

    [DllImport("MetricsPlugin")]
    private static extern void StartWriting(string fileName);

    [DllImport("MetricsPlugin")]
    private static extern void EndWriting();

    //PlayerAction inputAction;

    string m_Path;
    string fn;

    int timesJumped = 0;

    RoundManager roundInstance;

    float currentRoundTime;
    List<float> roundTimes = new List<float>();
    float averageTime = 0f;

    float currentTimer = 0f;
    float displayTime = 2f;

    // Start is called before the first frame update
    void Start()
    {
        roundInstance = RoundManager.instance;

        EventManager.onRoundEnd += OnRoundEnd;

        m_Path = Application.dataPath;
        fn = m_Path + "/metrics.txt";
        Debug.Log(fn);
    }

    public void OnRoundEnd(object sender, System.EventArgs eArg)
    {
        roundTimes.Add(roundInstance.currentRoundSecondsElapsed);
    }

    public void OnJump(InputAction.CallbackContext cntxt)
    {
        if (cntxt.performed) timesJumped++;
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer += Time.deltaTime;

        if (currentTimer >= displayTime)
        {
            currentTimer = 0f;
            currentRoundTime = roundInstance.currentRoundSecondsElapsed;

            StartWriting(fn);

            SaveToFile("Player has jumped this many times", timesJumped.ToString() + '\n');
            SaveToFile("Current round duration", currentRoundTime.ToString() + '\n');

            SaveToFile("Previous round durations", "\n");

            averageTime = 0f;

            for (int i = 0; i < roundTimes.Count; i++)
            {
                SaveToFile("Round " + (i + 1).ToString(), roundTimes[i].ToString() + '\n');

                averageTime += roundTimes[i];
            }

            if (roundTimes.Count > 0) averageTime = averageTime / roundTimes.Count;

            SaveToFile("Average round time", averageTime.ToString() + '\n');

            EndWriting();
        }
    }
}

//Round times
//Number of jumps
//# of rounds

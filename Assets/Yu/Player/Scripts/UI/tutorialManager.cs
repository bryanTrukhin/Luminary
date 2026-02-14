using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public TMP_Text messageUI;
    
    public string[] taskMessages; 
    public bool[] taskCompleted;
    private int currentTaskNumber = 0;
    public bool enableTesting = true;

    void Start()
    {
        updateDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)) 
        {
            SetTaskStatus(0, true);
        }

        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            SetTaskStatus(1, true);
        }
    }

    public void SetTaskStatus(int taskID, bool isDone)
    {
        if (taskID < 0 || taskID >= taskCompleted.Length || taskCompleted[taskID]) return;

        if (isDone){
            taskCompleted[taskID] = true;
            messageUI.text = "Completed: " + taskMessages[taskID];
            
            messageUI.CrossFadeAlpha(0, 1f, false);

            Invoke("nextTask", 1f);
        }
        else
        {
            messageUI.text = taskMessages[taskID];
        }
    }

    void nextTask()
    {
        currentTaskNumber++;
        messageUI.canvasRenderer.SetAlpha(1f);
        updateDisplay();
    }

    void updateDisplay()
    {
        if (currentTaskNumber < taskMessages.Length)
        {
            messageUI.CrossFadeAlpha(1f, 0f, false);
            messageUI.text = taskMessages[currentTaskNumber];
            messageUI.color = Color.white;
        }
        else
        {
            messageUI.text = "";
        }
    }
}
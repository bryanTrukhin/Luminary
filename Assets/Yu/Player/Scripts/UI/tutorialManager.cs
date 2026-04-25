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
        if (Input.GetKeyDown(KeyCode.A) && currentTaskNumber == 0) 
        {
            SetTaskStatus(0, true);
        }
        if (Input.GetKeyDown(KeyCode.D)&& currentTaskNumber == 1) 
        {
            SetTaskStatus(1, true);
        }
        if (Input.GetKeyDown(KeyCode.Space)&& currentTaskNumber == 2) 
        {
            SetTaskStatus(2, true);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl)&& currentTaskNumber == 3) 
        {
            SetTaskStatus(3, true);
        }
        if (Input.GetMouseButtonDown(0)&& currentTaskNumber == 4) 
        {
            SetTaskStatus(4, true);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)&& currentTaskNumber == 5) 
        {
            SetTaskStatus(5, true);
        }
    }

    public void SetTaskStatus(int taskID, bool isDone)
    {
        if (taskID < 0 || taskID >= taskCompleted.Length || taskCompleted[taskID]) return;



        if (isDone&&taskID!=4){
            taskCompleted[taskID] = true;
            messageUI.canvasRenderer.SetAlpha(1f);
            messageUI.text = "Completed: " + taskMessages[taskID];
            
            messageUI.CrossFadeAlpha(0, 1f, false);

            Invoke("nextTask", 1f);
        }
        else if(isDone&&taskID==4){
            taskCompleted[taskID] = true;
            messageUI.canvasRenderer.SetAlpha(1f);
            messageUI.text = "Tutorial done! Tip: Press F when near a locker";
            
            messageUI.CrossFadeAlpha(0, 8f, false);
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
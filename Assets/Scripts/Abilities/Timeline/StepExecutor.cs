using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using EasyMeshVR.Multiplayer;

public class StepExecutor : MonoBehaviour
{
    public static StepExecutor instance { get; private set; }

    public InputActionReference testSendColor = null;
    public InputActionReference globalUndo = null;
    public InputActionReference globalRedo = null;

    static Queue<Step> stepBuffer;

    static List<Step> stepHistory;
    static int counter = 0;

    private void Awake()
    {
        instance = this;
        testSendColor.action.started += SendTestColorCommand;
        globalUndo.action.started += UndoInputAction;
        globalRedo.action.started += RedoInputAction;

        stepBuffer = new Queue<Step>();
        stepHistory = new List<Step>();
    }

    private void OnDestroy()
    {
        testSendColor.action.started -= SendTestColorCommand;
        globalUndo.action.started -= UndoInputAction;
        globalRedo.action.started -= RedoInputAction;
    }

    public static void AddStep(Step step)
    {
        // If any changes were undone, future steps need to be removed so we can overwrite future history
        while (stepHistory.Count > counter)
            stepHistory.RemoveAt(counter);

        stepBuffer.Enqueue(step);
    }

    private void Update()
    {
        // Execute steps in execute bugger
        if (stepBuffer.Count > 0)
        {
            // Verify executable step
            Step curStep = stepBuffer.Peek();
            if (curStep.CanBeExecuted())
            {
                curStep.Execute();
                stepBuffer.Dequeue();
                stepHistory.Add(curStep);
                counter++;
                Debug.Log("Command history length: " + stepHistory.Count);
            }
            else
            {
                Debug.LogWarning("Step in step buffer could not be executed! Removing it.");
                stepBuffer.Dequeue();
            }
        }
    }

    private void UndoInputAction(InputAction.CallbackContext context)
    {
        Undo();
        NetworkMeshManager.instance.SynchronizeUndoTimeline();
    }

    public void Undo()
    {
        // Make sure we're not at the beginning of the timeline
        if (counter > 0)
        {
            // Attempt to undo
            if (stepHistory[counter-1].CanBeDeexecuted())
            {
                counter--;
                stepHistory[counter].Deexecute();
            }
            else
            {
                Debug.LogWarning("Step cannot be deexecuted!");
            }
        }
        else
        {
            Debug.Log("No steps left to undo");
        }
    }

    private void RedoInputAction(InputAction.CallbackContext context)
    {
        Redo();
        NetworkMeshManager.instance.SynchronizeRedoTimeline();
    }

    public void Redo()
    {
        // Make sure we're not at the end of the timeline
        if (counter < stepHistory.Count)
        {
            // Attempt to redo
            if (stepHistory[counter].CanBeExecuted())
            {
                stepHistory[counter].Execute();
                counter++;
            }
            else
            {
                Debug.LogWarning("Step cannot be executed!");
            }
        }
        else
        {
            Debug.Log("No steps left to redo");
        }
    }

    public void SendTestColorCommand(InputAction.CallbackContext context)
    {
        Vector3 colorVec = new Vector3(Random.value, Random.value, Random.value);
        SetLightColorOp(colorVec);
        NetworkMeshManager.instance.SynchronizeSetLightColorOp(colorVec);
    }

    public void SetLightColorOp(Vector3 colorVec)
    {
        Step step = new Step();
        SetLightColor op = new SetLightColor(new Color(colorVec.x, colorVec.y, colorVec.z));
        step.AddOp(op);
        StepExecutor.AddStep(step);
    }
}

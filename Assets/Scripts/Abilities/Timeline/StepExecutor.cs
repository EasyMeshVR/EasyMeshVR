using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using EasyMeshVR.Multiplayer;
using Photon.Pun;

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

        counter = 0;
        stepBuffer = new Queue<Step>();
        stepHistory = new List<Step>();
    }

    private void OnDestroy()
    {
        testSendColor.action.started -= SendTestColorCommand;
        globalUndo.action.started -= UndoInputAction;
        globalRedo.action.started -= RedoInputAction;
    }

    // Called when canvas is cleared or a new model is imported to get rid of old steps
    public void ClearSteps()
    {
        counter = 0;
        stepBuffer.Clear();
        stepHistory.Clear();
    }

    public void AddStep(Step step)
    {
        // If any changes were undone, future steps need to be removed so we can overwrite future history
        while (stepHistory.Count > counter)
            stepHistory.RemoveAt(counter);

        stepBuffer.Enqueue(step);
        UpdateHistory();
    }

    private void UpdateHistory()
    {
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

    private void Update()
    {
        // Execute steps in execute bugger
        UpdateHistory();
    }

    private void UndoInputAction(InputAction.CallbackContext context)
    {
        Undo();

        UndoTimelineEvent undoTimelineEvent = new UndoTimelineEvent
        {
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
            isCached = true
        };
        NetworkMeshManager.instance.SynchronizeUndoTimeline(undoTimelineEvent);
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

        RedoTimelineEvent redoTimelineEvent = new RedoTimelineEvent
        {
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
            isCached = true
        };
        NetworkMeshManager.instance.SynchronizeRedoTimeline(redoTimelineEvent);
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

        ChaneLightColorEvent changeLightColorEvent = new ChaneLightColorEvent
        {
            actorNumber = PhotonNetwork.LocalPlayer.ActorNumber,
            colorVec = colorVec,
            isCached = true
        };

        NetworkMeshManager.instance.SynchronizeSetLightColorOp(changeLightColorEvent);
    }

    public void SetLightColorOp(Vector3 colorVec)
    {
        Step step = new Step();
        SetLightColor op = new SetLightColor(new Color(colorVec.x, colorVec.y, colorVec.z));
        step.AddOp(op);
        AddStep(step);
    }
}

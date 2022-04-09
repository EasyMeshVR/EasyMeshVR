using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step
{
    private List<IOperation> opList;

    public Step(List<IOperation> newOpList = null)
    {
        if (opList == null)
        {
            if (newOpList == null)
                opList = new List<IOperation>();
            else
                opList = newOpList;
        }
    }

    public void Execute()
    {
        foreach (IOperation o in opList)
            o.Execute();
    }

    public bool CanBeExecuted()
    {
        foreach (IOperation o in opList)
            if (!o.CanBeExecuted())
                return false;
        return true;
    }

    public void Deexecute()
    {
        opList.Reverse();
        foreach (IOperation o in opList)
            o.Deexecute();
        opList.Reverse();
    }
    
    /// <summary>
    /// Check if a user's attempt to undo is being blocked by another users action.
    /// Currently just a stub that returns true, as multi-user timeline traversal is not implemented yet.
    /// </summary>
    /// <returns>true</returns>
    public bool CanBeDeexecuted()
    {
        // TODO: Multi-user timeline traversal
        //foreach (IOperation o in opList)
        //    if (!o.CanBeDeexecuted())
        //        return false;
        return true;
    }

    public void AddOp(IOperation op)
    {
        opList.Add(op);
    }
}

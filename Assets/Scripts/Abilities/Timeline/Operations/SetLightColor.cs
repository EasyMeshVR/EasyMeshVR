using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLightColor : IOperation
{
    Color oldColor, newColor;

    public SetLightColor(Color inputColor)
    {
        oldColor = GameObject.FindObjectOfType<Light>().color;
        newColor = inputColor;
    }

    public void Execute()
    {
        GameObject.FindObjectOfType<Light>().color = newColor;
    }

    bool IOperation.CanBeExecuted()
    {
        return true;
    }

    public void Deexecute()
    {
        GameObject.FindObjectOfType<Light>().color = oldColor;
    }

    public bool CanBeDeexecuted()
    {
        return true;
    }
}

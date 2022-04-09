using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOperation
{
    void Execute();
    bool CanBeExecuted();

    void Deexecute();
    bool CanBeDeexecuted();
}

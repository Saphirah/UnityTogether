using System;
using UnityEngine;

public class WaitUntilEditor : CustomYieldInstruction
{
    Func<bool> _predicate;

    public override bool keepWaiting
    {
        get { return !_predicate(); }
    }

    public WaitUntilEditor(Func<bool> predicate)
    {
        _predicate = predicate;
    }
}
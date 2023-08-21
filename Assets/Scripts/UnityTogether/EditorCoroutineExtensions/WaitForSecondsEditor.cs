using System;
using UnityEngine;

public class WaitForSecondsEditor : CustomYieldInstruction
{
    DateTime _targetTime;

    public override bool keepWaiting
    {
        get { return DateTime.UtcNow < _targetTime; }
    }

    public WaitForSecondsEditor(float seconds)
    {
        _targetTime = DateTime.UtcNow.AddSeconds(seconds);
    }
}
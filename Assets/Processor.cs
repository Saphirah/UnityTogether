using System;
using UnityEngine;

public abstract class Processor
{
    protected readonly UnityTogetherClient communication;

    protected Processor(UnityTogetherClient com)
    {
        communication = com;
        communication.OnMessageReceived += MessageReceived;
    }
    
    private void MessageReceived(int index, string msg, string userID)
    {
        try
        {
            OnMessageReceived(index, msg, userID);
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    protected abstract void OnMessageReceived(int index, string msg, string userID);
}
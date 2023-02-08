public abstract class Processor
{
    protected Communication communication;
    
    public Processor(Communication com)
    {
        communication = com;
        communication.OnMessageReceived += OnMessageReceived;
    }
    
    protected abstract void OnMessageReceived(int index, string msg, string userID);
}
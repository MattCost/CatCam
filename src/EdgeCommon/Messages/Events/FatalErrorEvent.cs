namespace CatCam.EdgeCommon.Messages.Events;

public delegate void FatalErrorEventOccurred(object sender, FatalErrorEventArgs e);
public class FatalErrorEventArgs : EventArgs
{
    public Exception Exception  { get; set; }   
    public string Message { get; set; } 
    public FatalErrorEventArgs(Exception exception, string message)
    {
        Exception = exception;
        Message = message;
    }
}
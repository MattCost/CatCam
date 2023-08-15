namespace CatCam.EdgeCommon.Messages.Exceptions
{
    public class InvalidPropertyException : Exception
    {
        public InvalidPropertyException(string message, Exception? innerEx = null) : base(message, innerEx){}

    }
    
}
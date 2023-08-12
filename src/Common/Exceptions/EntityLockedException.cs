namespace CatCam.Common.Exceptions;

public class EntityLockedException : Exception
{
    public EntityLockedException(string entityName, Exception? innerEx = null) : base($"Entity {entityName} locked.", innerEx)
    {

    }    
    public EntityLockedException(string entityName, string message, Exception? innerEx = null) : base($"Entity {entityName} locked. Message {message}", innerEx)
    {

    }

}
namespace CatCam.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entityName, Exception? innerEx = null) : base($"Entity {entityName} not found.", innerEx)
    {

    }    
    public EntityNotFoundException(string entityName, string message, Exception? innerEx = null) : base($"Entity {entityName} not found. Message {message}", innerEx)
    {

    }

}
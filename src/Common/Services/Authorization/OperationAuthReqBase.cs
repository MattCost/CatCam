using Microsoft.AspNetCore.Authorization;

namespace CatCam.Common.Services.Authorization;

public class OperationAuthReqBase : IAuthorizationRequirement
{
    public OperationAuthReqBase(string name) => Name = name;
    public string Name { get; init; }
    public override string ToString()
    {
        return Name;
    }
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public static bool operator==(OperationAuthReqBase left,OperationAuthReqBase right)
    {
        return left.Name == right.Name;
    }
    public static bool operator!=(OperationAuthReqBase left,OperationAuthReqBase right)
    {
        return left.Name != right.Name;
    }
}


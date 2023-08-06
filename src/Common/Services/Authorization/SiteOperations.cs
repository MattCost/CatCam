namespace CatCam.Common.Services.Authorization;

public class SiteOperations : OperationAuthReqBase
{
    public SiteOperations(string name) : base(name) { }

    public readonly static SiteOperations ListSites = new(nameof(ListSites));
    public readonly static SiteOperations CreateSites = new(nameof(CreateSites));
    public readonly static SiteOperations DeleteSites = new(nameof(CreateSites));
}
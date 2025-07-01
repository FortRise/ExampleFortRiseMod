namespace Teuria.WiderSet;

public sealed class ApiImplementation : IWiderSetModApi
{
    public bool IsWide => WiderSetModule.IsWide;
}
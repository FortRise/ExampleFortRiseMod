namespace BartizanMod;

public interface IWiderSetModApi 
{
    bool IsWide { get; }
}

public static class WiderSetUtils 
{
    public static int GetLeftOffset
    {
        get
        {
            var widerSetApi = BartizanModModule.Instance.WiderSetApi;
            if (widerSetApi is null)
            {
                return 0;
            }

            return widerSetApi.IsWide ? 55 : 0;
        }
    }

    public static int GetScreenWidth()
    {
        var widerSetApi = BartizanModModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 320;
        }
        return widerSetApi.IsWide ? 420 : 320;
    }

    public static int GetPlayerCount() 
    {
        var widerSetApi = BartizanModModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 4;
        }
        return widerSetApi.IsWide ? 8: 4;
    }

    public static int GetMenuPlayerCount() 
    {
        var widerSetApi = BartizanModModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 4;
        }
        return widerSetApi.IsWide ? 8: 4;
    }
}
namespace Teuria.BaronMode.Interop;

public static class EightPlayerUtils 
{
    public static int GetScreenWidth() 
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 320;
        }
        return widerSetApi.IsWide ? 320 : 420;
    }

    public static int GetUIOffset()
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 0;
        }
        return (int)widerSetApi.UIXOffset;
    }

    public static int GetPlayerCount() 
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 4;
        }
        return widerSetApi.IsWide ? 8: 4;
    }

    public static int GetMenuPlayerCount() 
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 4;
        }
        return widerSetApi.IsWide ? 8: 4;
    }
}

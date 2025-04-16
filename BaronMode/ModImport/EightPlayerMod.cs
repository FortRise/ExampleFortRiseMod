namespace Teuria.BaronMode.Interop;

public interface IWiderSetModApi 
{
    bool IsEightPlayer { get; }
    bool LaunchedEightPlayer { get; }
}

public static class EightPlayerUtils 
{
    public static int GetScreenWidth() 
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 320;
        }
        return widerSetApi.IsEightPlayer ? 420 : 320;
    }

    public static int GetPlayerCount() 
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 4;
        }
        return widerSetApi.IsEightPlayer ? 8: 4;
    }

    public static int GetMenuPlayerCount() 
    {
        var widerSetApi = BaronModeModule.Instance.WiderSetApi;
        if (widerSetApi is null)
        {
            return 4;
        }
        return widerSetApi.LaunchedEightPlayer ? 8: 4;
    }
}

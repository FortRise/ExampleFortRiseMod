using System;
using MonoMod.ModInterop;

namespace BaronMode.Interop;


[ModImportName("com.fortrise.EightPlayerMod")]
public class EightPlayerImport
{
    public static Func<bool> IsEightPlayer = null!;
    public static Func<bool> LaunchedEightPlayer = null!;
}

public static class EightPlayerUtils 
{
    public static int GetScreenWidth() 
    {
        return BaronModeModule.EightPlayerMod ? EightPlayerImport.IsEightPlayer() ? 420 : 320 : 320;
    }

    public static int GetPlayerCount() 
    {
        return BaronModeModule.EightPlayerMod ? EightPlayerImport.IsEightPlayer() ? 8 : 4 : 4;
    }

    public static int GetMenuPlayerCount() 
    {
        return BaronModeModule.EightPlayerMod ? EightPlayerImport.LaunchedEightPlayer() ? 8 : 4 : 4;
    }
}
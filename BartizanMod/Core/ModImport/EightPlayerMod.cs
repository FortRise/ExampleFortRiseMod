using System;
using MonoMod.ModInterop;

namespace BartizanMod;


[ModImportName("com.fortrise.EightPlayerMod")]
public class EightPlayerImport
{
    public static Func<bool> IsEightPlayer;
    public static Func<bool> LaunchedEightPlayer;
}

public static class EightPlayerUtils 
{
    public static int GetScreenWidth() 
    {
        return BartizanModModule.EightPlayerMod ? EightPlayerImport.IsEightPlayer() ? 420 : 320 : 320;
    }

    public static int GetPlayerCount() 
    {
        return BartizanModModule.EightPlayerMod ? EightPlayerImport.IsEightPlayer() ? 8 : 4 : 4;
    }

    public static int GetMenuPlayerCount() 
    {
        return BartizanModModule.EightPlayerMod ? EightPlayerImport.LaunchedEightPlayer() ? 8 : 4 : 4;
    }
}
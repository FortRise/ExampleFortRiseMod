using Monocle;
using FortRise;
using System;

namespace ExampleMod;

public class ExampleModSettings : ModuleSettings 
{
    [SettingsName("Cheat Mode")]
    public bool CheatMode;
    
    [SettingsNumber(0, 20, 2)]
    public int OnStepping;

    public Action FlightTest;
}

public static class CommandList 
{
    [Command("hello")]
    public static void SayHello(string[] args) 
    {
        Engine.Instance.Commands.Log("Hello");
    }
}
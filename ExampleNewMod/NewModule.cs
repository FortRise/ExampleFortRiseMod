using System;
using ExampleNewMod;
using FortRise;

namespace Teuria.NewExampleMod;

/* 


This is an exmaple mod, please remove all of the code except Load, Unload and Initialize 
to get started.


*/

internal class NewModule : FortModule
{
    public override void Initialize()
    {
        TriggerBrambleArrow.Register(Registry);
        PinkSlime.Register(Registry);
    }

    public override void Load()
    {
        TriggerBrambleArrow.Load();
        PinkSlime.Load();
    }

    public override void Unload()
    {
        TriggerBrambleArrow.Unload();
        PinkSlime.Unload();
    }

    // get to have the other mod get the API
    public override object? GetApi()
    {
        return new ApiImplementation();
    }
}


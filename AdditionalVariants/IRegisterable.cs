using FortRise;

namespace Teuria.AdditionalVariants;


internal interface IHookable 
{
    abstract static void Load();
    abstract static void Unload();
}

internal interface IRegisterable
{
    abstract static void Register(IModRegistry registry);
}
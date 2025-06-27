using FortRise;

namespace Teuria.BaronMode;

internal interface IHookable 
{
    abstract static void Load(IHarmony harmony);
}

internal interface IRegisterable
{
    abstract static void Register(IModContent content, IModRegistry registry);
}
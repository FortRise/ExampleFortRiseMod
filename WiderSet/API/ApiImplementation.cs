using FortRise;
using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

public sealed class ApiImplementation : IWiderSetModApi
{
    public ApiImplementation() {}

    public bool IsWide 
    {
        get => WiderSetModule.IsWide;
        set => WiderSetModule.IsWide = value;
    }

    public bool IsHoveringWide 
    {
        get => WiderSetModule.AboutToGetWide;
        set => WiderSetModule.AboutToGetWide = value;
    }

    public Matrix WideIdentity => MatrixUtilities.IdentityFixed;
    public float UIXOffset => WiderSetModule.IsWide ? 50 : 0;

    public IWiderSetModApi.IWiderVersusLevelApi WiderVersusLevelApi { get; } = new WiderVersusLevelApi();

    public void SetNotJoinedArcherOffset(int archerID, Vector2 offset, IWiderSetModApi.ArcherType type)
    {
        switch (type)
        {
            case IWiderSetModApi.ArcherType.Normal:
                WiderSetModule.NotJoinedCharacterOffset[archerID] = offset;
                break;
            case IWiderSetModApi.ArcherType.Alt:
                WiderSetModule.NotJoinedAltCharacterOffset[archerID] = offset;
                break;
            case IWiderSetModApi.ArcherType.Secret:
                WiderSetModule.NotJoinedSecretCharacterOffset[archerID] = offset;
                break;
        }
    }

    public void SetJoinedArcherOffset(int archerID, Vector2 offset, IWiderSetModApi.ArcherType type)
    {
        switch (type)
        {
            case IWiderSetModApi.ArcherType.Normal:
                WiderSetModule.JoinedCharacterOffset[archerID] = offset;
                break;
            case IWiderSetModApi.ArcherType.Alt:
                WiderSetModule.JoinedAltCharacterOffset[archerID] = offset;
                break;
            case IWiderSetModApi.ArcherType.Secret:
                WiderSetModule.JoinedSecretCharacterOffset[archerID] = offset;
                break;
        }
    }
}

public sealed class WiderVersusLevelApi : IWiderSetModApi.IWiderVersusLevelApi
{
    public void RegisterWideLevelsForVersusTower(string levelID, IResourceInfo[] levels)
    {
    }
}

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
}

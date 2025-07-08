using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

public sealed class ApiImplementation : IWiderSetModApi
{
    public bool IsWide => WiderSetModule.IsWide;

    public Matrix WideIdentity => MatrixUtilities.IdentityFixed;
}
using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

public interface IWiderSetModApi
{
    bool IsWide { get; }
    Matrix WideIdentity { get; }
}

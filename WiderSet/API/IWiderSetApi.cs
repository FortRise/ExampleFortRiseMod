using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

public interface IWiderSetModApi
{
    bool IsWide { get; }
    Matrix WideIdentity { get; }

    void SetNotJoinedArcherOffset(int archerID, Vector2 offset, ArcherType type);

    public enum ArcherType { Normal, Alt, Secret }
}

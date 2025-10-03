using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

public partial interface IWiderSetModApi
{
    public IWiderVersusLevelApi WiderVersusLevelApi { get; }


    /// <summary>
    /// A property for the game enters wide mode.
    /// </summary>
    bool IsWide { get; set; }

    /// <summary>
    /// A property for pads if it needs to transition in or out.
    /// </summary>
    bool IsHoveringWide { get; set; }

    /// <summary>
    /// The matrix identity to use for <see cref="Monocle.Layer"/> UI matrix in wide mode. 
    /// </summary>
    Matrix WideIdentity { get; }

    /// <summary>
    /// The offset that automatically adjust on the state. 
    /// </summary>
    /// <returns>0 if standard mode, 55 if wide mode</returns>
    float UIXOffset { get; }

    void SetNotJoinedArcherOffset(int archerID, Vector2 offset, ArcherType type);
    void SetJoinedArcherOffset(int archerID, Vector2 offset, ArcherType type);

    public enum ArcherType { Normal, Alt, Secret }
}

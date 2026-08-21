using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

public interface IWiderSetModApi
{
    /// <summary>
    /// A property for the game enters wide mode.
    /// </summary>
    bool IsWide { get; set; }

    /// <summary>
    /// The matrix identity to use for <see cref="Monocle.Layer"/> UI matrix in wide mode. 
    /// </summary>
    Matrix WideIdentity { get; }

    /// <summary>
    /// The offset that automatically adjust on the state. 
    /// </summary>
    /// <returns>0 if standard mode, 55 if wide mode</returns>
    float UIXOffset { get; }
}

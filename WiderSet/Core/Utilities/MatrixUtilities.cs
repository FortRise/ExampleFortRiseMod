using Microsoft.Xna.Framework;

namespace Teuria.WiderSet;

internal static class MatrixUtilities
{
    public static readonly Matrix IdentityFixed;
    static MatrixUtilities()
    {
        IdentityFixed = Matrix.CreateTranslation(
            new Vector3(-new Vector2(-50, 0), 0f)) *
            Matrix.CreateRotationZ(0) *
            Matrix.CreateScale(new Vector3(1, 1, 1f)) *
            Matrix.CreateTranslation(new Vector3(0, 0, 0f));
    }
}
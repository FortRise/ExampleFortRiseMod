using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace MiasmaColors;

public class SubTextureModifier
{
    private string subtextureName;
    
    private Color[] originalColors;
    private Subtexture texture;

    public SubTextureModifier(string subtextureName)
    {
        this.subtextureName = subtextureName;
        texture = TFGame.Atlas[subtextureName];
        originalColors = new Color[texture.Width * texture.Height];
        TFGame.Atlas.Texture2D.GetData(0, texture.Rect, originalColors, 0, originalColors.Length);
    }

    public void ApplyHueShift(float angle)
    {
        Color[] newColors = new Color[originalColors.Length]; 
        for (int i = 0; i < newColors.Length; i++) 
        {
            if (originalColors[i].A == 0)
            {
                newColors[i] = originalColors[i];
                continue;
            }

            HSV hsvColor = ColorUtils.ColorToHSV(originalColors[i]);
            ColorUtils.HueShift(ref hsvColor, angle);
            newColors[i] = ColorUtils.HSVToColor(hsvColor, originalColors[i].A);
        }
        TFGame.Atlas.Texture2D.SetData(0, texture.Rect, newColors, 0, newColors.Length);
    }

    public void Reset()
    {
        TFGame.Atlas.Texture2D.SetData(0, texture.Rect, originalColors, 0, originalColors.Length);
    }
}
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace Teuria.Fortfeit;

public class ForfeitButtonGuide : Component
{
    public Vector2 RenderPosition => Entity != null ? Entity.Position + Position : Position;

    private const int AT_X = 310;
    public Vector2 Position;
    private string title;
    private Subtexture? icon;


    public ForfeitButtonGuide(int y, string title)
        : base(true, true)
    {
        Position = new Vector2(AT_X, y);
        this.title = title;
        icon = TFGame.MenuAtlas["controls/xb360/y"];
    }

    public override void Render()
    {
        if (icon != null)
        {
            float x = TFGame.Font.MeasureString(title).X;
            float num = icon.Width + 4 + x;
            Draw.TextureCentered(icon, RenderPosition + Vector2.UnitX * (float)-(float)icon.Width / 2f, Color.White, 1f, 0f);
            Draw.TextCentered(TFGame.Font, title, RenderPosition + Vector2.UnitX * (-num + x / 2f), Color.White);
        }
    }

    public void Clear()
    {
        title = "";
        icon = null;
    }
}
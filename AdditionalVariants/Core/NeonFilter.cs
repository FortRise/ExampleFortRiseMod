using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Teuria.AdditionalVariants;

public class NeonFilter : SceneFilter, IHookable
{
    public NeonFilter() 
    {
    }


    public override void AfterRender(RenderTarget2D canvas)
    {
    }

    public override void BeforeRender(RenderTarget2D canvas)
    {

    }

    public override void Render(RenderTarget2D canvas)
    {
        var shader = AdditionalVariantsModule.NeonShader.EffectResource as NeonShaderResource;
        if (shader is null)
        {
            return;
        }
        Draw.SpriteBatch.BeginShaderRegion();
        
        shader.UseTexture1(ForegroundRenderTarget);
        shader.Apply();
        Draw.SpriteBatch.Draw(ForegroundRenderTarget, Vector2.Zero, Color.White);

        Draw.SpriteBatch.EndShaderRegion();
    }

    public override void Activated(LevelRenderData data)
    {
        base.Activated(data);
        BGTiles.Visible = false;
        if (Level.Background != null)
            Level.Background.Visible = false;

        if (Level.Foreground != null)
            Level.Foreground.Visible = false;
    }

    public override void Deactivated()
    {
        base.Deactivated();
        BGTiles.Visible = true;
        if (Level.Background != null)
            Level.Background.Visible = true;

        if (Level.Foreground != null)
            Level.Foreground.Visible = true;
    }

    public static void Load(IHarmony harmony)
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(TowerFall.Level), nameof(TowerFall.Level.Begin)),
            postfix: new HarmonyMethod(Level_Begin_Postfix)
        );
    }

    private static void Level_Begin_Postfix(TowerFall.Level __instance)
    {
        if (Variants.NeonWorld.IsActive())
        {
            var filter = new NeonFilter();
            __instance.Activate(filter);
        }
    }
}

public class NeonShaderResource : MiscEffectResource
{
    private Vector2 texelSize = new Vector2(1.0f / (320f - 1f), 1.0f / (240f - 1f));
    private Vector4 outlineColor = Vector4.One;

    public void Use(Vector2 texelSize, Vector4 outlineColor) 
    {
        this.texelSize = texelSize;
        this.outlineColor = outlineColor;
    }

    public override void Apply()
    {
        Shader.Parameters["uTexelSize"].SetValue(texelSize);
        Shader.Parameters["uOutlineColor"].SetValue(outlineColor);
        base.Apply();
    }
}
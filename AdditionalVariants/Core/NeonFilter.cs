using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace AdditionalVariants;

public class NeonFilter : ShaderFilter
{
    private RenderTarget2D neonRT;
    public NeonFilter() 
    {
        neonRT = new RenderTarget2D(Engine.Instance.GraphicsDevice, 320, 240);
    }


    public override void AfterRender(RenderTarget2D canvas)
    {
        // Override the main canvas
        Engine.Instance.GraphicsDevice.SetRenderTarget(canvas);
        Draw.SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, 
            DepthStencilState.None, RasterizerState.CullNone);
        Draw.SpriteBatch.Draw(neonRT, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
    }

    public override void BeforeRender(RenderTarget2D canvas)
    {

    }

    public override void Render(RenderTarget2D canvas)
    {
        var shader = AdditionalVariantsModule.NeonShader;
        Engine.Instance.GraphicsDevice.SetRenderTarget(neonRT);
        Draw.SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, 
            DepthStencilState.None, RasterizerState.CullNone, shader.Shader);

        shader.Apply();
        Draw.SpriteBatch.Draw(ForegroundRenderTarget, Vector2.Zero, Color.White);
        Draw.SpriteBatch.End();
    }

    public override void Activated(LevelRenderData data)
    {
        base.Activated(data);
        BGTiles.Active = false;
        BGTiles.Visible = false;
    }
}

public class NeonShaderResource : MiscShaderResource
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
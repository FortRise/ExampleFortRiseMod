// ----------------- ModLoader Parameters ---------------------
float4 uColor;
float uDeltaTime;

sampler2D uTexture0 : register(s0);
sampler2D uTexture1 : register(s1);
sampler2D uTexture2 : register(s2);

float2 uTextureSize0;
float2 uTextureSize1;
float2 uTextureSize2;
// ----------------- Owned Parameters -------------------------

float2 uTexelSize;
float4 uOutlineColor;

// ------------------------------------------------------------

struct VertexOutput {
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

float4 NeonPS(in VertexOutput input) : COLOR0 {
	float2 pos = input.TexCoord.xy;
	float4 color = tex2D(uTexture1, pos) * input.Color;
	color.rgb = color.r * 0 + color.g * 0 + color.b * 0;

	float center = ceil(color.a);

	pos = float2(input.TexCoord.x - uTexelSize.x, input.TexCoord.y);
	float left = ceil(tex2D(uTexture1, pos).a) * ceil(pos.x);

	pos = float2(input.TexCoord.x + uTexelSize.x, input.TexCoord.y);
	float right = ceil(tex2D(uTexture1, pos).a) * 1.0f - floor(pos.x);

	pos = float2(input.TexCoord.x, input.TexCoord.y - uTexelSize.y);
	float up = ceil(tex2D(uTexture1, pos).a) * ceil(pos.y);

	pos = float2(input.TexCoord.x, input.TexCoord.y + uTexelSize.y);
	float down = ceil(tex2D(uTexture1, pos).a) * 1.0f - floor(pos.y);

	float total = left + right + up + down;
	if (center > 0.0f && total < 4.0f)
		color = uOutlineColor;

	return color;
}

technique Technique1 
{
	pass Neon
	{
		PixelShader = compile ps_2_0 NeonPS();
	}
}
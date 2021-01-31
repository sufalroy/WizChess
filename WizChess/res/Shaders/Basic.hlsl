#type vertex

struct VertexInput
{
	float3 position : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

struct VertexOutput
{
	float4 position : SV_POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

cbuffer CameraData : register(b0)
{
	float4x4 viewProj;
}

cbuffer ObjectData : register(b1)
{
	float4x4 world;
}

VertexOutput main(VertexInput input)
{
	VertexOutput output;

	float4 position = float4(input.position, 1.0F);
	output.position = mul(position, world);
	output.position = mul(output.position, viewProj);
	output.normal = input.normal;
	output.uv = input.uv;
	return output;
}

#type pixel

Texture2D texture0 : register(t0);

SamplerState textureSampler : register(s0);

struct PixelInput
{
	float4 position : SV_POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
};

float4 main(PixelInput input) : SV_TARGET
{
	return texture0.Sample(textureSampler, input.uv);
}

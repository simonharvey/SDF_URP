Shader "Unlit/JustDepth"
{
	SubShader{
		Pass{
		Material{
		Diffuse(1,0,0,1)
	}
		Lighting Off
		ZTest Less
		ColorMask 0
	}
	}
}
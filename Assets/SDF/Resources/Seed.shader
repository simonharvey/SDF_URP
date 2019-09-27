Shader "Hidden/SDF/Seed"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4x4 _Mat;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 pos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(_Mat, UnityObjectToClipPos(v.vertex));
				//o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = v.uv;
				//o.pos = v.uv;
				o.pos = o.vertex * .5 + .5;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float3 _Dst_TexelSize;

			fixed4 frag (v2f i) : SV_Target
			{
				i.pos.y = 1 - i.pos.y;
				fixed4 col = tex2D(_MainTex, i.uv);

				//fixed u = step(.01, tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y)).a);
				//fixed d = step(.01, tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y)).a);
				//fixed r = step(.01, tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0)).a);
				//fixed l = step(.01, tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0)).a);
				//
				////if (u.a*d.a*r.a*l.a == 0)
				////	return fixed4(1, 0, 0, 1);
				//
				//float o = u*d*r*l < 1;
				//fixed2 outColor = i.pos * step(.01, col.a);
				//float inside = 1.0-step(col.a, 0);
				//return float4(o * step(.1, col.a), col.g, 0, 1);

				//tex2D
				
				return fixed4(1, 0, 0, 1);
				return fixed4(i.pos, 0/*col.a*/, step(col.a, .1));
				//return fixed4(i.pos*o* col.a, col.a, o * inside);
			}
			ENDCG
		}
	}
}

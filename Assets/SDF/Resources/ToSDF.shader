Shader "Hidden/SDF/SDF"
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _SDFSize;

			fixed4 frag (v2f i) : SV_Target
			{
				float sdfSize = _MainTex_TexelSize * _SDFSize;
				fixed4 col = tex2D(_MainTex, i.uv);

				float2 off = abs(col.xy - i.uv.xy) / sdfSize;
				float d = saturate(length(off)) * .5;
				float inside = col.b;
				if (!inside) d = -d;
				//if (inside) return float4(1, 0, 0, 1);
				return float4(float3(.5, .5, .5) + d, 1);
				return float4(d, d, d, 1);
				/*float d = length(abs(col.xy - i.uv.xy));
				
				//return col;
				return float4(d, d, d, 1);
				//return float4(i.uv.xy, 0, 1);
				return col;*/
			}
			ENDCG
		}
	}
}

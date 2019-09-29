Shader "SDF"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		//Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
		
		//Blend SrcAlpha OneMinusSrcAlpha

		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Seed"

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
		
			#include "UnityCG.cginc"
		
			//float4x4 _Mat;
			sampler2D _MainTex;

			float4 _MainTex_TexelSize;

			struct v2f
			{
				float4 uv : TEXCOORD0;
				//float2 pos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				o.uv = float4(v.texcoord.xy, 0, 0);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float c = tex2D(_MainTex, i.uv).a;
				float inside = 1.0 - step(c, 0.01);
				float4 res;
				res.xy = i.uv;// * inside;
				res.z = res.w = inside;
				return res;
			}

			ENDCG
		}

		Pass
		{
			Name "Flood"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float2 _Offset;
			float4x4 _Mat;

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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			float4 _MainTex_TexelSize;

			void Flood(in float2 origin, in float2 coord, inout float4 closestPos, inout float minDist)
			{
				float4 v = tex2D(_MainTex, coord);
				//v.y = 1 - v.y;
				if (v.a > 0)
				{
					float d = length(origin.xy - v.xy);// *_MainTex_TexelSize.zw) / _MainTex_TexelSize.zw;
					if (d < minDist)
					{
						minDist = d;
						closestPos.xy = v;
						closestPos.a = 1;
					}
				}
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 origin = i.uv;
				//float4 closestPos = float4(0, 0, 0, 0);
				float4 closestPos = tex2D(_MainTex, origin);
				float minDist = 1000.0;

				for (int y=-1; y<=1; ++y)
				{
					for (int x=-1; x<=1; ++x)
					{
						Flood(origin, origin + _Offset * float2(x, y), closestPos, minDist);
					}
				}

				//float4 c = tex2D(_MainTex, origin);
				//closestPos.b = c.b;
				//closestPos.a = c.a;
				return closestPos;
			}

			ENDCG
		}

		Pass
		{
			Name "SDF"

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
				float2 pos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.pos = v.uv;// o.vertex * .5 + .5;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _SDFSize;

			float4 frag(v2f i) : SV_Target
			{
				float2 sdfSize = _MainTex_TexelSize.zw / _SDFSize;// / _MainTex_ST.xy;
				float4 n = tex2D(_MainTex, i.uv);
				float2 p = i.uv;

				//return float4(n.xy, 0, 0);
				//return float4(p, 0, 0);

				float2 off = n.xy - p.xy;
				off = normalize(off);
				float dist = length(n.b * _MainTex_TexelSize.zw / _SDFSize);
				return float4(off * .5 + .5, 0, 1);// *_MainTex_TexelSize.zw / _MainTex_TexelSize.zw), 1);
			}
			ENDCG
		}
	}
}

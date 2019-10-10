Shader "SDF"
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
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				o.uv = v.texcoord;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 c = tex2D(_MainTex, i.uv);
				float inside = c.a > 0;
				float4 res;
				res.xy = i.uv;
				res.z = res.w = inside;
				return res;
			}

			ENDCG
		}

		Pass
		{
			Name "OutlineSeed"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
#pragma multi_compile _ PIXELSNAP_ON
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

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
//#ifdef PIXELSNAP_ON
//				o.vertex = UnityPixelSnap(o.vertex);
//#endif
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			float SampleAlphaCutout(float2 uv)
			{
				return step(tex2D(_MainTex, uv).a, 0.001);
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 c = tex2D(_MainTex, i.uv);
				float centerCut = step(c.a, 0.001);
				
				float u =  SampleAlphaCutout(i.uv + float2(0, _MainTex_TexelSize.y));
				float d =  SampleAlphaCutout(i.uv - float2(0, _MainTex_TexelSize.y));
				float r =  SampleAlphaCutout(i.uv + float2(_MainTex_TexelSize.x, 0));
				float l =  SampleAlphaCutout(i.uv - float2(_MainTex_TexelSize.x, 0));

				float kernel =  (u*d*r*l) + centerCut;
				float o = kernel == 1;
				
				float inside = 1.0 - step(c.a, 0);
				return float4(i.uv.xy, inside, o);
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
				if (v.a > 0)
				{
					float d = length((origin.xy - v.xy)*_MainTex_TexelSize.zw);
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
				float4 closestPos = tex2D(_MainTex, origin);
				float minDist = 10000000000.0;
				float2 off = _Offset.x * _MainTex_TexelSize.xy;

				for (int y=-1; y<=1; ++y)
				{
					for (int x=-1; x<=1; ++x)
					{
						Flood(origin, origin + off * float2(x, y), closestPos, minDist);
					}
				}

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

			float4 frag(v2f i) : SV_Target
			{
				float4 res = tex2D(_MainTex, i.uv);
				float inside = res.b;
				float d = length((i.uv - res.rg)*_MainTex_TexelSize.zw)*max(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
				if (inside)
				{
					d = (-d);
				}

				res.b = d + .5;
				return res;
			}

			ENDCG
		}

		Pass
		{
			Name "CamSeed"

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
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeScreenPos(o.vertex);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				return float4(1, 0, 0, 1);
			}

			ENDCG
		}
	}
}

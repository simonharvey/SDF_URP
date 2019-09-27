Shader "Hidden/SDF/Flood"
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

			float2 _Offset;

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

			void Flood(in float2 origin, in float2 coord, inout float4 closestPos, inout float minDist)
			{
				float4 v = tex2D(_MainTex, coord);
				//v.y = 1.0 - v.y;
				if (v.a > 0)
				{
					float d = length(origin - v.xy);
					if (d < minDist)
					{
						minDist = d;
						closestPos = v;
					}
				}
			}

			float4 frag (v2f i) : SV_Target
			{
				float2 origin = i.uv;
				//float2 origin = tex2D(_MainTex, i.uv);
				//origin.y = 1.0 - origin.y;

				float4 closestPos = float4(0, 0, 0, 0);
				float minDist = 100000.0;
				
				// +
				Flood(origin, origin, closestPos, minDist);
				Flood(origin, origin + _Offset * float2(-1.0,  0.0), closestPos, minDist);
				Flood(origin, origin + _Offset * float2( 1.0,  0.0), closestPos, minDist);
				Flood(origin, origin + _Offset * float2( 0.0,  1.0), closestPos, minDist);
				Flood(origin, origin + _Offset * float2( 0.0, -1.0), closestPos, minDist);
				// x
				Flood(origin, origin + _Offset * float2(-1.0, -1.0), closestPos, minDist);
				Flood(origin, origin + _Offset * float2( 1.0, -1.0), closestPos, minDist);
				Flood(origin, origin + _Offset * float2(-1.0,  1.0), closestPos, minDist);
				Flood(origin, origin + _Offset * float2( 1.0,  1.0), closestPos, minDist);

				//return float4(origin, 0, 1);

				closestPos.b = tex2D(_MainTex, origin).b;
				return closestPos;
			}

			

			ENDCG
		}
	}
}

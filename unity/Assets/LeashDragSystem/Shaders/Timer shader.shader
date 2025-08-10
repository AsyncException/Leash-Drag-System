Shader "VRChatOSCLeash/Counter"
{
    Properties
    {
        _NumberAtlas("Number Atlas", 2D) = "white" {}
        _Hour("Hour", Float) = 0
        _Minute("Minute", Float) = 0
        _Second("Second", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        [HDR] _EmissionColor("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength("Emission Strength", Range(0,20)) = 1
        _HueShift("Hue Shift Offset", Range(0,1)) = 0
        _HueSpeed("Hue Speed", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            AlphaToMask On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _NumberAtlas;
            uint _Hour;
            uint _Minute;
            uint _Second;
            float _Cutoff;
            fixed4 _EmissionColor;
            float _EmissionStrength;
            float _HueShift;
            float _HueSpeed;

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

            fixed3 HueShift(fixed3 color, float shift)
            {
                float angle = shift * 6.2831853; // 2 * PI
                float s = sin(angle);
                float c = cos(angle);

                float3x3 mat = float3x3(
                    0.299 + 0.701 * c + 0.168 * s, 0.587 - 0.587 * c + 0.330 * s, 0.114 - 0.114 * c - 0.497 * s,
                    0.299 - 0.299 * c - 0.328 * s, 0.587 + 0.413 * c + 0.035 * s, 0.114 - 0.114 * c + 0.292 * s,
                    0.299 - 0.300 * c + 1.250 * s, 0.587 - 0.588 * c - 1.050 * s, 0.114 + 0.886 * c - 0.203 * s
                );
                return mul(color, mat);
            }

            uint GetDigit(uint number, uint position)
            {
                if (position == 0)
                    return (number / 10) % 10;
                else
                    return number % 10;
            }

            float3 ApplyHDRExposure(float3 linearColor, float exposure)
			{
				return linearColor * pow(2, exposure);
			}

            float3 RGBtoHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
				
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

            float3 HSVtoRGB(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

            float3 ModifyViaHSV(float3 color, float h, float s, float v)
			{
				float3 colorHSV = RGBtoHSV(color);
				colorHSV.x = frac(colorHSV.x + h);
				colorHSV.y = saturate(colorHSV.y + s);
				colorHSV.z = saturate(colorHSV.z + v);
				return HSVtoRGB(colorHSV);
			}

            uint GetDigit(v2f i) {
                uint slot = floor(i.uv.x / 0.166666667);

                int digit = 0;
                switch(slot){
                    case 0:
                        return GetDigit((uint)_Hour, 0);
                    case 1:
                        return GetDigit((uint)_Hour, 1);
                    case 2:
                        return GetDigit((uint)_Minute, 0);
                    case 3:
                        return GetDigit((uint)_Minute, 1);
                    case 4:
                        return GetDigit((uint)_Second, 0);
                    case 5:
                        return GetDigit((uint)_Second, 1);
                    default:
                        return 0;
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                uint digit = GetDigit(i);

                float localX = frac(i.uv.x / 0.166666667);
                float2 uvAtlas = float2(localX / 10.0 + digit / 10.0, i.uv.y);
                fixed4 texColor = tex2D(_NumberAtlas, uvAtlas);

                clip(texColor.a - _Cutoff);

                float hueColor = frac(_HueShift + _HueSpeed * _Time.x);
                float3 color = ModifyViaHSV(_EmissionColor.rgb, hueColor, 0, 0);
                float3 exposure = ApplyHDRExposure(color, 0);
                return float4(exposure, _EmissionColor.a);
            }
            ENDCG
        }
    }
}

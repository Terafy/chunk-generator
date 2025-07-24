Shader "Custom/DamageDissolve"
{
    Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _DarkenTint("Darken Tint", Color) = (0.2, 0.2, 0.2, 1)
        _NoiseScale("Noise Scale", Float) = 1
        _DissolveSoftness("Dissolve Softness", Range(0.1, 10)) = 2
        _Ambient("Ambient Light", Range(0,1)) = 0.2
        _DarkenStrength("Darken Strength", Range(0,1)) = 0.7
        _Contrast("Contrast", Range(1,5)) = 2
        _RimPower("Rim Power", Range(0.5, 8)) = 3
        _RimStrength("Rim Strength", Range(0,1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float4 _DarkenTint;
            float _NoiseScale;
            float _DissolveSoftness;
            float _Ambient;
            float _DarkenStrength;
            float _Contrast;
            float _RimPower;
            float _RimStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float damage : TEXCOORD3;
            };

            float hash(float3 p)
            {
                p = floor(p);
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float radialNoise(float3 p, float3 center)
            {
                float3 offset = p - center;
                float dist = length(offset);
                float raw = hash(floor(p * _NoiseScale));
                return raw + dist / _DissolveSoftness;
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.damage = v.uv2.x;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = saturate(dot(normal, lightDir));
                float diff = pow(NdotL, _Contrast);
                float light = lerp(_Ambient, 1.0, diff);

                float rim = pow(1.0 - saturate(dot(normal, i.viewDir)), _RimPower);
                light += rim * _RimStrength;

                float3 baseColor = _Color.rgb * light;

                if (i.damage >= 0.999)
                    return fixed4(baseColor, 1.0);

                float damage = 1.0 - i.damage;

                float3 blockCenter = floor(i.worldPos) + 0.5;
                float n = radialNoise(i.worldPos, blockCenter);
                float damageMask = saturate((n - damage) * 10.0);

                float3 darkenColor = lerp(_DarkenTint.rgb, baseColor, 0.5);
                float3 darkened = lerp(darkenColor, baseColor, 1.0 - _DarkenStrength);
                float3 finalColor = lerp(darkened, baseColor, damageMask);

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}

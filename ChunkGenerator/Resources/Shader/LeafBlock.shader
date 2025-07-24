Shader "Custom/LeafBlock"
{
    Properties
    {
        _Color("Color", Color) = (0.3, 0.8, 0.3, 1)
        _WindStrength("Wind Strength", Range(0, 1)) = 0.1
        _WindDirection("Wind Direction", Vector) = (1, 0, 0, 0)
        _TransparencyCutoff("Transparency Cutoff", Range(0, 1)) = 0.5
        _NoiseScale("Noise Scale", Range(1, 50)) = 10
        _Ambient("Ambient Light", Range(0, 1)) = 0.2
        _Contrast("Contrast", Range(1, 5)) = 2
        _RimPower("Rim Power", Range(0.5, 8)) = 3
        _RimStrength("Rim Strength", Range(0, 1)) = 0.3
    }
    SubShader
    {
        Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
        LOD 200
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _WindStrength;
            float4 _WindDirection;
            float _TransparencyCutoff;
            float _NoiseScale;
            float _Ambient;
            float _Contrast;
            float _RimPower;
            float _RimStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
            };

            float hash(float3 p)
            {
                return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
            }

            float noise3D(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);

                float n000 = hash(i + float3(0, 0, 0));
                float n100 = hash(i + float3(1, 0, 0));
                float n010 = hash(i + float3(0, 1, 0));
                float n110 = hash(i + float3(1, 1, 0));
                float n001 = hash(i + float3(0, 0, 1));
                float n101 = hash(i + float3(1, 0, 1));
                float n011 = hash(i + float3(0, 1, 1));
                float n111 = hash(i + float3(1, 1, 1));

                float3 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(lerp(n000, n100, u.x), lerp(n010, n110, u.x), u.y),
                            lerp(lerp(n001, n101, u.x), lerp(n011, n111, u.x), u.y),
                            u.z);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float2 windDir = normalize(_WindDirection.xy);
                float wavePhase = dot(v.vertex.xy, windDir) * 5.0;
                float windOffset = sin(_Time.y * 3.0 + wavePhase) * _WindStrength;
                float3 offset = float3(windDir.x, 0, windDir.y) * windOffset;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz + offset;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);

                o.pos = UnityObjectToClipPos(v.vertex + float4(offset, 0));
                o.worldPos = worldPos;
                o.worldNormal = worldNormal;
                o.localPos = v.vertex.xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float n = noise3D(i.localPos * _NoiseScale);
                clip(n - _TransparencyCutoff);

                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = saturate(dot(normal, lightDir));
                float diff = pow(NdotL, _Contrast);

                float rim = pow(1.0 - saturate(dot(normal, i.viewDir)), _RimPower);
                float light = lerp(_Ambient, 1.0, diff) + rim * _RimStrength;

                return _Color * light;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Cutout/VertexLit"
}

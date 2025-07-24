Shader "Custom/AdvancedGradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.4, 0.6, 1, 1)
        _BottomColor ("Bottom Color", Color) = (1, 1, 1, 1)
        _Rotation ("Gradient Rotation (Degrees)", Range(0, 360)) = 0
        _Sharpness ("Sharpness", Range(0.01, 10)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off
        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Rotation;
            float _Sharpness;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                float angleRad = radians(_Rotation);
                float2 rotated = float2(
                    dir.x * cos(angleRad) - dir.z * sin(angleRad),
                    dir.x * sin(angleRad) + dir.z * cos(angleRad)
                );

                float gradientFactor = dir.y + rotated.x; // Y + rotated X (diagonal gradient)
                gradientFactor = saturate(gradientFactor * 0.5 + 0.5);
                gradientFactor = pow(gradientFactor, _Sharpness);

                return lerp(_BottomColor, _TopColor, gradientFactor);
            }
            ENDCG
        }
    }
}

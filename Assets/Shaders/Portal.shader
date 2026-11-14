Shader "Custom/Portal"
{
    Properties
    {
        [HDR] _FillColor ("Fill Color", Color) = (0.1, 0.5, 2.0, 1)
        [HDR] _EdgeColor ("Edge Color", Color) = (0.5, 2.0, 3.0, 1)
        _Intensity ("Intensity", Range(0, 5)) = 1.5
        _Opacity ("Opacity", Range(0, 1)) = 1

        _Aspect ("Aspect (width / height)", Float) = 0.5
        _EdgeWidth ("Edge Width", Range(0.01, 0.5)) = 0.08

        _FlowSpeed ("Flow Speed", Float) = 0.4
        _NoiseScale ("Noise Scale", Float) = 4
        _ScanlineCount ("Scanline Count", Float) = 20
        _ScanlineSpeed ("Scanline Speed", Float) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Portal"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One   // additivo: effetto luminoso
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _FillColor;
                half4 _EdgeColor;
                half _Intensity;
                half _Opacity;
                float _Aspect;
                float _EdgeWidth;
                float _FlowSpeed;
                float _NoiseScale;
                float _ScanlineCount;
                float _ScanlineSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;
                float2 uv = IN.uv;

                // Distanza dal bordo più vicino, in unità di altezza
                // (così lo spessore del bordo è uniforme anche su rettangoli non quadrati)
                float2 p = uv * 2.0 - 1.0;
                float distX = (1.0 - abs(p.x)) * _Aspect;
                float distY = 1.0 - abs(p.y);
                float dist = min(distX, distY);

                // Cornice luminosa
                float edge = 1.0 - smoothstep(0.0, _EdgeWidth, dist);

                // Bagliore morbido verso l'interno della cornice
                float innerGlow = 1.0 - smoothstep(0.0, _EdgeWidth * 4.0, dist);

                // Energia che scorre verso l'alto
                float2 flowUV = float2(uv.x * _Aspect, uv.y) * _NoiseScale;
                float n = valueNoise(flowUV + float2(0.0, -t * _FlowSpeed));
                n = n * 0.6 + valueNoise(flowUV * 2.0 + float2(0.0, -t * _FlowSpeed * 1.7)) * 0.4;

                // Linee orizzontali che salgono
                float scan = sin((uv.y * _ScanlineCount - t * _ScanlineSpeed * _ScanlineCount * 0.2) * 6.2831853);
                scan = pow(saturate(scan * 0.5 + 0.5), 6.0);

                half3 col = _FillColor.rgb * (0.3 + n * 0.7);
                col += _FillColor.rgb * scan * 0.5;
                col += _EdgeColor.rgb * innerGlow * 0.4;
                col += _EdgeColor.rgb * edge * 2.0;
                col *= _Intensity;

                return half4(col, _Opacity);
            }
            ENDHLSL
        }
    }
}

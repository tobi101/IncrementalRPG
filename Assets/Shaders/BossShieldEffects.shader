Shader "RPG/Boss Shield Effects"
{
    Properties
    {
        [Toggle(_TOTEM_LINK)] _TotemLink ("Totem Link", Float) = 0
        _Pattern ("Arc or Cell Texture", 2D) = "white" {}
        _Distortion ("Distortion Texture", 2D) = "gray" {}
        _Tint ("Glow", Color) = (1, 0.46, 0.12, 1)
        _Core ("Highlights", Color) = (1, 0.9, 0.66, 1)
        [HideInInspector] _FlowTime ("Flow Time", Float) = 0
        [HideInInspector] _HitFlash ("Hit Flash", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
            Cull Off
            ZWrite Off
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local_fragment _ _TOTEM_LINK
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Pattern);
            SAMPLER(sampler_Pattern);
            TEXTURE2D(_Distortion);
            SAMPLER(sampler_Distortion);

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half4 _Core;
                float _FlowTime;
                float _HitFlash;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _FlowTime;

                #if defined(_TOTEM_LINK)
                    float crosswise = uv.y - 0.5;
                    float drift = sin(uv.x * 18.0 - t * 5.0) * 0.06 * sin(PI * uv.x);
                    float pulse = 0.45 + 0.55 * pow(saturate(cos(uv.x * 20.0 - t * 7.0)), 4.0);
                    float arc = SAMPLE_TEXTURE2D(_Pattern, sampler_Pattern,
                        float2(frac(uv.x * 1.65 - t * 0.8), 0.5 + (crosswise - drift) * 1.12)).r;
                    float thread = exp2(-abs(crosswise - drift) * 25.0);
                    float glow = exp2(-abs(crosswise) * 11.0);
                    float taper = smoothstep(0.0, 0.045, min(uv.x, 1.0 - uv.x));
                    float terminal = exp2(-min(uv.x, 1.0 - uv.x) * 90.0 - crosswise * crosswise * 72.0);
                    float opacity = saturate(taper * (arc * (0.53 + pulse * 0.27) + thread * 0.32 + glow * 0.21)
                        + terminal * 0.44);
                    float highlight = saturate(arc * (0.65 + pulse * 0.35) + thread * 0.25 + terminal * 0.45);
                #else
                    float2 offset = (uv - 0.5) * 2.35;
                    float2 flow = SAMPLE_TEXTURE2D(_Distortion, sampler_Distortion,
                        uv * 1.55 + float2(t * 0.07, -t * 0.05)).rg - 0.5;
                    float radialDistance = length(offset + flow * 0.06);
                    float rimDistance = abs(radialDistance - 1.0);
                    float edge = exp2(-rimDistance * 59.0);
                    float halo = exp2(-rimDistance * 21.0);
                    float surface = 1.0 - smoothstep(0.97, 1.015, radialDistance);
                    float cells = SAMPLE_TEXTURE2D(_Pattern, sampler_Pattern, uv + flow * 0.035).r;
                    float veins = smoothstep(0.53, 0.82, cells);
                    float ripple = pow(saturate(cos(radialDistance * 31.0 - t * 4.0 + flow.x * 5.0)), 5.0);
                    float sweep = smoothstep(-0.5, 0.95, offset.y - offset.x * 0.6 + flow.y);
                    float flash = _HitFlash * _HitFlash;
                    float opacity = saturate(edge * (0.35 + sweep * 0.27 + flash * 0.33) + halo * (0.21 + flash * 0.14)
                        + surface * (0.026 + veins * (0.10 + 0.17 * radialDistance + flash * 0.29) + ripple * (0.025 + flash * 0.15)));
                    float highlight = saturate(edge * (0.42 + sweep * 0.38) + veins * 0.18 + flash * 0.7);
                #endif

                return half4(lerp(_Tint.rgb, _Core.rgb, highlight) * opacity, opacity);
            }
            ENDHLSL
        }
    }
}

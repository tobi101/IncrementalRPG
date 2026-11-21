Shader "IncrementalRPG/Totem Spawn Burn"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Spine Atlas", 2D) = "white" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput ("Straight Alpha Texture", Int) = 1
        _FadeTex ("Dissolve Pattern", 2D) = "white" {}
        _FadeBurnTex ("Flame Texture", 2D) = "white" {}
        _FadeBurnColor ("Flame Tint", Color) = (0.52, 0.12, 1, 1)
        _FadeBurnGlow ("Flame Glow", Float) = 4
        _FadeBurnWidth ("Burn Width", Float) = 0.071
        _FadeBurnTransition ("Burn Softness", Float) = 0.097
        [PerRendererData] _FadeAmount ("Fade Amount", Range(-0.1, 1)) = -0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _STRAIGHT_ALPHA_INPUT
            #include "UnityCG.cginc"
            #include "Packages/com.esotericsoftware.spine.spine-unity/Runtime/spine-unity/Shaders/CGIncludes/Spine-Common.cginc"

            sampler2D _MainTex, _FadeTex, _FadeBurnTex;
            float4 _FadeTex_ST, _FadeBurnTex_ST;
            half4 _FadeBurnColor;
            half _FadeAmount, _FadeBurnWidth, _FadeBurnTransition, _FadeBurnGlow;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 atlasUV : TEXCOORD0;
                float2 localXY : TEXCOORD1;
                half4 color : COLOR;
            };

            Varyings vert(appdata_full v)
            {
                Varyings o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.atlasUV = v.texcoord.xy;
                // Use one continuous burn pattern across all the Spine attachments.
                o.localXY = v.vertex.xy;
                o.color = PMAGammaToTargetSpace(v.color);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 body = tex2D(_MainTex, i.atlasUV);
                #if defined(_STRAIGHT_ALPHA_INPUT)
                    body.rgb *= body.a;
                #endif
                body *= i.color;
                if (_FadeAmount <= -0.1h) return body;
                if (_FadeAmount >= 1.0h) return 0;

                half noise = tex2D(_FadeTex, TRANSFORM_TEX(i.localXY, _FadeTex)).r;
                half visible = smoothstep(_FadeAmount, _FadeAmount + _FadeBurnTransition, noise);
                half edge = smoothstep(_FadeAmount - _FadeBurnWidth,
                    _FadeAmount - _FadeBurnWidth + 0.1h, noise) * (1 - visible) * saturate(_FadeAmount);
                half heat = tex2D(_FadeBurnTex, TRANSFORM_TEX(i.localXY, _FadeBurnTex)).r;
                half fireAlpha = edge * _FadeBurnColor.a * body.a;
                return body * visible + half4(_FadeBurnColor.rgb * heat * _FadeBurnGlow, 1) * fireAlpha;
            }
            ENDCG
        }
    }
}

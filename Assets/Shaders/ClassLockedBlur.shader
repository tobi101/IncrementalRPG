Shader "UI/ClassLockedBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurRadius ("Blur radius", Range(0,12)) = 6
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 world:TEXCOORD1; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _ClipRect;
            fixed4 _Color, _TextureSampleAdd;
            float _BlurRadius;
            v2f vert(appdata v)
            {
                v2f o; o.world=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; return o;
            }
            float4 SamplePremultiplied(float2 uv)
            {
                float4 sample = tex2D(_MainTex, saturate(uv)) + _TextureSampleAdd;
                sample.rgb *= sample.a;
                return sample;
            }
            float4 frag(v2f i):SV_Target
            {
                float2 d=_MainTex_TexelSize.xy*_BlurRadius;
                float4 c=SamplePremultiplied(i.uv)*4;
                c+=SamplePremultiplied(i.uv+float2(d.x,0))*2+SamplePremultiplied(i.uv-float2(d.x,0))*2;
                c+=SamplePremultiplied(i.uv+float2(0,d.y))*2+SamplePremultiplied(i.uv-float2(0,d.y))*2;
                c+=SamplePremultiplied(i.uv+d)+SamplePremultiplied(i.uv-d);
                c+=SamplePremultiplied(i.uv+float2(d.x,-d.y))+SamplePremultiplied(i.uv+float2(-d.x,d.y));
                c/=16;
                c.rgb/=max(c.a,0.0001);
                c*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(i.world.xy,_ClipRect);
                #endif
                return c;
            }
            ENDCG
        }
    }
}

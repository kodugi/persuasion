Shader "UI/Persuasion Glitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0
        _BlockCount ("Horizontal Block Count", Float) = 90
        _GlitchSpeed ("Glitch Speed", Float) = 24
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.05)) = 0.012

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _MainTex_ST;
            float4 _ClipRect;
            float _GlitchAmount;
            float _BlockCount;
            float _GlitchSpeed;
            float _ChromaticAberration;

            v2f vert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(output.worldPosition);
                output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            fixed4 SampleSprite(float2 uv)
            {
                return tex2D(_MainTex, uv) + _TextureSampleAdd;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float timeStep = floor(_Time.y * _GlitchSpeed);
                float horizontalBlock = floor(input.texcoord.y * max(1.0, _BlockCount));
                float noise = frac(sin(horizontalBlock * 12.9898 + timeStep * 78.233) * 43758.5453);
                float activeBlock = step(0.55, noise);
                float horizontalOffset = (noise - 0.5) * 0.07 * _GlitchAmount * activeBlock;
                float chromaticOffset = _ChromaticAberration * _GlitchAmount * activeBlock;

                float2 centerUv = input.texcoord + float2(horizontalOffset, 0.0);
                fixed4 center = SampleSprite(centerUv);
                fixed red = SampleSprite(centerUv + float2(chromaticOffset, 0.0)).r;
                fixed blue = SampleSprite(centerUv - float2(chromaticOffset, 0.0)).b;

                fixed4 color = fixed4(red, center.g, blue, center.a) * input.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}

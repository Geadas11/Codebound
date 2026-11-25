// Round soft particle for the menu stage (sparks, steam, embers, dust). Colour comes from the particle; _DstBlend picks
// additive (1 = One) or alpha (10 = OneMinusSrcAlpha) blending.
Shader "Codebound/SoftParticle"
{
    Properties { [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 10 }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha [_DstBlend]
        ZWrite Off Cull Off
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 pos : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct V { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            V vert(A i) { V o; o.pos = TransformObjectToHClip(i.pos.xyz); o.color = i.color; o.uv = i.uv * 2 - 1; return o; }
            half4 frag(V i) : SV_Target
            {
                float r = saturate(1 - dot(i.uv, i.uv));
                return half4(i.color.rgb, i.color.a * r * r);
            }
            ENDHLSL
        }
    }
}

Shader "Custom/URP_TransparentShadowCollector"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.6
        _ShadowContrast ("Shadow Contrast", Range(0.1, 5.0)) = 2.0
    }

    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _ShadowIntensity;
                half _ShadowContrast;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // Sample main light & shadow
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half shadowAtten = mainLight.shadowAttenuation;

                // Apply contrast curve to make shadows darker
                half contrast = saturate(pow(1.0 - shadowAtten, _ShadowContrast));

                // Convert to alpha darkness
                half shadowAlpha = contrast * _ShadowIntensity;

                half4 color = _Color;
                color.a = saturate(shadowAlpha); // force valid range

                return color;
            }
            ENDHLSL
        }
    }
    FallBack Off
}

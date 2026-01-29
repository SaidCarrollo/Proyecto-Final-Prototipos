Shader "Custom/ToonMobileLit_AutoShadow"
{
    Properties
    {
        [Header(Base Settings)]
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1,1,1,1)
        
        [Header(Toon Lighting)]
        _RampThreshold ("Ramp Threshold", Range(-1,1)) = 0.5
        _RampSmoothness ("Ramp Smoothness", Range(0.001, 1)) = 0.05
        
        // CAMBIO PRINCIPAL: En lugar de un color fijo, usamos una fuerza (multiplicador)
        _ShadowStrength ("Shadow Strength (Darkness)", Range(0, 1)) = 0.4
        _ShadowTint ("Extra Shadow Tint (Optional)", Color) = (1,1,1,1) 
        _Brightness ("Brightness Boost", Range(1, 5)) = 1.0

        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.003
        
        [Header(System)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Geometry" 
        }

        // --- PASS 1: OUTLINE ---
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ColorMask RGB
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input) {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                float3 normalCS = TransformWorldToHClipDir(TransformObjectToWorldNormal(input.normalOS));
                output.positionCS = vertexInput.positionCS;
                float2 offset = normalize(normalCS.xy) * _OutlineWidth * output.positionCS.w;
                output.positionCS.xy += offset;
                return output;
            }
            half4 frag(Varyings input) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }

        // --- PASS 2: LIGHTING ---
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2); 
                float3 positionWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _RampThreshold;
                half _RampSmoothness;
                // Nuevas variables
                half _ShadowStrength;
                half4 _ShadowTint; 
                half _Brightness;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            // Función Toon Actualizada: Usa un factor de sombra (0 a 1) en lugar de un color
            half3 CalculateToonLight(Light light, float3 normalWS, half shadowStrength, half threshold, half smoothness)
            {
                float NdotL = dot(normalWS, light.direction);
                float ramp = smoothstep(threshold - smoothness, threshold + smoothness, NdotL);
                float attenuation = light.distanceAttenuation * light.shadowAttenuation;
                
                // Aquí está la magia:
                // Si hay luz (ramp 1), factor es 1.0.
                // Si hay sombra (ramp 0), factor es _ShadowStrength (ej. 0.4).
                half lightFactor = lerp(shadowStrength, 1.0, ramp * attenuation);
                
                // Devolvemos el COLOR de la luz multiplicado por el factor de oscuridad.
                return light.color * lightFactor;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);
                
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normalWS = normalize(input.normalWS);

                // 1. Luz Principal
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
                    Light mainLight = GetMainLight(shadowCoord);
                #else
                    Light mainLight = GetMainLight();
                #endif

                // Calculamos cuánta luz recibe (no el color final, sino la intensidad toon)
                half3 lightIntensity = CalculateToonLight(mainLight, normalWS, _ShadowStrength, _RampThreshold, _RampSmoothness);

                // 2. Luces Adicionales
                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint i = 0; i < pixelLightCount; ++i)
                    {
                        Light light = GetAdditionalLight(i, input.positionWS); 
                        // Para luces extra, la sombra es oscuridad total (0), si no, iluminarían todo el cuarto.
                        lightIntensity += CalculateToonLight(light, normalWS, 0.0, _RampThreshold, _RampSmoothness);
                    }
                #endif

                // 3. Baked / Ambiental (IMPORTANTE PARA REALTIME)
                half3 bakedColor;
                #ifdef LIGHTMAP_ON
                    bakedColor = SampleLightmap(input.lightmapUV, normalWS);
                #else
                    // Si NO hay lightmap, usa el color del ambiente (Skybox/Gradient)
                    // Esto arregla que se vea negro en Realtime sin bakeo
                    bakedColor = SampleSH(normalWS); 
                #endif
                
                // Toonificar Baked
                half bakedLuminance = Luminance(bakedColor);
                half bakedRamp = smoothstep(0.1, 0.4, bakedLuminance);
                
                // Aplicamos la fuerza de sombra también a la luz ambiental
                half3 finalBaked = lerp(bakedColor * _ShadowStrength, bakedColor, bakedRamp);

                // 4. MEZCLA FINAL AUTOMÁTICA
                // ColorTextura * (LuzCalculada + LuzAmbiental) * TinteOpcional
                half3 finalColor = albedo.rgb * (lightIntensity + finalBaked) * _ShadowTint.rgb * _Brightness;

                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
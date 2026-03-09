Shader "Custom/WaypointHologram"
{
    Properties
    {
        [Header(Color Settings)]
        [HDR] _MainColor ("Hologram Color", Color) = (0, 1, 1, 0.5) 
        
        [Header(Hologram Effect)]
        _FresnelPower ("Fresnel Power (Bordes brillantes)", Range(0.1, 5.0)) = 2.0
        _ScanSpeed ("Scanline Speed", Range(-10, 10)) = 3.0
        _ScanAmount ("Scanline Density", Range(0, 50)) = 20.0
        
        [Header(Vertical Gradient)]
        //[Tooltip("Altura local inferior donde la opacidad es 100%")]
        _FadeBottom ("Bottom Y (Opaco)", Float) = -1.0
        //[Tooltip("Altura local superior donde la opacidad llega a 0%")]
        _FadeTop ("Top Y (Transparente)", Float) = 1.0

        _MinimumAlpha ("Minimum Opacity", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Transparent" 
        }

        Pass
        {
            Name "HologramUnlit"
            Tags { "LightMode" = "UniversalForward" }

            // Configuración para Transparencia (Estilo Aditivo/Alpha)
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
                float  localY     : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _MainColor;
                half  _FresnelPower;
                half  _ScanSpeed;
                half  _ScanAmount;
                float _FadeBottom;
                float _FadeTop;
                half  _MinimumAlpha;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Posiciones y direcciones
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                // Normales para el efecto de borde (Fresnel)
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                
                // Guardamos la altura local (Y) para hacer el gradiente y el escáner
                output.localY = input.positionOS.y;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Efecto Fresnel (Brilla más en los bordes, transparente en el centro)
                float NdotV = dot(normalize(input.normalWS), normalize(input.viewDirWS));
                float fresnel = pow(1.0 - saturate(NdotV), _FresnelPower);

                // 2. Líneas de escaneo animadas (Scanlines)
                float scanlines = sin(input.localY * _ScanAmount - _Time.y * _ScanSpeed);
                scanlines = scanlines * 0.5 + 0.5;

                // --- NUEVO: Gradiente Vertical ---
                // Convertimos la altura local a un valor entre 0 y 1
                float gradientT = saturate((input.localY - _FadeBottom) / (_FadeTop - _FadeBottom));
                // Invertimos: Queremos que Abajo (0) sea Opaco (1), y Arriba (1) sea Transparente (0)
                float verticalFade = lerp(1.0, 0.0, gradientT);

                // 3. Mezclamos todo
                // Multiplicamos el alpha resultante por nuestro nuevo 'verticalFade'
                half alpha = _MainColor.a * (fresnel + (scanlines * 0.3)) * verticalFade;
                
                // Aplicamos el mínimo permitido
                alpha = max(alpha, _MinimumAlpha);

                // Devolvemos el color
                return half4(_MainColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
#ifndef INPUT_LUXLWRP_BASE_INCLUDED
#define INPUT_LUXLWRP_BASE_INCLUDED

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//  defines a bunch of helper functions (like lerpwhiteto)
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"  
//  defines SurfaceData, textures and the functions Alpha, SampleAlbedoAlpha, SampleNormal, SampleEmission
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
//  defines e.g. "DECLARE_LIGHTMAP_OR_SH"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 
    #include "ATG URP Translucent Lighting.hlsl"


    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

//  Material Inputs
    CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half _Cutoff;
        half _Smoothness;
        half3 _SpecColor;
        half4 _WindMultiplier;

        float _SampleSize;

        half _TranslucencyPower;
        half _TranslucencyStrength;
        half _ShadowStrength;
        half _Distortion;

        half2 _MinMaxScales;
        half4 _HealthyColor;
        half4 _DryColor;  

    CBUFFER_END

//  Additional textures
    TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
    TEXTURE2D(_BumpSpecMap); SAMPLER(sampler_BumpSpecMap); float4 _BumpSpecMap_TexelSize;
    TEXTURE2D(_LuxLWRPWindRT); SAMPLER(sampler_LuxLWRPWindRT);

//  Global Inputs
    TEXTURE2D(_AtgWindRT); SAMPLER(sampler_AtgWindRT);
    
    CBUFFER_START(AtgGrass)
        float4 _AtgTerrainShiftSurface;
        float4 _AtgWindDirSize;
        float4 _AtgWindStrengthMultipliers;
        float4 _AtgSinTime;
        float4 _AtgGrassFadeProps;
        float4 _AtgGrassShadowFadeProps;
        float3 _AtgSurfaceCameraPosition;
    CBUFFER_END

//  Shared Variables
    float InstanceScale;
    float TextureLayer;
    #if defined(_NORMAL)
        half3 terrainNormal;
    #endif

//  Structs
    struct VertexInput
    {
        float3 positionOS                   : POSITION;
        float3 normalOS                     : NORMAL;
        float2 texcoord                     : TEXCOORD0;
        #if !defined(UNITY_PASS_SHADOWCASTER) && !defined(DEPTHONLYPASS) && !defined(DEPTHNORMALPASS)
            float4 tangentOS                : TANGENT;
            float2 lightmapUV               : TEXCOORD1;
        #endif
        half4 color                         : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct VertexOutput
    {
        float4 positionCS                   : SV_POSITION;
        float2 uv                           : TEXCOORD0;

        #if !defined(UNITY_PASS_SHADOWCASTER) && !defined(DEPTHONLYPASS)
            DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                float3 positionWS           : TEXCOORD2;
            #endif
            half3 normalWS                  : TEXCOORD3;
            #ifdef _NORMALMAP
                half4 tangentWS             : TEXCOORD4;
            #endif
            half4 fogFactorAndVertexLight   : TEXCOORD6;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord          : TEXCOORD7;
            #endif
            half4 color                     : COLOR;
        #endif
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    struct SurfaceDescription
    {
        float3 albedo;
        float alpha;
        float3 normalTS;
        float3 emission;
        float metallic;
        float3 specular;
        float smoothness;
        float occlusion;
        float translucency;
    };


    half4 SmoothCurve( half4 x ) {   
    return x * x *( 3.0h - 2.0h * x );   
    }
    half4 TriangleWave( half4 x ) {   
        return abs( frac( x + 0.5h ) * 2.0h - 1.0h );   
    }
    half4 SmoothTriangleWave( half4 x ) {   
        return SmoothCurve( TriangleWave( x ) );   
    }

    half2 SmoothCurve( half2 x ) {   
    return x * x *( 3.0h - 2.0h * x );   
    }
    half2 TriangleWave( half2 x ) {   
        return abs( frac( x + 0.5h ) * 2.0h - 1.0h );   
    }
    half2 SmoothTriangleWave( half2 x ) {   
        return SmoothCurve( TriangleWave( x ) );   
    }


    #define foliageMainWindStrengthFromZone _AtgWindStrengthMultipliers.y
    #define primaryBending _WindMultiplier.x
    #define secondaryBending _WindMultiplier.y
    #define edgeFlutter _WindMultiplier.z

    void animateVertex(half4 animParams, inout half3 normalOS, inout float3 positionOS, inout half4 instanceColor) {

        float scale = InstanceScale;
        const float3 pivot = UNITY_MATRIX_M._m03_m13_m23;
        float3 dist = pivot
                      //+ scale.xxx * 4 /* lets break up the boring distance*/ 
                      #if defined(DONOTUSE_ATGSETUP)
                            - _WorldSpaceCameraPos.xyz;         // vs shader version
                      #elif !defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                            - _WorldSpaceCameraPos.xyz;         // for wind setup
                      #else
                            - _AtgSurfaceCameraPosition.xyz;    // atg original shader: we have to use a custom cam pos to make it match compute.
                      #endif
        float SqrDist = dot(dist, dist);
    //  Calculate far fade factor
        #if defined (ISSHADOWPASS)
            //float fade = saturate((_AtgGrassShadowFadeProps.z - SqrDist) * _AtgGrassShadowFadeProps.w);
            float fade = 1.0f - saturate((SqrDist - _AtgGrassShadowFadeProps.z) * _AtgGrassShadowFadeProps.w);
        #elif defined (ISDEPTHPASS)
            float fade = saturate((_AtgGrassFadeProps.x - SqrDist) * _AtgGrassFadeProps.y);
        #else
            float fade = saturate(( _AtgGrassFadeProps.x - SqrDist) * _AtgGrassFadeProps.y);
        #endif
    //  Cull based on far culling distance
        if (fade == 0.0f) {
            positionOS.xyz = 0;
            return;
        } 
    //  Apply fading
        positionOS.xyz = lerp(positionOS.xyz, float3(0,0,0), 1.0 - fade);
    //  Instance Color
        instanceColor = lerp(_HealthyColor, _DryColor, (scale - _MinMaxScales.x) * _MinMaxScales.y);

    //  Wind
        float origLength = length(positionOS.xyz);
    //  NOTE: minus to make windDir match?!
        half3 windDir = -mul(UNITY_MATRIX_I_M, float4(_AtgWindDirSize.xyz, 0)).xyz;

        const half fDetailAmp = 0.1h;
        const half fBranchAmp = 0.3h;

    #if !defined(_WIND_MATH)
        float2 samplePos = ( TransformObjectToWorld(positionOS.xyz * _SampleSize).xz - instanceColor.a * windDir.xz)  * _AtgWindDirSize.ww         + scale * 0.025;
        
        half fVtxPhase = dot( normalize(positionOS.xyz), ((animParams.g + animParams.r) * 0.5).xxx );
        
        half4 wind = SAMPLE_TEXTURE2D_LOD(_AtgWindRT, sampler_AtgWindRT, samplePos.xy, _WindMultiplier.w);
        wind.r = wind.r * (wind.g * 2.0f - 0.24376f  /* not a "real" normal as we want to keep the base direction*/  );
        wind.r *= _AtgWindStrengthMultipliers.y * instanceColor.a;

    //  Factor in bending params from Material
        animParams.abg *= _WindMultiplier.xyz;
    //  Make math match
        //animParams.a *= 0.3;

        float3 offset = 0;
    //  Primary bending
        offset = animParams.a * windDir * foliageMainWindStrengthFromZone * wind.r; // * smoothstep(-1.5h, 1.0h, wind.r * (wind.g * 1.0h - 0.243h));

    //  Second texture sample taking phase into account
        wind = SAMPLE_TEXTURE2D_LOD(_AtgWindRT, sampler_AtgWindRT, samplePos.xy - animParams.rr * 0.5, _WindMultiplier.w);

    //  Edge Flutter
        //half3 bend = normalOS.xyz * (animParams.g * fDetailAmp * lerp(_LuxLWRPSinTime.y, _LuxLWRPSinTime.z, wind.r));
        float3 bend = animParams.g * fDetailAmp * normalOS.xyz;
    //  Edge Flutter and Secondary Bending
        offset += (bend + ( animParams.b * fBranchAmp * windDir * (wind.g * 2.0h - 0.243h) )) * wind.r; 

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            positionOS += offset;
            normalOS.xz += offset.xz * PI; // * _NormalBend
        #else
            positionOS -= offset;
            normalOS.xz -= offset.xz * PI; // * _NormalBend
        #endif
        
// ATG bending
    #else

        float2 samplePos = ( TransformObjectToWorld(positionOS.xyz * _SampleSize).xz - instanceColor.a * windDir.xz)  * _AtgWindDirSize.ww         + scale * 0.025;
        half4 wind = SAMPLE_TEXTURE2D_LOD(_AtgWindRT, sampler_AtgWindRT, samplePos.xy, _WindMultiplier.w);
        wind.r = wind.r * (wind.g * 2.0f - 0.24376f  /* not a "real" normal as we want to keep the base direction*/  );
        wind.r *= _AtgWindStrengthMultipliers.y * instanceColor.a;

        half2 variations = abs(frac( float2(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].y)));
        half fObjPhase = dot(variations, half2(1,1) );
    //  Factor in bending params from Material
        animParams.abg *= _WindMultiplier.xyz;

        float3 offset = 0;
    //  Primary bending
        offset = animParams.a * windDir * wind.r;
        half2 vWavesIn = _Time.yy + float2(0, fObjPhase  +  (animParams.r + instanceColor.a) );
        half4 vWaves = (frac( vWavesIn.xxyy * half4(1.975h, 0.793h, 0.375h, 0.193h) ) * 2.0h - 1.0h);
        vWaves = SmoothTriangleWave( vWaves );
        half2 vWavesSum = vWaves.xz + vWaves.yw;
    //  Edge Flutter
        float3 bend = animParams.g * fDetailAmp * normalOS.xyz;
    //  Secondary bending
        offset += ((vWavesSum.xyx * bend) + (animParams.b * fBranchAmp * windDir * vWavesSum.y)) * wind.r;
        
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            positionOS += offset;
            normalOS.xz += offset.xz * PI; // * _NormalBend
        #else
            positionOS -= offset;
            normalOS.xz -= offset.xz * PI; // * _NormalBend
        #endif


    #endif

    //  Preserve length
        positionOS.xyz = normalize(positionOS.xyz) * origLength;

    }


#endif
#ifndef TOON_LIGHTING_INCLUDED
#define TOON_LIGHTING_INCLUDED
//Data Structures------------------------
    struct ToonLightingData
    {
        float3 albedo;
        float lightingSteps;
        float3 worldSpacePosition;
        float3 worldSpaceNormals;
        float3 worldSpaceViewDirection;
        float3 specularTint;
        float smoothness;
        float specularLobeMinimum;
        float4 shadowCoord;
        float ambientOcclusion;
        float3 bakedGI;
    };
//Helper Functions------------------------
    float4 Quantize(float4 incoming, float4 steps)
    {
        return floor(incoming / (1 / steps)) * (1 / steps);
    }
    float Quantize01(float incoming, float steps)
    {
        return floor(incoming * (steps - 1) + 0.5)/ (steps / 1);
    }
    //Converts the 0 to 1 smoothness value to a nice exponent
    float GetSmoothnessPower(float rawSmoothness)
    {
        return exp2(10 * rawSmoothness + 1);
    }
//Light Handling------------------------
    #ifndef SHADERGRAPH_PREVIEW
        float3 ToonLightHandling(ToonLightingData lightingData, Light lightToCalculateFor)
        {
            float3 radiance = lightToCalculateFor.color * (lightToCalculateFor.shadowAttenuation * lightToCalculateFor.distanceAttenuation);
            float diffuse = Quantize(saturate(dot(lightingData.worldSpaceNormals, lightToCalculateFor.direction)), lightingData.lightingSteps).x;
            float specularDot = saturate(dot(lightingData.worldSpaceNormals, normalize(lightToCalculateFor.direction + lightingData.worldSpaceViewDirection)));
            float specular = step(lightingData.specularLobeMinimum, pow(specularDot, GetSmoothnessPower(lightingData.smoothness))) * (diffuse);

            //Lambertian Diffuse with specular tint color
            float3 baseDiffuse = lightingData.albedo * radiance * diffuse;

            //Blinn Phong and specular tint
            float3 color = baseDiffuse * ((lightingData.specularTint * specular) + diffuse);
            return color;
        }

        float3 ToonGlobalIllumination(ToonLightingData lightingData)
        {
            float3 indirectDiffuse = Quantize(float4(lightingData.albedo * lightingData.bakedGI * lightingData.ambientOcclusion, 1), 90).xyz;
            float3 reflectVector = reflect(-lightingData.worldSpaceViewDirection, lightingData.worldSpaceNormals);
            float fresnel = Pow4(1 - saturate(dot(lightingData.worldSpaceViewDirection, lightingData. worldSpaceNormals)));
            float3 indirectSpecular = Quantize(float4(GlossyEnvironmentReflection(reflectVector, RoughnessToPerceptualRoughness(1 - lightingData.smoothness), lightingData.ambientOcclusion) * fresnel, 1), 10) * lightingData.specularTint;
            return indirectDiffuse;
        }
    #endif
        float3 CalculateToonLighting(ToonLightingData incomingLightingData)
        {
            float3 finalColorAfterLighting = 0;
            //Shader graph preview doesnt like lights so fake one
            #ifdef SHADERGRAPH_PREVIEW
                float3 lightDirection = float3(0.5, 0.5, 0);
                float intensity =  Quantize(saturate(dot(incomingLightingData.worldSpaceNormals, lightDirection)), incomingLightingData.lightingSteps).x;
                float specularDot = saturate(dot(incomingLightingData.worldSpaceNormals, normalize(lightDirection + incomingLightingData.worldSpaceViewDirection)));
                float specular = step(incomingLightingData.specularLobeMinimum, pow(specularDot, GetSmoothnessPower(incomingLightingData.smoothness))) * (intensity);
                finalColorAfterLighting += (intensity * incomingLightingData.albedo) * ((incomingLightingData.specularTint * specular) + intensity);
            #else
                finalColorAfterLighting = ToonGlobalIllumination(incomingLightingData);
                Light mainLight = GetMainLight(incomingLightingData.shadowCoord, incomingLightingData.worldSpacePosition, 1);
                MixRealtimeAndBakedGI(mainLight, incomingLightingData.worldSpaceNormals, incomingLightingData.bakedGI);
                finalColorAfterLighting += ToonLightHandling(incomingLightingData, mainLight);
                #ifdef _ADDITIONAL_LIGHTS
                    uint additionalLightCount = GetAdditionalLightsCount();
                    //Handles the strange case of forward plus rendering
                    #if USE_FORWARD_PLUS
                        InputData inputData = (InputData)0;
                        inputData.positionWS = incomingLightingData.worldSpacePosition;
                        inputData.normalWS = incomingLightingData.worldSpaceNormals;
                        inputData.viewDirectionWS = incomingLightingData.worldSpaceViewDirection;
                        inputData.shadowCoord = incomingLightingData.shadowCoord;
                        LIGHT_LOOP_BEGIN(additionalLightCount);
                            Light lightForShading = GetAdditionalLight(lightIndex, incomingLightingData.worldSpacePosition);
                            finalColorAfterLighting += ToonLightHandling(incomingLightingData, lightForShading);
                        LIGHT_LOOP_END
                    #else
                        for(uint i = 0; i < additionalLightCount; i++)
                        {
                            Light lightForShading = GetAdditionalLight(i, incomingLightingData.worldSpacePosition);
                            finalColorAfterLighting += ToonLightHandling(incomingLightingData, lightForShading);
                        }
                    #endif
                #endif
            #endif
            return finalColorAfterLighting;
        }
        //Wrapper for shader graph
        void CalculateToonLighting_float
        (
            float3 albedo,
            float3 worldSpacePosition,
            float3 worldSpaceNormals,
            float lightingSteps,
            float3 worldSpaceViewDirection,
            float3 specularTint,
            float smoothness,
            float specularLobeMinimum,
            float ambientOcclusion,
            float2 lightmapUV,
            out float3 outputColor
        )
        {
            ToonLightingData finalLightingData;
            finalLightingData.albedo = albedo;
            finalLightingData.worldSpacePosition = worldSpacePosition;
            finalLightingData.lightingSteps = lightingSteps;
            finalLightingData.specularTint = specularTint;
            finalLightingData.smoothness = smoothness;
            finalLightingData.worldSpaceNormals = worldSpaceNormals;
            finalLightingData.worldSpaceViewDirection = worldSpaceViewDirection;
            finalLightingData.specularLobeMinimum = specularLobeMinimum;
            finalLightingData.ambientOcclusion = ambientOcclusion;

            #ifdef SHADERGRAPH_PREVIEW
                finalLightingData.shadowCoord = 0;
                finalLightingData.bakedGI = 0;
            #else
                //Shadow coord handling
                float4 positionCS = TransformWorldToHClip(worldSpacePosition);
                #if SHADOWS_SCREEN
                    finalLightingData.shadowCoord = ComputeScreenPos(positionCS);
                #else
                    finalLightingData.shadowCoord = TransformWorldToShadowCoord(worldSpacePosition);
                #endif
                //Baked GI info----------
                    //Lightmap UVs
                    float3 finalLightmapUvs;
                    OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, finalLightmapUvs);
                    //Spherical Harmonics
                    float3 vertexSH;
                    OUTPUT_SH(worldSpaceNormals, vertexSH);
                    //Final baked GI
                    finalLightingData.bakedGI = SAMPLE_GI(lightmapUV, vertexSH, worldSpaceNormals);
                //-----------------------
            #endif
            outputColor = CalculateToonLighting(finalLightingData);
        }

    #endif
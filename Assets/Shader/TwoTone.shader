// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture
Shader "Unlit/TwoTone" {
    Properties{
        [Header(Colors)] 
        [Toggle(DITHER)] _Dither("Dither", Float) = 0
        _DitherTex("Dither Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (1,1,1,1)

        //chaos
        [Header(Chaos)]
        [Toggle(CHAOTIC)] _Chaotic("Chaotic", Float) = 0
        [KeywordEnum(Elastic, Smooth)] _NoiseType("Noise Type", Float) = 0
        _Chaos("Chaos", Float) = 0
        _ChaosSnap("Chaos Snap", Float) = 0

        //proximity clip
        [Header(Proximity Clip)]
        [Toggle(PROXIMITY)] _Proximity("Proximity Clip", Float) = 0
        _ProximityDistance("Proximity Clip Distance", Float) = 0
        _ProximityNoise("Proximity Clip Noise", Float) = 0

        //inflation
        [Header(Inflation)]
        [Toggle(INFLATION)] _Inflation("Inflation", Float) = 0
        _InflationAmount("Inflation Amount", Float) = 0

        //quantisation
        [Header(Quantisation)]
        [Toggle(QUANTISATION)] _Quantisation("Quantisation", Float) = 0
        _QuantisationRes("Quantisation Resolution", Float) = 1

        //culling
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 2
    }

        SubShader{

            Cull[_CullMode] //set cull

            Tags 
            {
                "RenderType" = "Opaque" 
                "LightMode" = "ForwardBase"
                "PassFlags" = "OnlyDirectional"
            }
            LOD 100

            Pass {
                CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0
                    
                    #pragma shader_feature DITHER
                    #pragma shader_feature CHAOTIC
                    #pragma multi_compile _NOISETYPE_ELASTIC _NOISETYPE_SMOOTH
                    #pragma shader_feature PROXIMITY
                    #pragma shader_feature INFLATION
                    #pragma shader_feature QUANTISATION

                    #pragma multi_compile_fwdbase
                    #include "UnityCG.cginc"
                    #include "AutoLight.cginc"
                    #include "Easing.cginc"
                    #include "Dithering.cginc"

                    static const float SMOOTHSTEP = 1;

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float3 normal : NORMAL;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct v2f {
                        float4 pos : SV_POSITION;
                        float3 worldNormal : NORMAL;
                        float3 viewDir : TEXCOORD1;
                    #ifdef PROXIMITY
                        float4 worldPos : TEXCOORD2;
                    #endif
                    #ifdef DITHER
                        float4 screenPos : TEXCOORD3;
                    #endif
                        UNITY_FOG_COORDS(0)
                        UNITY_VERTEX_OUTPUT_STEREO
                        SHADOW_COORDS(4)
                    };

                    fixed4 _Color;
                    fixed4 _ShadowColor;

                #ifdef DITHER
                    sampler2D _DitherTex;
                #endif

                    //chaos
                #ifdef CHAOTIC
                    float _Chaos;
                    float _ChaosSnap;
                #endif

                    //proximity clip
                #ifdef PROXIMITY
                    float _ProximityDistance;
                    float _ProximityNoise;
                #endif
                    
                    //inflation
                #ifdef INFLATION
                    float _InflationAmount;
                #endif

                    //quantisation
                #ifdef QUANTISATION
                    float _QuantisationRes;
                #endif

                    inline float snap(float x, float snap)
                    {
                        return snap * floor(x / snap);
                    }

                #if defined(_NOISETYPE_ELASTIC)
                    //randomness and chaos
                    float rand(float2 co)
                    {
                        return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
                    }
                    float3 random3(float3 co)
                    {
                        float tot = co.x + co.y + co.z;
                        return float3(
                            rand(float2(tot, 0)) * 2 - 1,
                            rand(float2(tot + 1, 0)) * 2 - 1,
                            rand(float2(tot + 2, 0)) * 2 - 1
                            );
                    }
                #endif

                #if defined(_NOISETYPE_SMOOTH)
                    float2 unity_gradientNoise_dir(float2 p)
                    {
                        p = p % 289;
                        float x = (34 * p.x + 1) * p.x % 289 + p.y;
                        x = (34 * x + 1) * x % 289;
                        x = frac(x / 41) * 2 - 1;
                        return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
                    }
                    float rand(float2 p)
                    {
                        float2 ip = floor(p);
                        float2 fp = frac(p);
                        float d00 = dot(unity_gradientNoise_dir(ip), fp);
                        float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                        float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                        float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                        fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                        return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
                    }
                    float3 random3(float3 co)
                    {
                        float tot = co.x + co.y + co.z;
                        return float3(
                            rand(float2(tot, 0)),
                            rand(float2(tot + 1, 0)),
                            rand(float2(tot + 2, 0))
                            );
                    }
                #endif

                    v2f vert(appdata_t v)
                    {
                        v2f o;

                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                        //vertex chaos
                    #ifdef CHAOTIC
                        #if defined(_NOISETYPE_ELASTIC)
                        float time = snap(_Time.y, _ChaosSnap);
                        float delta = (_Time.y - snap(_Time.y, _ChaosSnap)) * (1 / _ChaosSnap);

                        float3 currentNoise = random3(v.vertex.xyz + float3(time, time, time)) * _Chaos;
                        float3 nextNoise = random3(v.vertex.xyz + float3(time + _ChaosSnap, time + _ChaosSnap, time + _ChaosSnap)) * _Chaos;
                        float3 smoothNoise = lerp(currentNoise, nextNoise, ease_in_out_back(delta));
                        v.vertex.xyz += mul(unity_WorldToObject, smoothNoise);
                        #endif

                        #if defined(_NOISETYPE_SMOOTH)
                        float time = _Time.y * _ChaosSnap;
                        float3 smoothNoise = random3(v.vertex.xyz + float3(time, time, time)) * _Chaos;
                        v.vertex.xyz += mul(unity_WorldToObject, smoothNoise);
                        #endif
                    #endif

                    #ifdef INFLATION
                        v.vertex.xyz += v.normal * _InflationAmount;
                    #endif

                    #ifdef QUANTISATION
                        v.vertex.xyz = round(v.vertex.xyz * _QuantisationRes) / _QuantisationRes;
                    #endif

                        o.pos = UnityObjectToClipPos(v.vertex);

                        UNITY_TRANSFER_FOG(o,o.vertex);
                        o.worldNormal = UnityObjectToWorldNormal(v.normal);
                        o.viewDir = WorldSpaceViewDir(v.vertex);
                        TRANSFER_SHADOW(o);


                    #ifdef PROXIMITY
                        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    #endif

                    #ifdef DITHER
                        o.screenPos = ComputeScreenPos(o.pos);
                    #endif

                        return o;
                    }

                    fixed4 frag(v2f i) : COLOR
                    {
                        //proximity alpha clip
                    #ifdef PROXIMITY
                        float dist = distance(_WorldSpaceCameraPos, i.worldPos)
                            + (rand(i.worldPos.xy) * 2 - 1) * _ProximityNoise;

                        if (_ProximityDistance < 0)
                            clip(dist + _ProximityDistance);
                        else
                            clip(_ProximityDistance - dist);
                    #endif

                        //default
                        float3 normal = normalize(i.worldNormal);
                        float NdotL = dot(_WorldSpaceLightPos0, normal);
                        float shadow = SHADOW_ATTENUATION(i);
                        float lighting = smoothstep(0, SMOOTHSTEP, NdotL * shadow);

                    #ifdef DITHER
                        fixed4 col = lerp(_ShadowColor, _Color, lighting);
                    #else
                        fixed4 col = lerp(_ShadowColor, _Color, lighting);
                    #endif

                        UNITY_APPLY_FOG(i.fogCoord, col);
                        UNITY_OPAQUE_ALPHA(col.a);

                        return col;
                    }
                ENDCG
            }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
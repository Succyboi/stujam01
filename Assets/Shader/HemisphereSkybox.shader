Shader "SkyboxPlus/Hemisphere"
{
    Properties
    {
        _TopColor("North Pole", Color) = (0.35, 0.37, 0.42)
        _BottomColor("South Pole", Color) = (0.12, 0.13, 0.15)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1
    }
    CGINCLUDE

    #include "UnityCG.cginc"

    half3 _TopColor;
    half3 _BottomColor;
    half _Exposure;

    struct appdata_t {
        float4 vertex : POSITION;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };

    v2f vert(appdata_t v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = v.vertex.xyz;
        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        half t = (i.texcoord.y + 1) / 2;
        half3 c = lerp(_TopColor, _BottomColor, t);
        return half4(c * _Exposure, 1);
    }

    ENDCG
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
    Fallback Off
}

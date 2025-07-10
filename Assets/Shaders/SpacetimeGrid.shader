Shader "Custom/SpacetimeGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  
        _Color ("Grid Color", Color) = (1,1,1,1) 
        _DistortionStrength ("Distortion Strength", Float) = 0.1
        _DistortionSmoothing ("Distortion Smoothing", Float) = 0.1 
        _Tiling ("Tiling", Float) = 10 
        _PlanetCount ("Planet Count", Float) = 0 
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _DistortionStrength;
            float _DistortionSmoothing;
            float _Tiling;
            float _PlanetCount;
            float4 _Planets[4];

            float4 _MainTex_ST; // Auto-passed tiling & offset from material

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = worldPosition.xy;  

                // Apply material's tiling & offset
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; 

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvOffset = float2(0,0);

                for (int p = 0; p < min(int(_PlanetCount), 4); p++) 
                {
                    float2 planetPos = _Planets[p].xy;
                    if (planetPos.x > 9000.0) continue;

                    float2 diff = i.worldPos - planetPos; 
                    float dist = max(length(diff), 0.01);

                    float pull = pow(2.7, -1 * dist * _DistortionSmoothing) * _DistortionStrength;
                    uvOffset -= normalize(diff) * pull; 
                }

                // Apply UV offset for distortion
                fixed4 col = tex2D(_MainTex, i.uv + uvOffset);
                col *= _Color;
                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/Rain2D"
{
    Properties
    {
        [HideInInspector] _MainTex ("Sprite", 2D) = "white" {}

        [Header(Rain)]
        _Color ("Color", Color) = (0.80, 0.96, 0.94, 0.45)
        [IntRange] _Layers ("Layers", Range(1, 3)) = 3
        _Density ("Density", Float) = 2.5
        _Amount ("Amount", Range(0, 1)) = 0.35
        _Speed ("Speed", Float) = 12
        _Length ("Length", Float) = 0.7
        _Thickness ("Thickness", Float) = 0.02
        _Slant ("Slant", Range(-1, 1)) = 0.12
        _Parallax ("Parallax", Range(0, 2)) = 0.6

        [Header(Mist)]
        _MistColor ("Mist Color", Color) = (0.70, 0.92, 0.90, 0.10)
        _MistHeight ("Mist Height", Range(0.05, 1)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _Layers, _Density, _Amount, _Speed, _Length, _Thickness, _Slant, _Parallax;
            fixed4 _MistColor;
            float _MistHeight;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float2 worldPos  : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                fixed4 color     : COLOR;
            };

            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float Hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xy;
                o.screenPos = ComputeScreenPos(o.pos);
                o.color = v.color;
                return o;
            }

            float RainLayer(float2 wp, float scale, float speedMul, float seed, float aaBase)
            {
                float par = _Parallax / scale;
                float2 p = wp - _WorldSpaceCameraPos.xy * (1.0 - par);

                float2 q = float2(p.x + p.y * _Slant, p.y) * scale;

                float col  = floor(q.x * _Density);
                float fx   = frac(q.x * _Density);
                float rCol = Hash11(col + seed * 17.13);

                float y = q.y + _Time.y * _Speed * speedMul * scale * (0.85 + 0.3 * rCol) + rCol * 57.3;

                float cellH = _Length * 3.0;
                float cy    = y / cellH;
                float cell  = floor(cy);
                float fy    = frac(cy);

                float alive = step(Hash12(float2(col + seed * 3.7, cell)), _Amount);
                float xPos  = 0.2 + 0.6 * Hash12(float2(cell + seed * 5.3, col * 1.37));

                float dx   = abs(fx - xPos) / _Density;
                float aa   = aaBase * scale * (1.0 + abs(_Slant));
                float lineMask = saturate(1.0 - (dx - _Thickness * 0.5) / aa);

                float along  = fy * 3.0;
                float streak = smoothstep(0.0, 0.08, along) * saturate(1.0 - along);

                return lineMask * streak * alive;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float aaBase = fwidth(i.worldPos.x);

                float rain = RainLayer(i.worldPos, 1.0, 1.0, 0.0, aaBase);
                if (_Layers > 1.5) rain += RainLayer(i.worldPos, 1.7, 0.75, 1.0, aaBase) * 0.6;
                if (_Layers > 2.5) rain += RainLayer(i.worldPos, 2.8, 0.55, 2.0, aaBase) * 0.35;
                rain = saturate(rain);

                float screenV = i.screenPos.y / i.screenPos.w;
                float mist = _MistColor.a * saturate(1.0 - screenV / _MistHeight);

                float aRain = rain * _Color.a;
                float alpha = mist + aRain - mist * aRain;
                float3 premul = _MistColor.rgb * mist * (1.0 - aRain) + _Color.rgb * aRain;
                float3 rgb = premul / max(alpha, 1e-4);

                return fixed4(rgb, alpha) * i.color;
            }
            ENDCG
        }
    }

    Fallback Off
}

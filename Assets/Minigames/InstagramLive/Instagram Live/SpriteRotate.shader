Shader "Custom/SpriteRotate"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Rotation ("Rotation (Degrees)", Float) = 0
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Center UV to pivot around (0.5, 0.5)
                uv -= 0.5;

                // Rotation in radians
                float rad = radians(_Rotation);
                float cosR = cos(rad);
                float sinR = sin(rad);

                // Rotate UV coordinates
                float2 rotatedUV;
                rotatedUV.x = uv.x * cosR - uv.y * sinR;
                rotatedUV.y = uv.x * sinR + uv.y * cosR;

                // Move UV back to original space
                rotatedUV += 0.5;

                fixed4 col = tex2D(_MainTex, rotatedUV) * _Color;
                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/InstancedIndirectColor" 
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" 
        [NoScaleOffset] _GradientPos ("GradientPos", Float) = 0.14
        [NoScaleOffset] _HardessPos ("HardessPos", Float) = 1.42
        [NoScaleOffset] _TopColor ("TopColor", vector) = (0.6823, 0.8784, 0.2745, 0)
        [NoScaleOffset] _BotColor ("BotColor", vector) = (0.047, 0.3764, 0.0705, 0)

    }
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv : TEXCOORD0; // texture coordinate
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv : TEXCOORD0; // texture coordinate
            }; 

            struct MeshProperties {
                float4x4 mat;
                float4 color;
            };

            StructuredBuffer<MeshProperties> _Properties;

            sampler2D _MainTex;
            float _GradientPos;
            float _HardessPos;
            float4 _TopColor;
            float4 _BotColor;

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                v2f o;

                float4 pos = mul(_Properties[instanceID].mat, i.vertex);

                float posSum = pos.x + pos.z;
                float PosTime = _Time + posSum;


                o.vertex = UnityObjectToClipPos(pos);
                o.color = _Properties[instanceID].color;
                o.uv = i.uv;

                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float TempsUVValue = (i.uv.y + _GradientPos) * _HardessPos;
                float satureValue = saturate(TempsUVValue);
                fixed4 col = lerp(_BotColor, _TopColor, satureValue);
                //col = tex2D(_MainTex, i.uv);
                return col;
            }

            ENDCG
        }
    }
}
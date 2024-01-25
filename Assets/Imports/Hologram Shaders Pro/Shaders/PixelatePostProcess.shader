Shader "Custom/PixelatePostProcess"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _PixelSize("Pixel Size", Range(1, 100)) = 10
    }

        SubShader
        {
            Pass
            {
                Name "Pixelate"
                CGPROGRAM
                #pragma vertex vert_img
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"

                struct PixelateAppData
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };


                sampler2D _MainTex;
                float _PixelSize;

                half4 frag(PixelateAppData i) : SV_Target

                {
                    float2 uv = i.uv;
                    uv = floor(uv * _PixelSize) / _PixelSize;
                    return tex2D(_MainTex, uv);
                }
                ENDCG
            }
        }
}
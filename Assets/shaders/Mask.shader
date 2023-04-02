Shader "Custom/MaskShader" {
    Properties{
        _MainTex("Texture", 2D) = "white" {}
        _StencilRef("Stencil Reference", Range(0, 255)) = 1
    }
    SubShader{
        Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
        LOD 100
        Pass {
            Stencil {
                Ref[_StencilRef]
                Comp always
                Pass replace
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

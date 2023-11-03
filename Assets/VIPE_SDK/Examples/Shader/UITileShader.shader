Shader "Custom/MultiRowTiledShader"
{
    Properties
    {
        _Color ("Image Color", Color) = (1, 1, 1, 1)
        _OffsetX ("Offset X", Range(-10, 10)) = 0.0
        _TilingX ("Tiling X", Range(1, 50)) = 1
        _RowCount ("Row Count", Range(1, 20)) = 5
        _RowMargin ("Row Margin", Range(0, 1)) = 0.1
        _TileMargin ("Tile Margin", Range(0, 1)) = 0.1
        _RowDirection ("Row Direction Multiplier", Range(-2, 2)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _OffsetX;
            float _TilingX;
            float _RowCount;
            float _RowMargin;
            float _TileMargin;
            float _RowDirection;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float rowHeight = 1.0 / _RowCount;
                int currentRow = (int)(i.uv.y / rowHeight);
                float localUVY = frac(i.uv.y / rowHeight);

                if(localUVY < _RowMargin * 0.5 || localUVY > (1.0 - _RowMargin * 0.5))
                {
                    return half4(0,0,0,0);
                }

                float2 uvTiled;
                uvTiled.y = (localUVY - _RowMargin * 0.5) / (1.0 - _RowMargin);
                uvTiled.x = i.uv.x * _TilingX + _OffsetX * (_RowDirection * (currentRow % 2 == 0 ? 1 : -1));

                if(frac(uvTiled.x) < _TileMargin * 0.5 || frac(uvTiled.x) > (1.0 - _TileMargin * 0.5))
                {
                    return half4(0,0,0,0);
                }

                half4 col = tex2D(_MainTex, uvTiled);
                return col * _Color;
            }
            ENDCG
        }
    }
}

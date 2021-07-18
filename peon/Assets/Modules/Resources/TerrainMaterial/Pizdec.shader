Shader "Custom/Pizdec"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [HideInInspector]_MainTex2 ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal", 2D) = "bumb" {}
        [HideInInspector]_BumpMap2 ("Albedo (RGB)", 2D) = "bumb" {}
        _OrmMap ("Orm", 2D) = "white" {}
        [HideInInspector]_OrmMap2 ("Orm", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MainTex2;
        sampler2D _OrmMap;
        sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv3_MainTex2;
            float2 uv_BumpMap;
            float2 uv3_BumpMap2;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 col  = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 colComp = tex2D (_MainTex, IN.uv3_MainTex2);

            float alpha = colComp.a;
            col.r = (1 - alpha) * col.r + alpha * colComp.r;
            col.g = (1 - alpha) * col.g + alpha * colComp.g;
            col.b = (1 - alpha) * col.b + alpha * colComp.b;
            col.a = 1;
            
            half3 n1 = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
            half3 n2 = UnpackNormal(tex2D(_BumpMap, IN.uv3_MainTex2)) ;
            //alpha = 1 - normalize(colComp.rgba);

            n1.xyz = lerp(n1.xyz, n2.xyz * 3, alpha);


            o.Albedo = col.rgb;
            o.Alpha = col.a;
            o.Normal = n2;

            fixed4 orm = tex2D(_OrmMap, IN.uv_MainTex);
            fixed4 orm2 = tex2D(_OrmMap, IN.uv3_MainTex2);

            orm.rgb = normalize(orm.rgb);
            orm2.rgb = normalize(orm2.rgb);
            orm.rgb = lerp(orm.rgb, orm2.rgb, alpha);

            o.Occlusion = orm2.r;
            o.Smoothness = 1 - orm2.g;
            o.Metallic = orm2.b;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

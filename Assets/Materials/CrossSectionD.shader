// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

  Shader "CrossSection/Diffuse" {
    Properties {
      _Color ("Main Color", Color) = (1,1,1,1)
	  _MainTex ("Texture", 2D) = "white" {}
	  
      _SectionColor ("Section Color", Color) = (1,0,0,1)
      _SectionPlane ("SectionPlane (x, y, z)", vector) = (0.707,0,-0.2)
      _SectionPoint ("SectionPoint (x, y, z)", vector) = (0,0,0)
      _ClipOffset ("ClipOffset",float) = 0
    }
    
    
    SubShader {
    	Tags { "RenderType"="Opaque" }
		LOD 200

	   Pass {
         Cull FRONT // cull only front faces
         
         CGPROGRAM 
         #include "UnityCG.cginc"
         #pragma vertex vert
         #pragma fragment frag
         
 		 fixed4 _SectionColor;
 		 fixed3 _SectionPlane;
	     fixed3 _SectionPoint;
	     float _ClipOffset;
  		 
         struct vertexInput {
            float4 vertex : POSITION;
         };
         struct vertexOutput {
            float4 pos : SV_POSITION;
            float3 wpos : TEXCOORD0;
         };
 
         vertexOutput vert(appdata_base input) {
            vertexOutput output;
            output.pos =  UnityObjectToClipPos(input.vertex);
            output.wpos = mul (unity_ObjectToWorld, input.vertex).xyz;
            return output;
         }
 
         float4 frag(vertexOutput input) : COLOR  {
         	if(_ClipOffset -dot((input.wpos - _SectionPoint),_SectionPlane) < 0) discard;
       		return _SectionColor;
         }
         ENDCG  
         
      }  
	      
	      
	      Cull Back
	      
	      CGPROGRAM
	      #pragma surface surf BlinnPhong
	      #pragma debug
	      fixed4 _Color;
	      fixed3 _SectionPlane;
	      fixed3 _SectionPoint;
	      float _ClipOffset;
	      
	      struct Input {
	          float2 uv_MainTex;
	          float3 worldPos;
	      };
	      sampler2D _MainTex;
	      
	      void surf (Input IN, inout SurfaceOutput o) {
	          clip (_ClipOffset -dot((IN.worldPos - _SectionPoint),_SectionPlane));
	          fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	          o.Albedo = tex.rgb * _Color.rgb;
	          o.Alpha = tex.a * _Color.a;
	      }
	      ENDCG

    } 

    Fallback "Diffuse"
  }
#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 1) uniform sampler2D texSampler;

layout(location = 0) in vec4 fragColor;
layout(location = 1) in vec2 fragTexCoord;
layout(location = 2) in vec4 LightColor;
layout(location = 0) out vec4 outColor;

void main() 
{
    if( fragTexCoord.x < 0.0 || fragTexCoord.y < 0.0 ) 
    {
	outColor = fragColor;
    }
    else //texture
    {  
       	vec4 textColor = texture( texSampler, fragTexCoord );
	if( textColor.a <= 0.0 ) //this is a text background
	{ 
	  discard;	  
	}
	else 
	{
	  if( fragColor.a < 0 ) //text rect frame
	  {
	     outColor = textColor; //突出贴图材质
	     outColor.a = 1.0;
	  }
	  else //normal texture
	  {
	    outColor = vec4(clamp( LightColor * textColor, 0, 1 ));//贴图+光照
	    outColor.a = fragColor.a;
          }	
	}
    }  
}
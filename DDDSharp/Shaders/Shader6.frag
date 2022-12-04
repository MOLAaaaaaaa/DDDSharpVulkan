#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 1) uniform sampler2D texSampler;

layout(location = 0) in vec4 fragColor;
layout(location = 1) in vec2 fragTexCoord;
layout(location = 2) in vec4 texLight;
layout(location = 0) out vec4 outColor;

void main() 
{
    if( fragTexCoord.x < 0.0 || fragTexCoord.y < 0.0 )
    {
	outColor = fragColor;
    }
    else 
    {  
	vec4 textColor = texture( texSampler, fragTexCoord );
	outColor = textColor;
    }  
}
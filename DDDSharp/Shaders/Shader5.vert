#version 450
#extension GL_ARB_separate_shader_objects : enable

// last modified by jian , fixed normal attrib, 2018-10-15

struct light_struct
{	
	// type.w >0  enabled this light,else disable it 
	// if type.x == 0, this is a point light
    	vec4 type;	
    	vec4 pos;
	vec4 ambient;
	vec4 diffuse;
	vec4 specular;	
};
struct material_struct
{
   vec4 shininess;   // shiniess.w = 128,
   vec4 emission;    // Ecm   
   vec4 ambient;     // Acm   
   vec4 diffuse;     // Dcm   
   vec4 specular;    // Scm
};
layout(binding = 0) uniform UniformBufferObject 
{
	mat4 model;
	mat4 normal;
	mat4 view;
	mat4 proj;
	vec3 eyepos;
	material_struct material;
	light_struct lights[8];    
} ubo;

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;
layout(location = 2) in vec2 inTexCoord;
layout(location = 3) in vec3 inNormal;

layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec2 fragTexCoord;
layout(location = 2) out vec4 LightColor; //all light

out gl_PerVertex 
{
    vec4 gl_Position;
    float gl_PointSize;
};

void main() 
{
    ////////////////////////////////
	mat4 modelView = ubo.model;	
	mat4 projectView = ubo.proj *ubo.view *ubo.model;
        vec3 N,V,L,H;
	
	//vec4 eyepos = vec4(ubo.eyepos,1.0);
	//convert coordinate y to -y
	vec4 eyepos = vec4(ubo.eyepos.x,-ubo.eyepos.y,ubo.eyepos.z,1.0);

	//convert coordinate y to -y
	vec3 position = vec3(inPosition.x,-inPosition.y,inPosition.z);

	vec3 ECPosition = vec3( modelView * vec4(position,1.0));

	LightColor = vec4(0,0,0,1);

	// draw line or point, do not need calculate light
	if( inNormal.x == 0 && inNormal.y == 0 && inNormal.z == 0 )
	{
		fragColor = inColor;
       		fragTexCoord.x = -1;
		fragTexCoord.y = -1;
		gl_Position = projectView * vec4(position,1.0);
		return;
	}

	//normalize	
	//convert coordinate y to -y
	vec3 normal = vec3(inNormal.x,-inNormal.y,inNormal.z);
	N = normalize( mat3( ubo.normal)  * normal );
	
	//eye -- vertex
	V = normalize( eyepos.xyz - ECPosition );

	vec4 light_pos;
	vec4 light_ambient;
	vec4 light_diffuse;
	vec4 light_specular;

	//Ns and shininess
	float ns = ubo.material.shininess.x;
	float shininess = ubo.material.shininess.w;

	vec4 emission = ubo.material.emission;
	vec4 material_diffuse = ubo.material.diffuse;
	vec4 material_specular = ubo.material.specular;
	vec4 material_ambient = ubo.material.ambient;

	vec4 cal_diffuse,cal_specular,cal_ambient;
	vec4 color = vec4(0,0,0,0);

	for(int i=0;i<8;i++)
	{
		if( ubo.lights[i].type.w > 0)
		{
			//convert coordinate y to -y
			light_pos = vec4(ubo.lights[i].pos.x,-ubo.lights[i].pos.y,ubo.lights[i].pos.z,1);
			light_ambient = ubo.lights[i].ambient;
			light_diffuse = ubo.lights[i].diffuse;
			light_specular = ubo.lights[i].specular;

			// lithgt to vertex
			L = normalize( light_pos.xyz - ECPosition );
			H = normalize(V + L);	

			cal_ambient = (ubo.lights[i].ambient * material_ambient);
			
			cal_diffuse =  light_diffuse * max( 0.0, dot(N, L) );
			cal_diffuse = cal_diffuse * material_diffuse;
			
			cal_specular =  light_specular *  pow( max(dot(N, H), 0.0), shininess ) * ns;	
			cal_specular = cal_specular * material_specular;

			color = color + cal_diffuse + cal_specular + cal_ambient;		
		}
	}	

	LightColor = vec4(clamp( color, 0, 1 ));
	fragColor = LightColor*inColor;
	fragColor.a = inColor.a;

	// -------------debug begin-----------------
	//if( ubo.material.shininess == 128  ) fragColor = inColor;
	//if( ubo.material.emission.x ==1 && ubo.material.emission.y ==2 && ubo.material.emission.z ==3) fragColor = inColor;
	//if( ubo.material.specular.x ==0.5 && ubo.material.specular.y == 0.6 && ubo.material.specular.z == 0.7 && ubo.material.specular.w == 1.0) fragColor = inColor;	
	//if( eyepos.x ==1 && eyepos.y ==2 && eyepos.z ==3) fragColor = inColor;
	//else fragColor = vec4(1,1,1,1);
	// --------debug end-----------------------
	gl_PointSize = 1.0f;
	gl_Position = projectView * vec4(position,1.0);
    	fragTexCoord = inTexCoord;
}
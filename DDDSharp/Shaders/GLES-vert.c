struct project_struct
{
	mat4 model;
	mat4 view;
	mat4 proj;
	mat4 normal;
	vec4 eyepos;
};
struct light_struct
{
	vec4 type;
	vec4 pos;
	vec4 ambient;
	vec4 diffuse;
	vec4 specular;
};

uniform project_struct project;
uniform int lightNum;
uniform light_struct light0;
uniform light_struct light1;
uniform light_struct light2;
uniform light_struct light3;
uniform light_struct light4;
uniform light_struct light5;
uniform light_struct light6;
uniform light_struct light7;

attribute vec4 ma_shiness;	
attribute vec4 ma_diffuse;
attribute vec4 ma_ambient;
attribute vec4 ma_emission;
attribute vec4 ma_specular;

attribute vec4 inColor;
attribute vec3 inNormal;
attribute vec2 inTexCoord;
attribute vec4 inPosition;

varying vec4 fragColor;
varying vec2 fragTexCoord;
varying vec4 LightColor;

void main()
{
	light_struct lights[8];
	mat4 projectView = project.proj * project.view * project.model;
	
	if ( inNormal.x == 0.0 && inNormal.y == 0.0 && inNormal.z == 0.0)
	{
		fragColor = inColor;
		fragTexCoord = inTexCoord;
		gl_Position = projectView * inPosition;
		return;
	}

	vec3 N, V, L, H;
	vec4 eyepos = project.eyepos;
	vec3 ECPosition = vec3(project.model * inPosition);
	N = normalize( inNormal.xyz * mat3(project.normal) );
	V = normalize( eyepos.xyz - ECPosition );

	float ns = ma_shiness.x;
	float shininess = ma_shiness.w;
	bool enableMaterial = false;
	if ( ma_shiness.z > 0.0 )enableMaterial = true;

	vec4 cal_diffuse, cal_specular, cal_ambient;
	vec4 color = vec4(0, 0, 0, 0);
	
	if (light0.type.w > 0.0) lights[0] = light0;
	if (light1.type.w > 0.0) lights[1] = light1;
	if (light2.type.w > 0.0) lights[2] = light2;
	if (light3.type.w > 0.0) lights[3] = light3;
	if (light4.type.w > 0.0) lights[4] = light4;
	if (light5.type.w > 0.0) lights[5] = light5;
	if (light6.type.w > 0.0) lights[6] = light6;
	if (light7.type.w > 0.0) lights[7] = light7;
	
	if ( enableMaterial )
	{
		for (int i = 0; i < lightNum; i++)
		{
			L = normalize(lights[i].pos.xyz - ECPosition);
			H = normalize(V + L);

			cal_ambient = lights[i].ambient * ma_ambient;
			cal_diffuse = lights[i].diffuse * max( 0.0, dot(N, L) ) * ma_diffuse;
			cal_specular = lights[i].specular* ma_specular;
			cal_specular = cal_specular * pow( max(dot(N, H),0.0 ), shininess) * ns;
			color = color + inColor* (cal_diffuse + cal_specular + cal_ambient);
		}
	}
	else
	{
		for (int i = 0; i < lightNum; i++)
		{
			L = normalize(lights[i].pos.xyz - ECPosition);
			H = normalize(V + L);
			cal_ambient = lights[i].ambient;
			cal_diffuse = lights[i].diffuse * max(0.0, dot(N, L));			
			cal_specular = lights[i].specular * pow(max(dot(N, H), 0.0), shininess) * ns;
			color = color + inColor * (cal_diffuse + cal_specular + cal_ambient);
		}
	}
	
	vec4 min = vec4(0.0, 0.0, 0.0, 0.0);
	vec4 max = vec4(1.0, 1.0, 1.0, 1.0);
	LightColor = vec4(clamp(color, 0, 1));

	fragColor = vec4(clamp(color, min, max));
	fragColor.a = inColor.a;

	fragTexCoord = inTexCoord;
	gl_Position = projectView * inPosition;
}
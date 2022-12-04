precision highp float;
uniform sampler2D texSampler;

varying vec4 fragColor;
varying vec2 fragTexCoord;
varying vec4 LightColor;

void main()
{
	
	if (fragTexCoord.x < 0.0 || fragTexCoord.y < 0.0)
	{
		gl_FragColor = fragColor;
	}
	else 
	{
		vec4 textColor = texture(texSampler, fragTexCoord);
		if (textColor.a <= 0.0) 
		{
			discard;
		}
		else
		{
		
			if (fragColor.a < 0)
			{
				gl_FragColor = textColor;
				gl_FragColor.a = 1.0;
			}
			else
			{
				gl_FragColor = vec4(clamp(LightColor * textColor, 0, 1));
				gl_FragColor = fragColor.a;

			}
		}


		
		
	}	
	gl_FragColor = vec4(1, 1, 1, 1);
}
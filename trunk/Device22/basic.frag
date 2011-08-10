// basic.frag
uniform sampler2D Texture0;
uniform sampler2D Texture1;

uniform float weight;
varying vec2 texCoord;
varying float y;

void main(void)
{
	gl_FragColor = texture2D(Texture0, texCoord );
	//gl_FragColor = texture2D(Texture1, vec2(1-y, 1-y))*weight + (1-weight)*gl_Color ;
	//gl_FragColor = vec4(1.0,0.2,0.2,1.0);
}

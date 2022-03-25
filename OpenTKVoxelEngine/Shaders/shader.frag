#version 330 core
out vec4 fragColor;

in vec4 vColor; // The input variable from the vertex shader (same name and type).

void main()
{
	fragColor = vColor;
}
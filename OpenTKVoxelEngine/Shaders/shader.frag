#version 330 core
out vec4 fragColor;

in vec3 ourColor; // The input variable from the vertex shader (same name and type).

void main()
{
	fragColor = vec4(ourColor, 1.0);
}
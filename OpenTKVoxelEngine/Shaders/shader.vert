#version 330 core
layout (location = 0) in vec3 aPosition;

out vec4 vColor; // Specify a color output to the fragment shader.

void main()
{
    gl_Position = vec4(aPosition, 1.0);
	vColor = vec4(1, 0.0, 0.0, 1.0); // red color
}
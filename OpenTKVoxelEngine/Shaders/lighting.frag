#version 330 core
out vec4 FragColor;

uniform vec3 objectColor; // The color of the object.
uniform vec3 lightColor; // The color of the light.
uniform vec3 lightPos; // The position of the light in world space.
uniform vec3 viewPos; // The position of the view and/or of the player.

in vec3 Normal; // The normal of the fragment is calculated in the vertex shader.
in vec3 FragPos; // The fragment position.

void main()
{
	// The ambient color is the color where the light does no directly hit the object.
	float ambientStrengh = 0.1;
	vec3 ambient = ambientStrengh * lightColor;

	// Calculate the light direction, and make sure the normal is normalized.
	vec3 norm = normalize(Normal);
	vec3 lightDir = normalize(lightPos - FragPos);

	// The diffuse part of the phong model.
	// This is the part of the light that gives the most, it is the color of the object where it is hit by the light.
	float diff = max(dot(norm, lightDir), 0.0);
	vec3 diffuse = diff * lightColor;

	// The specular light is the light that shines from the object, like light hitting metal.
	float specularStrengh = 0.5;
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 reflectDir = reflect(-lightDir, norm);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
	vec3 specular = specularStrengh * spec * lightColor;

	vec3 result = (ambient + diffuse + specular) * objectColor;
	FragColor = vec4(result, 1.0);
}
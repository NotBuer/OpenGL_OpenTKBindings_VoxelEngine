#version 330 core

// Material structure.
struct Material 
{
	sampler2D diffuse;
	sampler2D specular;
	float shininess; // Shininess is the power of the specular light is raised to.
};

// Light shininess.
struct Light
{
	vec3 position;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

uniform Light light;
uniform Material material;
uniform vec3 viewPos;

out vec4 FragColor;

in vec3 Normal; // The normal of the fragment is calculated in the vertex shader.
in vec3 FragPos; // The fragment position.

// Now we need the texture coordinates, however we only need one set even though we have 2 textures,
// as every fragment should have the same texture position no matter what texture we are using.
in vec2 TexCoords;

void main()
{
	// Calculate the ambient color.
	vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));

	// Diffuse.
	// Calculate the light direction, and make sure the normal is normalized.
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);

	// The diffuse part of the phong model.
	// This is the part of the light that gives the most, it is the color of the object where it is hit by the light.
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords));

	// The specular light is the light that shines from the object, like light hitting metal.
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    vec3 specular = light.specular * spec * vec3(texture(material.specular, TexCoords)); //Remember to use the material here.

    vec3 result = ambient + diffuse + specular;
    FragColor = vec4(result, 1.0);
}
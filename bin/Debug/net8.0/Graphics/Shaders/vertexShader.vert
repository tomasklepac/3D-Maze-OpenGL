#version 330 core

// === Vertex Attributes ===
layout(location = 0) in vec3 aPosition;   // Vertex position in model space
layout(location = 1) in vec3 aNormal;     // Vertex normal in model space
layout(location = 2) in vec2 aTexCoord;   // Texture coordinates

// === Outputs to Fragment Shader ===
out vec2 vTexCoord;       // UV coordinates (for texture sampling)
flat out vec3 Normal;     // Interpolated world-space normal (flat to reduce artifacts)
out vec3 FragPos;         // World-space position of the fragment
out mat3 TBN;             // Tangent-Bitangent-Normal matrix for normal mapping

// === Uniforms ===
uniform mat4 model;       // Model transform matrix
uniform mat4 view;        // View (camera) matrix
uniform mat4 projection;  // Projection matrix (perspective)
uniform sampler2D heightMap;
uniform float heightScale;

void main()
{
    // === World-space fragment position ===
    vec4 worldPosition = model * vec4(aPosition, 1.0);
    FragPos = vec3(worldPosition);

    // === Transform normal to world space ===
    mat3 normalMatrix = mat3(model); // No transpose because model is rigid
    Normal = normalize(normalMatrix * aNormal);

    // === Pass through texture coordinates ===
    vTexCoord = aTexCoord;

    // === Construct tangent space basis (simple placeholder axes) ===
    // Normally you'd calculate T & B per vertex or per triangle
    vec3 T = vec3(1.0, 0.0, 0.0); // Tangent
    vec3 B = vec3(0.0, 1.0, 0.0); // Bitangent
    vec3 N = normalize(Normal);  // Normal (already transformed)

    TBN = mat3(normalize(T), normalize(B), normalize(N));

    // === Final screen-space position ===
    gl_Position = projection * view * worldPosition;
}

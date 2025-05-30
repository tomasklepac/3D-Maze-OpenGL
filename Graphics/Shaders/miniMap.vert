#version 330 core

// === Vertex Attributes ===
layout(location = 0) in vec2 aPosition;  // 2D position in minimap space
layout(location = 1) in vec3 aColor;     // RGB tile color

// === Output to fragment shader ===
out vec3 vColor;

// === Uniforms ===
uniform mat4 uProjection;               // Orthographic projection with rotation & translation

void main()
{
    // Pass tile color to fragment shader
    vColor = aColor;

    // Apply projection transform
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
}
